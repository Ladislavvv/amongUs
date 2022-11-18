using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public class AU_PlayerController : MonoBehaviour, IPunObservable
{
    [SerializeField] bool hasControl;
    public static AU_PlayerController localPlayer;

    Rigidbody myRb;
    Transform myAvatar;
    Animator myAnim;

    //Управление персонажем
    [SerializeField] InputAction WASD;
    Vector2 movementInput;
    [SerializeField] float movementSpeed;

    float direction = 1;

    // Смена цвета скина
    static Color myColor;
    SpriteRenderer myAvatarSprite;

    //Player Hat
    static Sprite myHatSprite;
    SpriteRenderer myHatHolder;

    // Роли игроков
    [SerializeField] bool isImposter;
    [SerializeField] InputAction KILL;

    //float killInput // Почему закоменчено???
    
    List<AU_PlayerController> targets;

    AU_PlayerController target;
    [SerializeField] Collider myCollider;

    bool isDead;

    [SerializeField] GameObject bodyPrefab;

    // репорт тела
    public static List<Transform> allBodies;

    List<Transform> bodiesFound;

    [SerializeField] InputAction REPORT;
    [SerializeField] LayerMask ignoreForBody;

    //Interaction
    [SerializeField] InputAction MOUSE;
    Vector2 mousePositionInput;
    Camera myCamera;
    [SerializeField] InputAction INTERACTION;
    [SerializeField] LayerMask interactLayer;

    //Networking
    PhotonView myPV;
    [SerializeField] GameObject Light2D;
    //[SerializeField] lightcaster myLightCaster;


    private void Awake()
    {
        KILL.performed += KillTarget;
        REPORT.performed += ReportBody;
        INTERACTION.performed += Interact;
    }

    private void OnEnable() 
    {
        WASD.Enable();
        KILL.Enable();
        REPORT.Enable();
        MOUSE.Enable();
        INTERACTION.Enable();
    }

    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
        REPORT.Disable();
        MOUSE.Disable();
        INTERACTION.Disable();
    }
    

    // Start is called before the first frame update
    void Start()
    {
        myPV = GetComponent<PhotonView>();

        if(myPV.IsMine)
        {
            localPlayer = this;
        }
        myCamera = transform.GetChild(2).GetComponent<Camera>();
        targets = new List<AU_PlayerController>();
        myRb = GetComponent<Rigidbody>();
        myAvatar = transform.GetChild(0);
        myAnim = GetComponent<Animator>();
        myAvatarSprite = myAvatar.GetComponent<SpriteRenderer>();
        myHatHolder = myAvatar.GetChild(1).GetComponent<SpriteRenderer>();
        if (!myPV.IsMine)
        {
            myCamera.gameObject.SetActive(false);
            Light2D.SetActive(false);
            // myLightCaster.enabled = false;
            return;
        }

        if (myColor == Color.clear)
            myColor = Color.white;
        myAvatarSprite.color =  myColor;


        //allBodies = new List<Transform>();
        if (allBodies == null)
        {
            allBodies = new List<Transform>();
        }

        bodiesFound = new List<Transform>();

        if (myHatSprite != null)
            myHatHolder.sprite = myHatSprite;
    }

    // Update is called once per frame
    void Update()
    {
        myAvatar.localScale = new Vector2(direction, 1);// поворот скина сихронизация

        if (!myPV.IsMine)
            return;

        // включаем анимацию бега при движении
        movementInput = WASD.ReadValue<Vector2>();
        myAnim.SetFloat("Speed", movementInput.magnitude);
        if (movementInput.x != 0)
        {
            // поворачиваем спрайт на 180* если бежим в другую сторону
            //myAvatar.localScale = new Vector2(Mathf.Sign(movementInput.x), 1);
            direction = Mathf.Sign(movementInput.x);
        }

        // если кл-во тел > 0, то включаем поиск тела
        if(allBodies.Count > 0)
        {
            BodySearch();
        }

        /////
        /////
        /////не показал
        if (REPORT.triggered)
        {
            if (bodiesFound.Count == 0)
                return;
            Transform tempBody = bodiesFound[bodiesFound.Count - 1];
            allBodies.Remove(tempBody);
            bodiesFound.Remove(tempBody);
            tempBody.GetComponent<AU_Body>().Report();
        }
        ////////
        ////////
        ////////


        mousePositionInput = MOUSE.ReadValue<Vector2>();
    }

    private void FixedUpdate() 
    {
        if (!myPV.IsMine)
            return;
        myRb.velocity = movementInput * movementSpeed;
    }

    public void SetColor(Color newColor)
    {
        myColor = newColor;
        if(myAvatarSprite != null)
        {
            myAvatarSprite.color = myColor;
        }
    }

    public void SetHat(Sprite newHat)
    {
        myHatSprite = newHat;
        myHatHolder.sprite = myHatSprite;
    }

    public void SetRole(bool newRole)
    {
        isImposter = newRole;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if(isImposter)
            {
                if (tempTarget.isImposter)
                    return;
                else
                {
                    targets.Add(tempTarget);
                    //target = tempTarget;
                    //Debug.Log(target.name);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if (targets.Contains(tempTarget))
            {
                targets.Remove(tempTarget);
            }
        }
    }

    void KillTarget(InputAction.CallbackContext context)
    {
        if (!myPV.IsMine)
            return;
        if (!isImposter)
            return;

        if (context.phase == InputActionPhase.Performed)
        {
            if (targets.Count == 0)
                return;
            else
            {
                if (targets[targets.Count-1].isDead)
                    return;
                transform.position = targets[targets.Count - 1].transform.position;
                //targets[targets.Count - 1].Die();
                targets[targets.Count - 1].myPV.RPC("RPC_Kill", RpcTarget.All);
                targets.RemoveAt(targets.Count - 1);
            }
        }
    }

    [PunRPC]
    public void RPC_Kill()
    {
        Die();
    }


    private void Die()
    {
        if (!myPV.IsMine)
            return;

        //AU_Body tempBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<AU_Body>();
        AU_Body tempBody = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "AU_Body"), transform.position, transform.rotation).GetComponent<AU_Body>();
        tempBody.SetColor(myAvatarSprite.color);

        isDead = true;

        myAnim.SetBool("isDead", isDead);
        gameObject.layer = 9;
        myCollider.enabled = false;
    }

    void BodySearch()
    {
        foreach(Transform body in allBodies)
        {
            RaycastHit hit;
            Ray ray = new Ray(transform.position, body.position - transform.position);
            Debug.DrawRay(transform.position, body.position - transform.position, Color.cyan);
            if(Physics.Raycast(ray, out hit, 1000f, ~ignoreForBody))
            {

                if(hit.transform == body)
                {
                    Debug.Log(hit.transform.name);
                    Debug.Log(bodiesFound.Count);
                    if (bodiesFound.Contains(body.transform))
                        return;
                    bodiesFound.Add(body.transform);
                }
                else
                {
                    bodiesFound.Remove(body.transform);
                }
            }
        }
    }

    //ReportBody он переделал, ее тут нет, на в Start() вроде 
    private void ReportBody(InputAction.CallbackContext obj)
    {
        if (bodiesFound == null)
            return;
        if (bodiesFound.Count == 0)
            return;
        Transform tempBody = bodiesFound[bodiesFound.Count - 1];
        allBodies.Remove(tempBody);
        bodiesFound.Remove(tempBody);
        tempBody.GetComponent<AU_Body>().Report();
    }

    void Interact(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            //Debug.Log("Here");
            RaycastHit hit;
            Ray ray = myCamera.ScreenPointToRay(mousePositionInput);
            if (Physics.Raycast(ray, out hit, interactLayer))
            {
                if (hit.transform.tag == "Interactable")
                {
                    if (!hit.transform.GetChild(0).gameObject.activeInHierarchy)
                        return;
                    AU_Interactable temp = hit.transform.GetComponent<AU_Interactable>();
                    temp.PlayMiniGame();
                }
            }
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //throw new System.NotImplementedException();
        if (stream.IsWriting)
        {
            stream.SendNext(direction);
            stream.SendNext(isImposter);
        }
        else
        {
            //direction = (float)stream.ReceiveNext();
            this.direction = (float)stream.ReceiveNext();
            this.isImposter = (bool)stream.ReceiveNext();
        }
    }

    public void BecomeImposter(int ImposterNumber)
    {
        Debug.Log("In AU_PC BecomeImposter,ImposterNumber: " + ImposterNumber);
        if(PhotonNetwork.LocalPlayer == PhotonNetwork.PlayerList[ImposterNumber - 1])
        {
            Debug.Log("PlayerList(ImposterNumber):" + ImposterNumber);
            isImposter = true;
            Debug.Log("isImposter activated");
        }
    }
}
