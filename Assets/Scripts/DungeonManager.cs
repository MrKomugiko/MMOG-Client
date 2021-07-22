
using System.Xml;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Globalization;
using System.Data.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DungeonManager : MonoBehaviour
{
    private const int MAXROOMIDVALUE = 1_000_000;

    [SerializeField] private GameObject MainSelectDungeonButton_Prefab;
    [SerializeField] private WindowScript MainWindow;
    [SerializeField] private GameObject DungeonsListContainer;
    [SerializeField] private GameObject RoomButton_Prefab;
    [SerializeField] private WindowScript SelectionWindow;
    [SerializeField] private GameObject RoomsListContainer;
    [SerializeField] private WindowScript WaitingLobbyRoomWindow;

    public static List<DungeonsLobby> ListOfDungeonLobby = new List<DungeonsLobby>();
    public List<GameObject> ListOfDungeonLobby_GameObject;
    public List<GameObject> ListOfDungeonMainPages_GameObject;
    public static DungeonManager instance;

    public static DUNGEONS CurrentScrollingDungeonCategory = 0;

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
        ListOfDungeonLobby_GameObject = new List<GameObject>();
    }
    public void Load() 
    {
        if(GameManager.GetLocalPlayer() != null)
        {
            // Create list of avaiable dungeons
            string[]listaDungeonow = Enum.GetNames(typeof(DUNGEONS));
            DUNGEONS dungeon;
            foreach(string dungeon_string in listaDungeonow)
            {
                Enum.TryParse<DUNGEONS>(dungeon_string,out dungeon);
                CreateMainDungeonChannelButton(dungeon);
            }       
            
            ClientSend.GetCurrentDungeonLobbysData(dungeon = default); 
        }
    }
    private void CreateMainDungeonChannelButton(DUNGEONS dungeonType)
    {
        if(ListOfDungeonMainPages_GameObject.Where(o=>o.transform.name == dungeonType.ToString()).Any()) return;

        GameObject dungeonObject = Instantiate(MainSelectDungeonButton_Prefab,Vector3.zero,Quaternion.identity,DungeonsListContainer.transform);
        
        dungeonObject.name = dungeonType.ToString();
        dungeonObject.GetComponentInChildren<TextMeshProUGUI>().SetText(dungeonType.ToString());
        
        dungeonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        dungeonObject.GetComponent<Button>().onClick.AddListener(()=>OnClick_OpenDungeonLobbysWindow(dungeonType));

        ListOfDungeonMainPages_GameObject.Add(dungeonObject);
    }
    private void OnClick_OpenDungeonLobbysWindow(DUNGEONS dungeonType)
    {
        MainWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().onClick.RemoveAllListeners();
        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().onClick.AddListener(()=>OnClick_CreateAndJoinNewLobby(dungeonType, Client.instance.myId));
        ClientSend.GetCurrentDungeonLobbysData(dungeonType);
        SelectionWindow.gameObject.transform.Find("ContentText").GetComponent<TextMeshProUGUI>().SetText(CurrentScrollingDungeonCategory.ToString());
    }
    public void UpdateLobbyData(int _lobbyId, DungeonsLobby newRoomData)
    {
        // sprawdzenie czy obiekt juz istnieje na scenie:
        DungeonsLobby room = null; 
        if(ListOfDungeonLobby != null)
        {
            if(ListOfDungeonLobby.Count>0)
            {
                room = ListOfDungeonLobby.Where(l=>l.LobbyID == _lobbyId).FirstOrDefault();
            }
        }

        if(room != null)
        {
            room = newRoomData;

            GameObject sceneReference = room.lobbySceneObjectRefference;
            sceneReference.GetComponent<roomScript>().AssignRoomDataToWindow(room);
            ConfigureRoomLabel(sceneReference, room);
            ConfigureRoomContentPlayerList(room);
        }
        else
        {   
            if(ListOfDungeonLobby_GameObject.Where(o=>o.name == "ROOM_"+newRoomData.LobbyID).Any()) 
            {
                print("istnieje juz taki obiekt na");
                return;
            }
            GameObject roomObject = Instantiate(RoomButton_Prefab,Vector3.zero,Quaternion.identity,RoomsListContainer.transform);
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.name = "ROOM_"+newRoomData.LobbyID;

            ListOfDungeonLobby_GameObject.Add(roomObject);           
            ListOfDungeonLobby.Add(newRoomData);
            // edycja danych
            ConfigureRoomLabel(roomObject,newRoomData);     
            
            roomObject.GetComponent<roomScript>().AssignRoomDataToWindow(newRoomData    );
        }
    }
    private void ConfigureRoomContentPlayerList(DungeonsLobby room)
    {
        TextMeshProUGUI textUserListInLobby = WaitingLobbyRoomWindow.gameObject.transform.Find("ContentText").GetComponent<TextMeshProUGUI>();

        textUserListInLobby.SetText(
        $"Players ready for adventure:\n\n" +
        $"   * {room.LobbyOwner} [Leader]\n"
        );

        foreach (string player in room.Players)
        {
            if (player == room.LobbyOwner) continue;
            textUserListInLobby.SetText($"{textUserListInLobby.text}   * {player}\n");
        }
    }
    private void ConfigureRoomLabel(GameObject roomLabelObject, DungeonsLobby newRoomData)
    {
        GameObject sceneReference = newRoomData.lobbySceneObjectRefference;
        var title = sceneReference.transform
                .Find("roomMembersCount")
                .GetComponent<TextMeshProUGUI>();

        title.SetText(newRoomData.PlayersCount+" / "+newRoomData.MaxPlayersCapacity);
        
        roomLabelObject.transform.Find("roomName").GetComponent<TextMeshProUGUI>()
            .SetText("#"+newRoomData.LobbyOwner);

        if(newRoomData.IsFull)
        {
            sceneReference.GetComponent<Button>().interactable = false;
            title.SetText($"<color=yellow>{title.text} [FULL]</color>");
        }
        else
            sceneReference.GetComponent<Button>().interactable = true;
        
        if(newRoomData.IsStarted)
        {
            sceneReference.GetComponent<Button>().interactable = false;
            title.SetText($"<color=red>{title.text} [IN GAME]</color>");
        }
        roomLabelObject.GetComponent<Button>().onClick.RemoveAllListeners();
        roomLabelObject.GetComponent<Button>().onClick.AddListener(()=>OnClick_JoinToRoom(newRoomData.LobbyID, Client.instance.myId));
    }
    private void OnClick_CreateAndJoinNewLobby(DUNGEONS dungeonType, int clientId)
    {
        LOCATIONS location;
        Enum.TryParse<LOCATIONS>(dungeonType.ToString(),out location);

        PlayerManager player_leader = GameManager.players[clientId];
        var newLobby = new DungeonsLobby(UnityEngine.Random.Range(0, MAXROOMIDVALUE), player_leader.Username, dungeonType);
        print("utworzono lobby o id = "+newLobby.LobbyID);

        player_leader.dungeonRoom = newLobby;
        ClientSend.CreateNewDungeonLobby(location, newLobby.LobbyID); 

        UpdateLobbyData(newLobby.LobbyID, newLobby);

        // Edit text containing description and current users init (wpis z nickiem leadera)
        WaitingLobbyRoomWindow.gameObject.transform.Find("ContentText").GetComponent<TextMeshProUGUI>().SetText(
            $"Players ready for adventure:\n\n"+
            $"   * {player_leader.Username} [Leader]\n"
        );

        // Open waiting room
        SelectionWindow.OnClick_Close();
        WaitingLobbyRoomWindow.OnClick_OpenCloseWindow(WaitingLobbyRoomWindow.gameObject);
      
        // button cancel -> usuniecie rekordu lobby z bazy 
        var enteButton = WaitingLobbyRoomWindow.gameObject.transform.Find("EnterDungeonButton").GetComponent<Button>();
            enteButton.onClick.RemoveAllListeners();
            enteButton.gameObject.SetActive(true);
            enteButton.onClick.AddListener(()=>OnClick_StartAndEnterDungeon(dungeonType, newLobby.LobbyID));

        var cancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("CancelDungeonButton").GetComponent<Button>();
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(()=>OnClick_CancelAndRemoveLobby(dungeonType, newLobby.LobbyID));
            cancelButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cancel");

        var backAndCancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("Close").GetComponent<Button>();
            backAndCancelButton.onClick.RemoveAllListeners();
            backAndCancelButton.onClick.AddListener(()=>OnClick_CancelAndRemoveLobby(dungeonType, newLobby.LobbyID));
    }
    private void OnClick_CancelAndRemoveLobby(DUNGEONS dungeonType, int roomId)
    {
        WaitingLobbyRoomWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
        DiscposeRoom(dungeonType,roomId);
    }
    public void DiscposeRoom(DUNGEONS dungeonType, int roomId)
    {
        DungeonsLobby lobbyToRemove = GetDungeonLobbyByRoomID(roomId);

        int? roomObjectIndex = null;
        // sprawdzenie czy pokoj istnieje
        if(lobbyToRemove!=null)
        {
            if(lobbyToRemove.Players.Contains(GameManager.players[Client.instance.myId].Username))
            {
                GameManager.players[Client.instance.myId].dungeonRoom = null;
            }
            ClientSend.RemoveExistingDungeonLobby(lobbyToRemove);
            ListOfDungeonLobby.Remove(lobbyToRemove);
        }


        roomObjectIndex = ListOfDungeonLobby_GameObject
            .IndexOf(ListOfDungeonLobby_GameObject
            .Where(room=>room.name == $"ROOM_{roomId}")
            .FirstOrDefault());

        if(roomObjectIndex != null)
        {
            try
            {
                Destroy(ListOfDungeonLobby_GameObject[(int)roomObjectIndex]);
                ListOfDungeonLobby_GameObject.RemoveAt((int)roomObjectIndex);
            }
            catch (System.Exception)
            {
               // Debug.LogError("Error ?");
            }
        }
    }
    private void OnClick_StartAndEnterDungeon(DUNGEONS dungeonType, int lobbyID)
    {   
        WaitingLobbyRoomWindow.OnClick_Close();

        LOCATIONS location;
        Enum.TryParse<LOCATIONS>(dungeonType.ToString(),out location);
        
        
        ClientSend.GroupTeleportPlayersInRoom(location, lobbyID);
        ClientSend.GroupEnteredDungeon(lobbyID);
    }
   
    public void BackToTown(int LobbyId)
    {
        print("back to town executing");
        ClientSend.GroupLeaveTeleport(LobbyId);
    }

    private void OnClick_JoinToRoom(int roomId, int clientId)
    {
        PlayerManager playerWhoWantJoin = GameManager.players[clientId];
        var existingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        UpdateLobbyData(existingLobby.LobbyID, existingLobby);

        if(playerWhoWantJoin.Username != existingLobby.LobbyOwner)
        {
            // tylko lider moze wystartowac gre w swoim pokoju
            WaitingLobbyRoomWindow.gameObject.transform.Find("EnterDungeonButton").transform.gameObject.SetActive(false);
            var cancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("CancelDungeonButton").GetComponent<Button>();
            cancelButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Leave lobby");
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(()=>LeaveRoom(roomId,clientId));

            ClientSend.JoinLobby(existingLobby);
            playerWhoWantJoin.dungeonRoom = existingLobby;
        }

        SelectionWindow.OnClick_Close();
        WaitingLobbyRoomWindow.OnClick_OpenCloseWindow(WaitingLobbyRoomWindow.gameObject);
    }
    public void LeaveRoom(int roomId, int clientId)
    {
        PlayerManager playerWhoWantLeaveRoom = GameManager.players[clientId];
        var leavingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        ClientSend.LeaveLobbyRoom(leavingLobby);
        playerWhoWantLeaveRoom.dungeonRoom = null;

        leavingLobby.Players.Remove(playerWhoWantLeaveRoom.Username);
        UpdateLobbyData(leavingLobby.LobbyID, leavingLobby);

        WaitingLobbyRoomWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
    }
   public static DungeonsLobby GetDungeonLobbyByRoomID(int _roomID) 
   {
        if(DungeonManager.ListOfDungeonLobby.Count>0)
        {
            return DungeonManager.ListOfDungeonLobby.Where(room=>room.LobbyID == _roomID).FirstOrDefault();
        }
        
        Debug.LogWarning($"Brak DungeonRoom'u o wybrnaym ID: {_roomID}. Zwrócono wartosc null");
        return null;
    }
}