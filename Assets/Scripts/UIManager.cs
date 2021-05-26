using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public GameObject startMenu;
    public GameObject buttonsPanel;
    public GameObject czatPanel;
    public static TextMeshPro czatTMP;

    public InputField usernameField;

    private void Awake()
    {
        czatTMP = czatPanel.GetComponentInChildren<TextMeshPro>();
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void ConnectToServer()
    {
        startMenu.SetActive(false);
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
        buttonsPanel.SetActive(true);
        czatPanel.SetActive(true);
    }
}
