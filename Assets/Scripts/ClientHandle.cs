using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet)
    {

       ThreadManager.ExecuteOnMainThread(()=>UIManager.instance.EnterGame());

        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
        ClientSend.UDPTestReceived();
    }

    public static void SpawnPlayer(Packet _packet)
    {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();

        // TODO: spakowac Vector3Int odpowiadajacy coordynatom na tilemapie
        Vector3Int _tileMapCoordinates = new Vector3Int(0,0,2);
        // Vector3Int _tileMapCoordinates = new Vector3Int(_packet.ReadInt(),_packet.ReadInt(),_packet.ReadInt());
        // aktualnie standardowo 0,0,0 dla kazdego 
        
        GameManager.instance.SpawnPlayer(_id,_username,_position,_rotation, _tileMapCoordinates);
    }

    public static void PlayerPosition(Packet _packet) {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

//        print(_position.x+" "+_position.y+" "+_position.z);
        GameManager.players[_id].transform.position = _position;
        GameManager.players[_id].MoveToPositionInGrid(new Vector3Int((int)_position.x,(int)_position.y,(int)_position.z));
    }

    public static void UpdateChat(Packet _packet) {
        string _msg = _packet.ReadString();
        print("Odebrano wiadomosc od GM: " + _msg);

        string chattext = UIManager.czatTMP.text;
        UIManager.czatTMP.SetText(chattext+$"\n<color=red><b>[{DateTime.Now.ToShortTimeString()}]:[GM]:{_msg}</b></color>");
    }
    public static void UpdateChat_NewUserPost(Packet _packet)
    {
        int _playerId = _packet.ReadInt();
        string _message = _packet.ReadString();
        string _username = GameManager.players[_playerId].Username;
        string _time=DateTime.Now.ToShortTimeString();

        string chattext = UIManager.czatTMP.text;
        UIManager.czatTMP.SetText(chattext+$"\n<color=white><b>[{_time}]</b>:<b>[{_username}]</b>:{_message}</color>");
    }

    public static void RemoveOfflinePlayer(Packet _packet) {
        int _id = _packet.ReadInt();
        Destroy(GameManager.players[_id].gameObject);
        GameManager.players.Remove(_id);
        // puff ;d gracz offline znika z gry 
    }
}
