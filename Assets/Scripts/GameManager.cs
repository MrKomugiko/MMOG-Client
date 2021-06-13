using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public partial class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [SerializeField] public Text ANDROIDLOGGER;
    [SerializeField] public GameObject shopWindow;
    [SerializeField] public GameObject NPC_Glowing_SPRITE_PREFAB;
    [SerializeField] public List<Tile> listaDostepnychTilesow;

    [SerializeField] public List<GameObject> ListaDostepnychLokalizacji = new List<GameObject>();
     [SerializeField] public GameObject StartLocationSeconFloorContainer;
    [SerializeField]  public GameObject StartLocationFirstFloorContainer;
    public Tilemap _tileMap;
    public Tilemap _tileMap_GROUND;
    public static Dictionary<Vector3Int,string> MAPDATA {get; set;}
    public static Dictionary<Vector3Int,string> MAPDATA_Ground  {get; set;}

    public Tilemap _tileMap3ndFloor_GROUND;
    public Tilemap _tileMap2ndFloor;
    public static Dictionary<Vector3Int, string> MAPDATA2ndFloor_Ground { get; internal set; }
    public static Dictionary<Vector3Int, string> MAPDATA2ndFloor { get; internal set; }

   [Obsolete] public int CurrentUpdateVersion 
    { 
        get => currentUpdateVersion; 
        set 
        { 
           // if(currentUpdateVersion < value)
          //  {
                // mapa jest starsza niz aktualnie na serwerze
              //  print($"Current update version is outdated: [{currentUpdateVersion}]. New update on server is {value} ready to download");
              //  ClientSend.DownloadLatestMapData();
                currentUpdateVersion = value; 
              //  UIManager.instance.UpdateBuildIndicatorOnScreen(currentUpdateVersion, _isDownloadAvaiable: true);
          //  }
          //  else
          //  {
                // mapa jest aktualna 
             //   print("Map is up to date");
                currentUpdateVersion = value;

              //  UIManager.instance.UpdateBuildIndicatorOnScreen(currentUpdateVersion,_isDownloadAvaiable: false);
           // }
        }
    }

    public static Dictionary<int,PlayerManager> players = new Dictionary<int, PlayerManager>();
    
    public GameObject localPlayerPrefab; // lokalny gracz 
    public Tile localPlayerTile;
    public GameObject playerPrefab; // inni gracze na serwerze
    public Tile playerTile;
    [SerializeField] private int currentUpdateVersion;
    Vector3Int startingLocationOnGrid = new Vector3Int(0,0,2);
    Vector3 basePodition = new Vector3(0,0,2);

    private void Awake()
    {
        MAPDATA = new Dictionary<Vector3Int, string>();
        MAPDATA_Ground = new Dictionary<Vector3Int,string>();

        MAPDATA2ndFloor = new Dictionary<Vector3Int, string>();
        MAPDATA2ndFloor_Ground = new Dictionary<Vector3Int,string>();

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
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, Vector3Int tileCoordPosition, LOCATIONS _currentLocation)
    {
       if(players.ContainsKey(_id)) return;

        bool _isLocal;
        GameObject _player;
        if(_id == Client.instance.myId) 
        {
            _player = Instantiate(localPlayerPrefab, _position,_rotation);
         //   _tileMap.SetTile(tileCoordPosition,localPlayerTile);
            _isLocal = true;
        }
        else
        {
            _player = Instantiate(playerPrefab,_position,_rotation);
         //   _tileMap.SetTile(tileCoordPosition,playerTile);
            _isLocal = false;
        }

        PlayerManager _playerData = _player.GetComponent<PlayerManager>();
        _playerData.Id = _id;
        _playerData.Username = _username;
        _playerData.CurrentPosition_GRID = tileCoordPosition;
        _playerData.CurrentLocation = _currentLocation;
        _playerData.IsLocal = _isLocal;
        
        players.Add(_id,_playerData);
    }
    public void OnClick_CloseApplication()
    {
        Application.Quit(); 
        print("quit");
    }
    // public void CheckForUpdates()
    // {
    //     //TODO: dodac numer update`a do sprawdzanmia

    //     // zablokowanie gry na czas sciagania mapy

    //     // wyslanie proźby o nową wersje mapy

    //     // zakttualizowanie mapy

    //     // odblokowanie gry

    //     // connect
    // }

    public Dictionary<Vector3Int, LOCATIONS[]> LocationMaps = 
   // zmienic  Locations na zwykłą klase zawierającą szcegolowe info o mapie - np wysokosc ;d 
        new Dictionary<Vector3Int, LOCATIONS[]>{
        { 
            new Vector3Int( 7, -2, 14), new LOCATIONS[2]{LOCATIONS.Start_First_Floor,LOCATIONS.Start_Second_Floor}
        }
    };
 
    public void EnterNewLocation(Vector3Int locationCoord_Grid, PlayerManager player)
    {

        var enterNewLocation = LocationMaps[locationCoord_Grid];
        var new_location = enterNewLocation[0] == player.CurrentLocation?enterNewLocation[1]:enterNewLocation[0];

        if(player.IsLocal == false) {
            player.CurrentLocation = new_location;
            Debug.Log(player.Username+"  entered " + new_location.ToString());
            return;
            
        }


        // if(enterNewLocation.Contains(player.CurrentLocation)) return;
        // TODO: jakos to zautomatyzować
        switch(new_location)
        {
            case LOCATIONS.Start_First_Floor:
           //     player.GetComponent<SpriteRenderer>().sortingOrder = 0;
                //TODO: dodać liste lokalizacji ( gameobiektow) do listy
                StartLocationSeconFloorContainer.SetActive(false);
            break;


            case LOCATIONS.Start_Second_Floor:
          //      player.GetComponent<SpriteRenderer>().sortingOrder = 2;
                StartLocationSeconFloorContainer.SetActive(true);
                // StartLocationFirstFloorContainer.SetActive(false);
            break;
        }

        Debug.Log("You entered "+new_location.ToString());
        player.CurrentLocation = new_location;
        ClientSend.SendServerPlayerNewLocalisation(new_location);
        }
    }

