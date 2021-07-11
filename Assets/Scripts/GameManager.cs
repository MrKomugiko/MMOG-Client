using System;
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

    [SerializeField] public List<GameObject> ListaDostepnychLokalizacji = new List<GameObject>();
    [SerializeField] public GameObject StartLocationSeconFloorContainer;
    [SerializeField] public GameObject StartLocationFirstFloorContainer;
    public Tilemap _tileMap;
    public Tilemap _tileMap_GROUND;
    public static Dictionary<Vector3Int, string> MAPDATA { get; set; }
    public static Dictionary<Vector3Int, string> MAPDATA_Ground { get; set; }

    public Tilemap _tileMap3ndFloor_GROUND;
    public Tilemap _tileMap2ndFloor;
    public static Dictionary<Vector3Int, string> MAPDATA2ndFloor_Ground { get; internal set; }
    public static Dictionary<Vector3Int, string> MAPDATA2ndFloor { get; internal set; }


    public Tilemap _tilemapDUNGEON;
    public Tilemap _tilemapDUNGEON_GROUND;
    public static Dictionary<Vector3Int, string> MAPDATA_DUNGEON { get; internal set; }
    public static Dictionary<Vector3Int, string> MAPDATA_DUNGEON_GROUND { get; internal set; }

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab; 
    public GameObject playerPrefab; 

    private void Awake()
    {
        MAPDATA = new Dictionary<Vector3Int, string>();
        MAPDATA_Ground = new Dictionary<Vector3Int, string>();

        MAPDATA2ndFloor = new Dictionary<Vector3Int, string>();
        MAPDATA2ndFloor_Ground = new Dictionary<Vector3Int, string>();

        ListaDostepnychLokalizacji.Add(StartLocationFirstFloorContainer);
        ListaDostepnychLokalizacji.Add(StartLocationSeconFloorContainer);


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
        if (players.ContainsKey(_id))
        {
            print("w grze jest juz taki gracz z tym id");
            return;
        }

        bool _isLocal;
        GameObject _player;
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

        players.Add(_id, _playerData);
        SetPlayerLocation_init(players[_id].CurrentLocation, players[_id], _currentfloor);


        DungeonManager.instance.Load();
    }
    public void OnClick_CloseApplication()
    {
        Application.Quit();
        print("quit");
    }
 
    public Dictionary<Vector3Int, LOCATIONS[]> LocationMaps =
        new Dictionary<Vector3Int, LOCATIONS[]>
        {
            {new Vector3Int( 7, -2, 14), new LOCATIONS[2]{LOCATIONS.Start_First_Floor,LOCATIONS.Start_Second_Floor}},
            {new Vector3Int( -14, -1, 2), new LOCATIONS[2]{LOCATIONS.DUNGEON_1,LOCATIONS.Start_Second_Floor}}
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

        foreach (var mapa in GameManager.instance.ListaDostepnychLokalizacji)
        {
            mapa.SetActive(false);
        }
        switch (new_location)
        {
            case LOCATIONS.Start_First_Floor:
                GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == new_location.ToString()).First().SetActive(true);
                break;

            case LOCATIONS.Start_Second_Floor:
                GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == new_location.ToString()).First().SetActive(true);
                // UWAGA: na piętro 2 wejdziemy tylko z piętra 1, więc konieczne jest zostawienie piętra 1 nadal aktywnego
                GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == LOCATIONS.Start_First_Floor.ToString()).First().SetActive(true);
                break;

            case LOCATIONS.DUNGEON_1:
                GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == new_location.ToString()).First().SetActive(true);
                break;
        }

        Debug.Log("You entered " + new_location.ToString());
        player.CurrentLocation = new_location;
        ClientSend.SendServerPlayerNewLocalisation(new_location);
    }

    public void SetPlayerLocation_init(LOCATIONS location, PlayerManager player, int _currentfloor)
    {
        player.movementScript.CurrentFloor = _currentfloor;
        if (player.IsLocal)
        {
            // pozamykanie innych otwartych map
            foreach (var mapa in GameManager.instance.ListaDostepnychLokalizacji)
            {
                mapa.SetActive(false);
            }
            switch (location)
            {
                case LOCATIONS.Start_First_Floor:
                    GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == location.ToString()).First().SetActive(true);
                    break;

                case LOCATIONS.Start_Second_Floor:
                    GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == location.ToString()).First().SetActive(true);
                    // UWAGA: na piętro 2 wejdziemy tylko z piętra 1, więc konieczne jest zostawienie piętra 1 nadal aktywnego
                    GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == LOCATIONS.Start_First_Floor.ToString()).First().SetActive(true);
                    break;

                case LOCATIONS.DUNGEON_1:
                    GameManager.instance.ListaDostepnychLokalizacji.Where(loc => loc.name == location.ToString()).First().SetActive(true);
                    break;
            }
        }

        Debug.Log("You entered " + location.ToString());
        player.CurrentLocation = location;
    }

    public static PlayerManager GetLocalPlayer()
    {
        if (GameManager.players.ContainsKey(Client.instance.myId))
            return GameManager.players[Client.instance.myId];
        else
            return null;
    }
    public enum DUNGEONS
    {
        DUNGEON_1
    }
}

