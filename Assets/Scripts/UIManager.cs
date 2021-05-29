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
    public static TextMeshProUGUI czatTMP;
    [SerializeField] private TextMeshProUGUI czat;

    public GameObject grid;

    public InputField usernameField;

    private void Awake()
    {
        czatTMP = czat;
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
        //TODO: wygląd gridu powinien być na starcieściągnięty z serwera jednorazowo jako metoda wczytywania mapy
        grid.SetActive(true);
    }

    public void BackToStartScreen()
    {

        foreach (PlayerManager player in GameManager.players.Values)
        {
            try
            {
                print("usuniecie tilesów graczy");
                GameManager.instance._tileMap.SetTile(player.CurrentPosition_GRID, null);
            }
            catch { }
            try
            {
                print("usunięcie obiektów graczy");
                Destroy(player.gameObject);
            }
            catch { }
        }
        print("usuniecie graczy z pamieci");
        GameManager.players.Clear();

        print("powrót na strone główną");
        grid.SetActive(false);
        buttonsPanel.SetActive(false);
        czatPanel.SetActive(false);
        startMenu.SetActive(true);
    }
}
