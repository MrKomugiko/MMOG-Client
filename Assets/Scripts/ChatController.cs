using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    private ChatController instance;
    [SerializeField] private TMP_InputField message_InputField;
    [SerializeField] private Button send_Button;


    private void Awake()
    {
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

    public void OnClick_SendMessage()
    {
        string _message = message_InputField.text;
        int _playerId = Client.instance.myId;
        string _username = GameManager.players[_playerId].Username;
        string _time=DateTime.Now.ToShortTimeString();

        print($"Wiadomosc na czacie powinna wyglądać następująco"+
              $"[{_time}]:[{_username}]:{_message}");

        //Clear input field;
        message_InputField.text = "";

        ClientSend.SendChatMessage(_message);
    }
}
