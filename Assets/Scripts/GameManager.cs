using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    [SerializeField] public Camera cam;
    public static GameManager instance;
    [SerializeField] public Text ANDROIDLOGGER;
    [SerializeField] public GameObject dungeonsWindow;
    [SerializeField] public GameObject NPC_Glowing_SPRITE_PREFAB;
    [SerializeField] public List<Tile> listaDostepnychTilesow;
     [SerializeField] public List<GameObject> listaDostepnychObiektow_ANIMACJI;
    public List<UnityMapElements> ListaDostepnychMapTEST = new List<UnityMapElements>();
    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();
    public GameObject localPlayerPrefab; 
    public GameObject playerPrefab; 
    public CounterScript Counter;
    [SerializeField] GameObject TilemapContainerPrefab;
    private void Awake()
    {
        var mapsContainer = new UnityMapElements(LOCATIONS.Start_First_Floor,GameObject.Find("Grid").transform, TilemapContainerPrefab); 
            ListaDostepnychMapTEST.Add(mapsContainer);

        mapsContainer = new UnityMapElements(LOCATIONS.Start_Second_Floor,GameObject.Find("Grid").transform, TilemapContainerPrefab);  
            ListaDostepnychMapTEST.Add(mapsContainer);

        mapsContainer = new UnityMapElements(LOCATIONS.DUNGEON_1,GameObject.Find("Grid").transform, TilemapContainerPrefab);
            ListaDostepnychMapTEST.Add(mapsContainer);

        mapsContainer = new UnityMapElements(LOCATIONS.DUNGEON_2,GameObject.Find("Grid").transform, TilemapContainerPrefab);
            ListaDostepnychMapTEST.Add(mapsContainer);

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
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, Vector3Int tileCoordPosition, LOCATIONS _currentLocation, int _currentfloor, int _dungeonRoomID)
    {
        bool _isLocal;
        GameObject _player;
        InstantiatePlayerGameObject(_id, _position, _rotation, out _isLocal, out _player);

        var _playerData = PreparePlayerData(_id, _username, tileCoordPosition, _currentLocation, _currentfloor, _dungeonRoomID, _isLocal, _player);
        _playerData.nickDisplayed.SetText(_playerData.Username);

        players.Add(_id, _playerData);
        
        SetPlayerLocation_init(_playerData, _currentfloor);

        DungeonManager.instance.Load();
    }

    private void InstantiatePlayerGameObject(int _id, Vector3 _position, Quaternion _rotation, out bool _isLocal, out GameObject _player)
    {
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
            _isLocal = true;
            InventoryScript.instance.SetupInventory();
            GameManager.instance.LogedIn = true;
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
            _isLocal = false;
        }
    }

    private PlayerManager PreparePlayerData(int _id, string _username, Vector3Int tileCoordPosition, LOCATIONS _currentLocation, int _currentfloor, int _dungeonRoomID, bool _isLocal, GameObject _player)
    {
        PlayerManager _playerData = _player.GetComponent<PlayerManager>();
        _playerData.Id = _id;
        _playerData.Username = _username;
        _playerData.CurrentPosition_GRID = tileCoordPosition;
        _playerData.CurrentLocation = _currentLocation;
        _playerData.IsLocal = _isLocal;
        if (_dungeonRoomID > 0)
        {
            try
            {
                _playerData.dungeonRoom = DungeonManager.GetDungeonLobbyByRoomID(_dungeonRoomID);

            }
            catch (System.Exception)
            {
                Debug.LogWarning("cos nie tak z szukaniem roomid dla nowego gracza");
            }
        }

        return _playerData;
    }

    public void OnClick_CloseApplication()
    {
        Application.Quit();
        print("quit");
    }
 
    public Dictionary<Vector3Int, LOCATIONS[]> LocationMaps =
        new Dictionary<Vector3Int, LOCATIONS[]>
        {
            // TODO: powinno byc pobierane z update patchnotesa, ale tam vector3 sie nie zapisuje, trzeba sprawdzic dlaczego
            {new Vector3Int( 7, -2, 14), new LOCATIONS[2]{LOCATIONS.Start_First_Floor,LOCATIONS.Start_Second_Floor}}, // przejscie z jednej mapy na drugą
            {new Vector3Int( -14, -1, 2), new LOCATIONS[2]{LOCATIONS.DUNGEON_1,LOCATIONS.DUNGEON_1}}, // tylko wejscie
            {new Vector3Int( 2,-11,2), new LOCATIONS[2]{LOCATIONS.Start_First_Floor,LOCATIONS.Start_First_Floor}}, // tylko wejscie
            {new Vector3Int( -12, 0, 2), new LOCATIONS[2]{LOCATIONS.DUNGEON_2,LOCATIONS.DUNGEON_2}}, // tylko wejscie
            //18/-2/10
           


//18/-4/10
        };
    public bool LogedIn = false;
    public void EnterNewLocation(Vector3Int locationCoord_Grid, PlayerManager player)
    {
        var enterNewLocation = LocationMaps[locationCoord_Grid];
        var new_location = enterNewLocation[0] == player.CurrentLocation ? enterNewLocation[1] : enterNewLocation[0];

        if (player.IsLocal == false)
        {
            player.CurrentLocation = new_location;
            Debug.Log(player.Username + "  entered " + new_location.ToString());
            return;
        }

        SetLocationScene(new_location);

        if (player.CurrentLocation != new_location)
        {
            Debug.Log("You entered " + new_location.ToString());
            player.CurrentLocation = new_location;
            ClientSend.SendServerPlayerNewLocalisation(new_location);
        }
    }
    private static void SetLocationScene(LOCATIONS new_location)
    {
        foreach (var mapa in GameManager.instance.ListaDostepnychMapTEST)
        {
            mapa.Container.SetActive(false);
        }

        switch (new_location)
        {
            case LOCATIONS.Start_First_Floor: break;
            case LOCATIONS.Start_Second_Floor:

                // UWAGA: na piętro 2 wejdziemy tylko z piętra 1, więc konieczne jest zostawienie piętra 1 nadal aktywnego
                GameManager.instance.ListaDostepnychMapTEST.Where(loc => loc.MapName == LOCATIONS.Start_First_Floor).First().Container.SetActive(true);
                break;

            case LOCATIONS.DUNGEON_1: break;
            case LOCATIONS.DUNGEON_2: break;
        }
        GameManager.instance.ListaDostepnychMapTEST.Where(loc => loc.MapName == new_location).First().Container.SetActive(true);
    }
    public void SetPlayerLocation_init(PlayerManager player, int _currentfloor)
    {
        player.movementScript.CurrentFloor = _currentfloor;
        if (player.IsLocal)
        {
            SetLocationScene(player.CurrentLocation);
        }
        player.CurrentLocation = player.CurrentLocation;
    }
    public static PlayerManager GetLocalPlayer()
    {
        try
        {
        if (GameManager.players.ContainsKey(Client.instance.myId))
            return GameManager.players[Client.instance.myId];
        else
            return null;
            
        }
        catch (System.Exception)
        {
            print("blad szukania local player");
            return null;
            
        }
    }
}

