using System.Net.NetworkInformation;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Net.Sockets;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [SerializeField] public GameObject shopWindow;
    [SerializeField] public GameObject NPC_Glowing_SPRITE_PREFAB;
    [SerializeField] public List<Tile> listaDostepnychTilesow;
    
    public static Dictionary<Vector3Int,string> MAPDATA {get; set;}
    public static Dictionary<Vector3Int,string> MAPDATA_Ground  {get; set;}
    public int CurrentUpdateVersion 
    { 
        get => currentUpdateVersion; 
        set 
        { 
            if(currentUpdateVersion < value)
            {
                // mapa jest starsza niz aktualnie na serwerze
                print($"Current update version is outdated: [{currentUpdateVersion}]. New update on server is {value} ready to download");
                ClientSend.DownloadLatestMapData();
                currentUpdateVersion = value; 
                UIManager.instance.UpdateBuildIndicatorOnScreen(currentUpdateVersion, _isDownloadAvaiable: true);
            }
            else
            {
                // mapa jest aktualna 
                print("Map is up to date");
                currentUpdateVersion = value;

                UIManager.instance.UpdateBuildIndicatorOnScreen(currentUpdateVersion,_isDownloadAvaiable: false);
            }
        }
    }

    public static Dictionary<int,PlayerManager> players = new Dictionary<int, PlayerManager>();
    
    public GameObject localPlayerPrefab; // lokalny gracz 
    public Tile localPlayerTile;
    public GameObject playerPrefab; // inni gracze na serwerze
    public Tile playerTile;
    [SerializeField] private int currentUpdateVersion;
    public Tilemap _tileMap;
    public Tilemap _tileMap_GROUND;
    
    Vector3Int startingLocationOnGrid = new Vector3Int(0,0,2);
    Vector3 basePodition = new Vector3(0,0,2);

    private void Awake()
    {
        MAPDATA = new Dictionary<Vector3Int, string>();
        MAPDATA_Ground = new Dictionary<Vector3Int,string>();
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
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, Vector3Int tileCoordPosition )
    {
       if(players.ContainsKey(_id)) return;

        bool _isLocal;
        GameObject _player;
        if(_id == Client.instance.myId) 
        {
            _player = Instantiate(localPlayerPrefab, _position,_rotation);
            _tileMap.SetTile(tileCoordPosition,localPlayerTile);
            _isLocal = true;
        }
        else
        {
            _player = Instantiate(playerPrefab,_position,_rotation);
            _tileMap.SetTile(tileCoordPosition,playerTile);
            _isLocal = false;
        }

        PlayerManager _playerData = _player.GetComponent<PlayerManager>();
        _playerData.Id = _id;
        _playerData.Username = _username;
        _playerData.CurrentPosition_GRID = tileCoordPosition;
        _playerData.IsLocal = _isLocal;
        _playerData.MyTile = _isLocal?localPlayerTile:playerTile;
        
        players.Add(_id,_playerData);
    }
    public void OnClick_CloseApplication()
    {
        Application.Quit(); 
        print("quit");
    }
    public void CheckForUpdates()
    {
        //TODO: dodac numer update`a do sprawdzanmia

        // zablokowanie gry na czas sciagania mapy

        // wyslanie proźby o nową wersje mapy

        // zakttualizowanie mapy

        // odblokowanie gry

        // connect
    }

 
}
