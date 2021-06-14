using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;

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
    public static InputField Login_InputUsername;
    public static InputField Login_InputPassword;
    public static InputField Register_InputUsername;
    public static InputField Register_InputPassword;

    [SerializeField] public GameObject LoadingWindow;
    [SerializeField] public LoadingAnimation LoadingAnimation;
    [SerializeField] public GameObject RegistrationWindow;


    private void Awake()
    {
        czatTMP = czat;
        
        Login_InputUsername = startMenu.transform.Find("Login_InputUsername").GetComponent<InputField>();
        Login_InputPassword = startMenu.transform.Find("Login_InputPassword").GetComponent<InputField>();
        Register_InputPassword = RegistrationWindow.transform.Find("Register_InputPassword").GetComponent<InputField>();
        Register_InputUsername = RegistrationWindow.transform.Find("Register_InputUseranem").GetComponent<InputField>();
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

    public void ClearMessage(float time, TextMeshProUGUI responsMessage)
    {
        StartCoroutine(ClearMessageAfterTime(time,responsMessage));
    }

    public void EnterAsGuest()
    {
        print("Gracz chce wejsc do gry jako gość");
        ThreadManager.ExecuteOnMainThread(()=>UIManager.instance.LoadGameScene());
        ClientSend.WelcomeReceived();
    }
    public void LogInToServer()
    {
        Login_InputUsername.interactable = false;
        // Client.instance.ConnectToServer();
        print("sprawdzanie danych do logowania poczekaj");
        print("Gracz chce sie zalogowac - wysłanie do sprawdzenia pary nicku ( wpisanego + z pamięci, hasło )");
        // po rejestracji, haslo zapisze sie na urządzeniu i bedzie wysylane razem z niskiem w komplecie?
        ClientSend.SendLoginCreditionals(UIManager.Login_InputUsername.text, UIManager.Register_InputPassword.text, "LOGIN");
    }
      public void RegisterNewAccount()
    {
        LoadingWindow.SetActive(true);

        // ThreadManager.ExecuteOnMainThread(()=>UIManager.instance.EnterGame());
        print("Gracz chce stworzyc nowe konto");
        ClientSend.SendLoginCreditionals(UIManager.Register_InputUsername.text, UIManager.Register_InputPassword.text,"REGISTER");
    }
 

    public void LoadGameScene()
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


    private IEnumerator ClearMessageAfterTime(float time, TextMeshProUGUI textTMP)
    {
        print("za "+time+" sekund zniknie wiadomosc");
        yield return new WaitForSeconds(time);
        textTMP.SetText("");
    }
}
