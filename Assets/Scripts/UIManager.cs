using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI czat;
    [SerializeField] private GameObject updateAndMapVersion;
    public static UIManager instance;
    public static TextMeshProUGUI czatTMP;
    
    public GameObject startMenu;
    public GameObject buttonsPanel;
    public GameObject czatPanel;
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
        print("Aktywowanie sceny gry / Enter Game");
        startMenu.SetActive(false);
        buttonsPanel.SetActive(true);
        czatPanel.SetActive(true);
        grid.SetActive(true);
    }
    public void BackToStartScreen()
    {
        print("Powrót na strone główną");
        foreach (PlayerManager player in GameManager.players.Values)
        {
            try
            {
                //print("usuniecie tilesów graczy");
                GameManager.instance._tileMap.SetTile(player.CurrentPosition_GRID, null);
            }
            catch { }
            try
            {
               // print("usunięcie obiektów graczy");
                if(player.IsLocal)
                {
                    var camera = GameObject.Find("Main Camera").gameObject;
                    camera.transform.parent = null;
                    camera.transform.localPosition = Vector3.zero;
                }
                Destroy(player.gameObject);
            }
            catch { }
        }
      //  print("usuniecie graczy z pamieci");
        GameManager.players.Clear();

        grid.SetActive(false);
        buttonsPanel.SetActive(false);
        czatPanel.SetActive(false);
        startMenu.SetActive(true);
    }

    public void UpdateBuildIndicatorOnScreen(int _currentBuildVersion = 0000, bool _isDownloadAvaiable = false)
    {
        // change text
        updateAndMapVersion.transform.Find("text_buildVersion").GetComponent<TextMeshProUGUI>().SetText($"Build ver. {_currentBuildVersion}");

        //activate or no button
        updateAndMapVersion.GetComponent<Button>().interactable = _isDownloadAvaiable;
    }
}
