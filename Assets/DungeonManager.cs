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
    [SerializeField] WindowScript MainWindow;
    [SerializeField] GameObject DungeonsListContainer;

    [SerializeField] GameObject RoomButton_Prefab;
    [SerializeField] WindowScript SelectionWindow;
    [SerializeField] GameObject RoomsListContainer;

    [SerializeField] WindowScript WaitingLobbyRoomWindow;

    private List<DungeonsLobby> ListOfDungeonLobby;
    [SerializeField] private List<GameObject> ListOfDungeonLobby_GameObject;

    private void Start() 
    {
        ListOfDungeonLobby_GameObject = new List<GameObject>();
        
        // Create list of avaiable dungeons
        string[]listaDungeonow = Enum.GetNames(typeof(DUNGEONS));
        DUNGEONS dungeon;
        foreach(string dungeon_string in listaDungeonow)
        {
            Enum.TryParse<DUNGEONS>(dungeon_string,out dungeon);
            GameObject dungeonObject = Instantiate(MainSelectDungeonButton_Prefab,Vector3.zero,Quaternion.identity,DungeonsListContainer.transform);
            
            dungeonObject.name = dungeon_string;
            dungeonObject.GetComponentInChildren<TextMeshProUGUI>().SetText(dungeon_string);
            
            dungeonObject.GetComponent<Button>().onClick.RemoveAllListeners();
            dungeonObject.GetComponent<Button>().onClick.AddListener(()=>OpenDungeonLobbysWindow(dungeon));

        }

        ListOfDungeonLobby = new List<DungeonsLobby>();
        print("elo ! => init data from server");
    }

    private void OpenDungeonLobbysWindow(DUNGEONS dungeonType)
    {
        MainWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);

        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().GetComponent<Button>().onClick.RemoveAllListeners();
        SelectionWindow.gameObject.transform.Find("CreateButton").GetComponent<Button>().onClick.AddListener(()=>OnClick_CreateNewLobby(dungeonType, Client.instance.myId));
        DownloadDungeonsLobbyData(dungeonType);
    }
    public void DownloadDungeonsLobbyData(DUNGEONS dungeonType)
    {

        var lobbyData = CreateTestData(dungeonType);
        UpdateLobbyData(lobbyData.LobbyID, lobbyData);
    }

    private DungeonsLobby CreateTestData(DUNGEONS dungeonType)
    {
        // init bez dodatkowych graczy w srodku
        return new DungeonsLobby(1,"TEST_Player",dungeonType);
    }


    public void UpdateLobbyData(int _lobbyId, DungeonsLobby newRoomData)
    {
        DungeonsLobby room = ListOfDungeonLobby.Where(l=>l.LobbyID == _lobbyId).FirstOrDefault();
        if(room != null)
        {
            // TODO: doprecyzować jakiego typu update ma zostac przeprowadzony
            // TODO: CRUD
            print("nadpisanie / uaktualnienie danych");
            room = newRoomData;
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
            print("spawn nowego obiektu listy");
            // brak takiego wpisu, trzeba go zainstancjonowac
            ListOfDungeonLobby.Add(newRoomData);
            GameObject roomObject = Instantiate(RoomButton_Prefab,Vector3.zero,Quaternion.identity,RoomsListContainer.transform);
            ListOfDungeonLobby_GameObject.Add(roomObject);
            // edycja danych
            roomObject.name = "ROOM_"+newRoomData.LobbyID;
            roomObject.transform.localPosition = Vector3.zero;
            roomObject.transform.Find("roomName").GetComponent<TextMeshProUGUI>().SetText("#"+newRoomData.LobbyOwner);
            roomObject.transform.Find("roomMembersCount").GetComponent<TextMeshProUGUI>().SetText(newRoomData.PlayersCount+" / "+newRoomData.MaxPlayersCapacity);
            roomObject.GetComponent<Button>().onClick.AddListener(()=>JoinToRoom(newRoomData.LobbyID, Client.instance.myId));
            // TODO: kolorowanie obiektu jezeli pełny - na czerwono inaczej na zielono
            // TODO: włączanie ikonki kłódki gdy pokoj jest zablokowany -> poźniej
        }
        
    }
    
    public void OnClick_CreateNewLobby(DUNGEONS dungeonType, int clientId)
    {
        print("cfreate");
        PlayerManager player_leader = GameManager.players[clientId];
        var newLobby = new DungeonsLobby(2, player_leader.Username, dungeonType);
        UpdateLobbyData(newLobby.LobbyID, newLobby);

        // TODO: wyslanie info do serwera zeby dodal nowy wpis

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
    }

    private void CancelAndRemoveLobby(DUNGEONS dungeonType, int roomId)
    {
        WaitingLobbyRoomWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
        // usuniecie obiektu nieaktywnego juz lobby z listy
        ListOfDungeonLobby.Remove(ListOfDungeonLobby.Where(room=>room.LobbyID==roomId).FirstOrDefault());
        int roomIndex =ListOfDungeonLobby_GameObject.IndexOf(ListOfDungeonLobby_GameObject.Where(room=>room.name == $"ROOM_{roomId}").FirstOrDefault());
      try
      {
          
        Destroy(ListOfDungeonLobby_GameObject[roomIndex]);
      }
      catch (System.Exception ex)
      {
          
       print("error 1"+ ex.Message);
      }
        try
      {
          
        ListOfDungeonLobby_GameObject.RemoveAt(roomIndex);
      }
      catch (System.Exception ex)
      {
          
            print("error 2"+ex.Message);
      }

        print("anulujesz lobby, wszyscy wracacie do listy lobby");
    }

    private void StartAndEnterDungeon(DUNGEONS dungeonType, int lobbyID)
    {
        WaitingLobbyRoomWindow.OnClick_Close();

        LOCATIONS location;
        Enum.TryParse<LOCATIONS>(dungeonType.ToString(),out location);
        ClientSend.TeleportMe(location); // lider klikajacy start
        // TODO: teleport other party members
        print("przechodzisz do dungeonu, a wraz z tobą cała zebrana druzyna");
    }

    public void JoinToRoom(int roomId, int clientId)
    {
        PlayerManager playerWhoWantJoin = GameManager.players[clientId];
        var existingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        // TODO: wyslanie info do serwera zeby dodal nowy wpis
        existingLobby.Players.Add(GameManager.players[clientId].Username);
        UpdateLobbyData(existingLobby.LobbyID, existingLobby);

        if(playerWhoWantJoin.Username != existingLobby.LobbyOwner)
        {
            // tylko lider moze wystartowac gre w swoim pokoju
            WaitingLobbyRoomWindow.gameObject.transform.Find("EnterDungeonButton").transform.gameObject.SetActive(false);
            var cancelButton = WaitingLobbyRoomWindow.gameObject.transform.Find("CancelDungeonButton").GetComponent<Button>();
            cancelButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Leave lobby");
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(()=>LeaveRoom(roomId,clientId));
        }

        SelectionWindow.OnClick_Close();
        WaitingLobbyRoomWindow.OnClick_OpenCloseWindow(WaitingLobbyRoomWindow.gameObject);
    }

    private void LeaveRoom(int roomId, int clientId)
    {
        PlayerManager playerWhoWantJoin = GameManager.players[clientId];
        var leavingLobby = ListOfDungeonLobby.Where(room=>room.LobbyID == roomId).First();

        // TODO: wyslanie info do serwera zeby dodal nowy wpis
        leavingLobby.Players.Remove(GameManager.players[clientId].Username);
        UpdateLobbyData(leavingLobby.LobbyID, leavingLobby);

        WaitingLobbyRoomWindow.OnClick_Close();
        SelectionWindow.OnClick_OpenCloseWindow(SelectionWindow.gameObject);
    }
}

