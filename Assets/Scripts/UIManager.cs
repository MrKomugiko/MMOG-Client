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
    public static string ConnectingMode = "";
    public void ConnectToServer()
    {
        ConnectingMode = "";
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
    public void LogInToServer()
    {
        ConnectingMode = "LOGIN";
        usernameField.interactable = false;
        Client.instance.ConnectToServer();
    }
      public void RegisterNewAccount()
    {
       
      
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
               // print("usunięcie obiektów graczy");
                if(player.IsLocal)
                {
                    GameManager.instance.cam.transform.parent = GameManager.instance.gameObject.transform;
                    GameManager.instance.cam.transform.localPosition = Vector3.zero;
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

    [SerializeField] TextMeshProUGUI PlayersOnlineText;

        public void PrintCurrentOnlineUsers()
        {
            PlayersOnlineText.SetText("");
            foreach(var player in GameManager.players.Values)
            {
                PlayersOnlineText.SetText(PlayersOnlineText.text + $"\n- [{player.Id}] [{player.Username}]");
            }
        }
}
