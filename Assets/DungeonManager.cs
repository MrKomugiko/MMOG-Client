
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

    [SerializeField] GameObject MainSelectDungeonButton_Prefab;

     [SerializeField] public List<GameObject> ListOfDungeonMainPages_GameObject;
    [SerializeField] WindowScript MainWindow;
    [SerializeField] GameObject DungeonsListContainer;

    [SerializeField] GameObject RoomButton_Prefab;
    [SerializeField] WindowScript SelectionWindow;
    [SerializeField] GameObject RoomsListContainer;

    [SerializeField] WindowScript WaitingLobbyRoomWindow;

    public static List<DungeonsLobby> ListOfDungeonLobby;
    [SerializeField] public List<GameObject> ListOfDungeonLobby_GameObject;
    public static DungeonManager instance;

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
        try
        {

            print("my id = "+Client.instance.myId);
            print("players count : "+GameManager.players.Count());
            if(GameManager.players[Client.instance.myId].IsLocal)
            {
                // Create list of avaiable dungeons
                string[]listaDungeonow = Enum.GetNames(typeof(DUNGEONS));
                DUNGEONS dungeon;
                foreach(string dungeon_string in listaDungeonow)
                {
                    Enum.TryParse<DUNGEONS>(dungeon_string,out dungeon);
                    CreateMainDungeonChannelButton(dungeon);
                }
                ListOfDungeonLobby = new List<DungeonsLobby>();
                
                print("elo ! => init data from server");
                ClientSend.GetCurrentDungeonLobbysData(dungeon = default); // ALL
            }
        }
        catch(Exception ex) { 
            foreach(var players in GameManager.players)
            {
                Debug.Log(players.Key);
            }
            Debug.LogWarning("------------------------------------------------------- error, client id ? "+ex.Message);}
    }
    private void CreateMainDungeonChannelButton(DUNGEONS dungeonType)
    {
        if(ListOfDungeonMainPages_GameObject.Where(o=>o.transform.name == dungeonType.ToString()).Any()) return;

        GameObject dungeonObject = Instantiate(MainSelectDungeonButton_Prefab,Vector3.zero,Quaternion.identity,DungeonsListContainer.transform);
        
        dungeonObject.name = dungeonType.ToString();
        dungeonObject.GetComponentInChildren<TextMeshProUGUI>().SetText(dungeonType.ToString());
        
        dungeonObject.GetComponent<Button>().onClick.RemoveAllListeners();
        dungeonObject.GetComponent<Button>().onClick.AddListener(()=>OpenDungeonLobbysWindow(dungeonType));

        ListOfDungeonMainPages_GameObject.Add(dungeonObject);
    }
    private void OpenDungeonLobbysWindow(DUNGEONS dungeonType)
    {
        MainWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);

        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().GetComponent<Button>().onClick.RemoveAllListeners();
        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().onClick.AddListener(()=>OnClick_CreateAndJoinNewLobby(dungeonType, Client.instance.myId));
        ClientSend.GetCurrentDungeonLobbysData(dungeonType);
    }
    public void UpdateLobbyData(int _lobbyId, DungeonsLobby newRoomData)
    {
        print("UpdateLobbyData");
       
        // sprawdzenie czy obiekt juz istnieje na scenie:
        DungeonsLobby room = null; 
        if(ListOfDungeonLobby.Count>0)
        {
            room = ListOfDungeonLobby.Where(l=>l.LobbyID == _lobbyId).FirstOrDefault();
        }

        if(room != null)
        {
            print("---------------------------------------"+_lobbyId+"-------------------------------------");

            room = newRoomData;
            room.lobbySceneObjectRefference.GetComponent<roomScript>().AssignRoomDataToWindow(room);
            
            string myUsername = GameManager.players[Client.instance.myId].Username;

            ////// if (room.Players.Contains(myUsername) == false)
           ///// // {
             // JEŻELI TWOJ AKTUALNIE OTWARTY POKOJ NIE JEST AKTUALIZOWANY< NIE RUSZAJ GO  xD
              /////  print("nie ma potrzeby edytować zawartosci pokoju, nie ma cie w nim = nie jest otwarty");
                // zaktualizuje sie tylko liczba graczy 
                var title = room.lobbySceneObjectRefference.transform
                        .Find("roomMembersCount")
                        .GetComponent<TextMeshProUGUI>();

                title.SetText(room.PlayersCount+" / "+room.MaxPlayersCapacity);

                if(room.IsFull)
                {
                
                    room.lobbySceneObjectRefference.GetComponent<Button>().interactable = false;

                    title.SetText($"<color=yellow>{title.text} [FULL]</color>");
                }
                else
                {
                    room.lobbySceneObjectRefference.GetComponent<Button>().interactable = true;
                }

                if(room.IsStarted)
                {
                    room.lobbySceneObjectRefference.GetComponent<Button>().interactable = false;

                    title.SetText($"<color=red>{title.text} [IN GAME]</color>");
                }
         
                
            ///// //   return;
          /////// }
            // okno szczegolow jest otwarte i zostanie
            TextMeshProUGUI textUserListInLobby = WaitingLobbyRoomWindow.gameObject.transform.Find("ContentText").GetComponent<TextMeshProUGUI>();
            textUserListInLobby.SetText(
            $"Players ready for adventure:\n\n"+
            $"   * {room.LobbyOwner} [Leader]\n"
            );

            foreach(string player in room.Players)
            {
                if(player == room.LobbyOwner) continue;
                textUserListInLobby.SetText($"{textUserListInLobby.text}   * {player}\n");
                
            }
        }
        else
        {   
            if(ListOfDungeonLobby_GameObject.Where(o=>o.name == "ROOM_"+newRoomData.LobbyID).Any()) 
            {
                print("istnieje juz taki obiekt w pamieci");
                return;
            }
            print("spawn nowego obiektu listy jezeli nie istnieje");

            GameObject roomObject = Instantiate(RoomButton_Prefab,Vector3.zero,Quaternion.identity,RoomsListContainer.transform);
            
            ListOfDungeonLobby_GameObject.Add(roomObject);           
            ListOfDungeonLobby.Add(newRoomData);
            // edycja danych
            roomObject.name = "ROOM_"+newRoomData.LobbyID;
            roomObject.transform.localPosition = Vector3.zero;

            roomObject.transform.Find("roomName").GetComponent<TextMeshProUGUI>()
                .SetText("#"+newRoomData.LobbyOwner);
            roomObject.transform.Find("roomMembersCount").GetComponent<TextMeshProUGUI>()
                .SetText(newRoomData.PlayersCount+" / "+newRoomData.MaxPlayersCapacity);

            roomObject.GetComponent<Button>().onClick.AddListener(()=>JoinToRoom(newRoomData.LobbyID, Client.instance.myId));
            // TODO: kolorowanie obiektu jezeli pełny - na czerwono inaczej na zielono
            // TODO: włączanie ikonki kłódki gdy pokoj jest zablokowany -> poźniej
        }
        
    }
    

    public void OnClick_CreateAndJoinNewLobby(DUNGEONS dungeonType, int clientId)
    {
        LOCATIONS location;
        Enum.TryParse<LOCATIONS>(dungeonType.ToString(),out location);

        PlayerManager player_leader = GameManager.players[clientId];
        var newLobby = new DungeonsLobby(UnityEngine.Random.Range(0,1_000_000), player_leader.Username, dungeonType);
        print("utworzono lobby o id = "+newLobby.LobbyID);

        player_leader.dungeonRoom = newLobby;
        ClientSend.CreateNewDungeonLobby(location, newLobby.LobbyID); // TODO: dokonczyc proses tworzenia lobby 

        UpdateLobbyData(newLobby.LobbyID, newLobby);

        // TODO: wyslanie info do serwera zeby dodal nowy wpis
        // TODO: [WAŻNE] !synchronizacja, roomsID ! 

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
            enteButton.onClick.AddListener(()=>StartAndEnterDungeon(dungeonType, newLobby.LobbyID));

        // button start -> TODO: zablokownie lobby do czasu zakończenia gry -> w razie ewentualnego dc mozna bedzie ponownie dołączyc o ile znajdujemy sie na liscie graczy ktoryz rozpoczeli gre ? 
        var cancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("CancelDungeonButton").GetComponent<Button>();
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(()=>CancelAndRemoveLobby(dungeonType, newLobby.LobbyID));
            cancelButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Cancel");

        var backAndCancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("Close").GetComponent<Button>();
            backAndCancelButton.onClick.RemoveAllListeners();
            backAndCancelButton.onClick.AddListener(()=>CancelAndRemoveLobby(dungeonType, newLobby.LobbyID));
    }

    public void CancelAndRemoveLobby(DUNGEONS dungeonType, int roomId)
    {
        print("anulujesz lobby, wszyscy wracacie do głownej listy");

        WaitingLobbyRoomWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
        DiscposeRoom(dungeonType,roomId);
        
    }

    public void DiscposeRoom(DUNGEONS dungeonType, int roomId)
    {
        print("usuwanie pokoju");
        DungeonsLobby lobbyToRemove = ListOfDungeonLobby.Where(room=>room.LobbyID==roomId).FirstOrDefault();

        int? roomObjectIndex = null;
        // sprawdzenie czy pokoj istnieje
        if(lobbyToRemove!=null)
        {
            if(lobbyToRemove.Players.Contains(GameManager.players[Client.instance.myId].Username))
            {
                GameManager.players[Client.instance.myId].dungeonRoom = null;
            }
            print("usunieto wpis dungeonalobby z pamięci i wyslanie do serwera info o usuniecie");
            ClientSend.RemoveExistingDungeonLobby(lobbyToRemove);
            ListOfDungeonLobby.Remove(lobbyToRemove);
        }

        roomObjectIndex = ListOfDungeonLobby_GameObject
            .IndexOf(ListOfDungeonLobby_GameObject
            .Where(room=>room.name == $"ROOM_{roomId}")
            .First());

        print("roomObjectIndex"+roomObjectIndex);
        if(roomObjectIndex != null)
        {
            Destroy(ListOfDungeonLobby_GameObject[(int)roomObjectIndex]);
            ListOfDungeonLobby_GameObject.RemoveAt((int)roomObjectIndex);
            print("obiekt pokoju z listy został pomyślnie usunięty"); 
        }
    }

    private void StartAndEnterDungeon(DUNGEONS dungeonType, int lobbyID)
    {   
        WaitingLobbyRoomWindow.OnClick_Close();

        LOCATIONS location;
        Enum.TryParse<LOCATIONS>(dungeonType.ToString(),out location);
        
        //ClientSend.TeleportMe(location); // lider klikajacy start
        ClientSend.GroupTeleportPlayersInRoom(location, lobbyID);
        ClientSend.GroupEnteredDungeon(lobbyID);
    
        print("przechodzisz do dungeonu, a wraz z tobą cała zebrana druzyna");
    }
    private void JoinToRoom(int roomId, int clientId)
    {
        PlayerManager playerWhoWantJoin = GameManager.players[clientId];
        var existingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        // TODO: wyslanie info do serwera zeby dodal nowy wpis
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
        print("opuszczono dungeonLobby room");
        PlayerManager playerWhoWantLeaveRoom = GameManager.players[clientId];
        var leavingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        // TODO: wyslanie info do serwera zeby dodal nowy wpis
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
        // TODO:
        return null;
    }
}