public class DungeonsLobby
{
    public int LobbyID { get; private set; }
    public string LobbyOwner { get; private set; }
    public DUNGEONS DungeonLocation { get; private set; }
    public List<string> Players { get; set; } = new List<string>();
    public int PlayersCount { get => Players.Count; }
    public int MaxPlayersCapacity { get; private set; }
    public bool IsFull {get => PlayersCount >= MaxPlayersCapacity?true:false;}
    public DungeonsLobby(int lobbyID, string lobbyOwner, DUNGEONS dungeonLocation,int maxPlayersCapacity = 2, List<string> players = null)
    {
        LobbyID = lobbyID;
        LobbyOwner = lobbyOwner;
        DungeonLocation = dungeonLocation;
        MaxPlayersCapacity = maxPlayersCapacity;

        if(players == null)
        {
            Players.Add(LobbyOwner);
        }
        else
        {
            Players = players;
        }
    }
}

/* co potrzeba z serwera do wyswietlenia lokalnie u klienta ?
    0. id => polaczone z Join po stronei serwera
    1. nazwa dungeonu -> location -> do teleporta
    2. nazwa lidera -> opis pokoju
    3. liczbe graczy w lobby -> opis pokoju
    4; maksylana liczbe graczy -> opis pokoju

    //TODO: lista aktualnie czekajcych graczy w pokoju




*/

public enum DUNGEONS // nazwa jednakowa z tą w locations !
{
    DUNGEON_1
}