using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardSwipe : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float timeSpend;
    public float deltaX;

    private bool isDragging;
    private Vector3 oldPosition;

    [SerializeField] GameObject Panel;

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isDragging)
        {
            Vector3 point = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            gameObject.transform.position = new Vector3(transform.position.x + point.x, transform.position.y, transform.position.z);
        }
        else
        {
            if (transform.position.x > 3.5f)
            {
                print(transform.position.x);
                gameObject.transform.position += new Vector3(-6f * Time.deltaTime, 0, 0);
            }
        }

        if (!isDragging)
        {
            if (transform.position.x > 3.5f)
            {
                transform.position += new Vector3(-6f * Time.deltaTime, 0, 0);
            }
        }

        Vector3 cPos = transform.position;
        cPos.x = Mathf.Clamp(cPos.x, 3.5f, 11.8f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        timeSpend = 0;
        deltaX = eventData.position.x - transform.position.x;
        if (transform.position.x > 3.5f)
        {
            print("Ну удалось считать");
        }
        else
        {
            isDragging = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        print(timeSpend);
        
        if (transform.position.x > 11.6f)
        {
            if (timeSpend > 1.2f)
            {
                print("Долго");
            }
            if (timeSpend > 0.7f)
            {
                print("Быстро");
            }
            if (timeSpend < 1.2f && timeSpend > 0.7f)
            {
                print("Успешно!");
            }
        }
        else
        {
            isDragging = true;
        }
    }

    private void OnMouseDrag()
    {
        timeSpend += Time.deltaTime;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x - deltaX, transform.position.y, transform.position.z);
    }

    private void OnMouseDown()
    {
        timeSpend = 0;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        isDragging = true;
    }

    private void OnMouseUP()
    {
        timeSpend = 0;
        isDragging = false;
    }

    public void ClosePanel()
    {
        Panel.SetActive(false);
    }
}