[Serializable]
public class DungeonsLobby
{
    [SerializeField] private int lobbyID;
    [SerializeField] private string lobbyOwner;
    [SerializeField] private DUNGEONS dungeonLocation;
    [SerializeField] private List<string> players = new List<string>();
    [SerializeField] private int maxPlayersCapacity;
    [SerializeField] private bool isStarted;

    public GameObject lobbySceneObjectRefference
    {
        get => DungeonManager.instance.ListOfDungeonLobby_GameObject
            .Where(room => room.name == ("ROOM_" + LobbyID))
            .FirstOrDefault();
    }

    public int LobbyID { get => lobbyID; private set => lobbyID = value; }
    public string LobbyOwner { get => lobbyOwner; private set => lobbyOwner = value; }
    public DUNGEONS DungeonLocation { get => dungeonLocation; private set => dungeonLocation = value; }
    public List<string> Players { get => players; set => players = value; }
    public int PlayersCount { get => Players.Count; }
    public int MaxPlayersCapacity { get => maxPlayersCapacity; private set => maxPlayersCapacity = value; }
    public bool IsFull { get => PlayersCount >= MaxPlayersCapacity ? true : false; }
    public bool IsStarted { get=>isStarted;  set => isStarted = value; }

    public DungeonsLobby(int lobbyID, string lobbyOwner, DUNGEONS dungeonLocation, int maxPlayersCapacity = 2, List<string> players = null, bool isStarted = false)
    {
        LobbyID = lobbyID;
        LobbyOwner = lobbyOwner;
        DungeonLocation = dungeonLocation;
        MaxPlayersCapacity = maxPlayersCapacity;
        IsStarted = isStarted;

        if (players == null)
            {
                Players.Add(LobbyOwner);
            }
        else
        {
            Players = players;
        }
    }
    public DungeonsLobby(int lobbyID, string lobbyOwner, LOCATIONS dungeonLocation, int maxPlayersCapacity = 2, List<string> players = null, bool isStarted = false)
    {
        LobbyID = lobbyID;
        LobbyOwner = lobbyOwner;
        DUNGEONS dungeonloc;
        Enum.TryParse<DUNGEONS>(dungeonLocation.ToString(), out dungeonloc);
        DungeonLocation = dungeonloc;
        MaxPlayersCapacity = maxPlayersCapacity;
        IsStarted = isStarted;

        if (players == null)
            {
                Players.Add(LobbyOwner);
            }
        else
        {
            Players = players;
        }
    }
}

public enum DUNGEONS // nazwa jednakowa z tą w locations !
{
    DUNGEON_1 = 1,
    DUNGEON_2
}