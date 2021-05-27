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
        usernameField.interactable = false;
        Client.instance.ConnectToServer();        
    }

    public void EnterGame()
    {
        startMenu.SetActive(false);
        buttonsPanel.SetActive(true);
        czatPanel.SetActive(true);    
    }
    
    public void BackToStartScreen()
    {

        try{
            print("usunięcie obiektów graczy");
            foreach(PlayerManager player in GameManager.players.Values)
            {
                Destroy(player.gameObject);
            }
        }catch{}
        GameManager.players.Clear();
        
        print("powrót na strone główną");
        startMenu.SetActive(true);
        buttonsPanel.SetActive(false);
        czatPanel.SetActive(false);   
    }
}
