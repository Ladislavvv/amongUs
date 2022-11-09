using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class AU_PlayerController : MonoBehaviour
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
    }
    

    // Start is called before the first frame update
    void Start()
    {
        if(hasControl)
        {
            localPlayer = this;
        }
        // myCamera = transform.GetChild(1).GetComponent<Camera>();
        targets = new List<AU_PlayerController>();
        myRb = GetComponent<Rigidbody>();
        myAvatar = transform.GetChild(0);
        myAnim = GetComponent<Animator>();
        myAvatarSprite = myAvatar.GetComponent<SpriteRenderer>();
        myHatHolder = myAvatar.GetChild(1).GetComponent<SpriteRenderer>();
        if (!hasControl)
            return;
        if (myColor == Color.clear)
            myColor = Color.white;
        myAvatarSprite.color =  myColor;


        allBodies = new List<Transform>();

        bodiesFound = new List<Transform>();

        if (myHatSprite != null)
            myHatHolder.sprite = myHatSprite;
    }

    // Update is called once per frame
    void Update()
    {
        if (!hasControl)
            return;

        // включаем анимацию бега при движении
        movementInput = WASD.ReadValue<Vector2>();
        myAnim.SetFloat("Speed", movementInput.magnitude);
        if (movementInput.x != 0)
        {
            // поворачиваем спрайт на 180* если бежим в другую сторону
            myAvatar.localScale = new Vector2(Mathf.Sign(movementInput.x), 1);
        }

        // если кл-во тел > 0, то включаем поиск тела
        if(allBodies.Count > 0)
        {
            BodySearch();
        }

        mousePositionInput = MOUSE.ReadValue<Vector2>();
    }

    private void FixedUpdate() 
    {
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
        if(context.phase == InputActionPhase.Performed)
        {
            if (targets.Count == 0)
                return;
            else
            {
                if (targets[targets.Count-1].isDead)
                    return;
                transform.position = targets[targets.Count - 1].transform.position;
                targets[targets.Count - 1].Die();
                targets.RemoveAt(targets.Count - 1);
            }
        }
    }

    private void Die()
    {
        isDead = true;

        myAnim.SetBool("isDead", isDead);
        myCollider.enabled = false;
        gameObject.layer = 9;

        AU_Body tempBody = Instantiate(bodyPrefab, transform.position, transform.rotation).GetComponent<AU_Body>();
        tempBody.SetColor(myAvatarSprite.color);
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
                    //Debug.Log(hit.transform.name);
                    //Debug.Log(bodiesFound.Count);
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
}
