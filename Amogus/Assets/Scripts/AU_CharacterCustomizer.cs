using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AU_CharacterCustomizer : MonoBehaviour
{
    [SerializeField] GameObject Panel;

    [SerializeField] Color[] allColors;
    [SerializeField] Sprite[] allHats;

    [SerializeField] GameObject colorPanel;
    [SerializeField] GameObject hatPanel;
    [SerializeField] Button colorTabButton;
    [SerializeField] Button hatTabButton;
    //[SerializeField] Button closeButton;

    public void SetColor(int colorIndex)
    {
        AU_PlayerController.localPlayer.SetColor(allColors[colorIndex]);
    }

    public void SetHat(int hatIndex)
    {
        AU_PlayerController.localPlayer.SetHat(allHats[hatIndex]);
    }

    public void NextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    public void EnableColors()
    {
        colorPanel.SetActive(true);
        hatPanel.SetActive(false);
        colorTabButton.interactable = false;
        hatTabButton.interactable = true;
    }

    public void EnableHats()
    {
        colorPanel.SetActive(false);
        hatPanel.SetActive(true);
        colorTabButton.interactable = true;
        hatTabButton.interactable = false;
    }

    public void SetOffPanel()
    {
        Panel.SetActive(false);
    }
}
