using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public static Dictionary<int,PlayerManager> players = new Dictionary<int, PlayerManager>();
    public GameObject localPlayerPrefab; // lokalny gracz 
    public Tile localPlayerTile;
    public GameObject playerPrefab; // inni gracze na serwerze
    public Tile playerTile;
    public Tilemap _tileMap;

    Vector3Int startingLocationOnGrid = new Vector3Int(0,0,2);

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

    Vector3 basePodition = new Vector3(0,0,2);
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation, Vector3Int tileCoordPosition )
    {
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
}
