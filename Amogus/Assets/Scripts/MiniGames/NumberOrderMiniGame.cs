using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberOrderMiniGame : MonoBehaviour
{
    [SerializeField] int nextButton;
    [SerializeField] GameObject GamePanel;
    [SerializeField] GameObject[] myObjects;

    // Start is called before the first frame update
    void Start()
    {
        nextButton = 0;
    }

    private void OnEnable()
    {
        nextButton = 0;
        for (int i = 0; i < myObjects.Length; i++)
        {
            myObjects[i].transform.SetSiblingIndex(Random.Range(0, 9));
            //transform.SetSiblingIndex(Random.Range(0, 9));
        }
    }

    public void ButtonOrder(int button)
    {
        Debug.Log("Pressed");
        if (button == nextButton)
        {
            nextButton++;
            Debug.Log("Next Button: " + nextButton);
            if (button == 9)
            {
                Debug.Log("Successfully!");
                nextButton = 0;
                ButtonOrderPanelClose();
            }
        }
        else
        {
            Debug.Log("Failed");
            Debug.Log("Next Button: " + nextButton);
            nextButton = 0;
            OnEnable();
        }
        
        //if (button == 9)
        //{
        //    if (nextButton == 10)
        //    {
        //        Debug.Log("Pass");
        //        nextButton = 0;
        //        ButtonOrderPanelClose();
        //    }
        //}
    }

    public void ButtonOrderPanelClose()
    {
        GamePanel.SetActive(false);
    }

    public void ButtonOrderPanelOpen()
    {
        GamePanel.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
