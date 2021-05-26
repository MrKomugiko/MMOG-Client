using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static Dictionary<int,PlayerManager> players = new Dictionary<int, PlayerManager>();
    public GameObject localPlayerPrefab; // lokalny gracz 
    public GameObject playerPrefab; // inni gracze na serwerze
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

    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if(_id == Client.instance.myId) 
            _player = Instantiate(localPlayerPrefab, _position,_rotation);
        else
            _player = Instantiate(playerPrefab,_position,_rotation);

        PlayerManager _playerData = _player.GetComponent<PlayerManager>();
        _playerData.Id = _id;
        _playerData.Username = _username;
        players.Add(_id,_playerData);
    }
}
