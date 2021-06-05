using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }
    private static void SendUDPData(Packet _packet)
    {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    #region Packets
        public static void WelcomeReceived()
        {
            print("Wysłanie do serwera pingu Welcom z id i nickiem");
            using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(UIManager.instance.usernameField.text);

                SendTCPData(_packet);
            }

                ClientSend.DownloadLatestUpdateVersionNumber();
        }
        public static void PlayerMovement(bool[] _inputs) 
        {
            //print("Wysłanie informacji o ruchu na serwer do przetworzenia");
            //Debug.Log("PlayerMovement in ClientSend");
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) {
                _packet.Write(_inputs.Length);
                foreach(bool _input in _inputs) {
                    _packet.Write(_input);
                }
                _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

                SendUDPData(_packet);
            }
        }
        public static void SendChatMessage(string _message)
        {
            print("Wysłanie wiadomości na serwer.");
            using (Packet _packet = new Packet((int)ClientPackets.SendChatMessage))
            {
                _packet.Write(Client.instance.myId);
                _packet.Write(_message);

                SendTCPData(_packet);
            }
        }
        public static void PingReceived() {
//            print("Odpowiedź na ping do serwera.");
            using (Packet _packet = new Packet((int)ClientPackets.PingReceived)) {
                _packet.Write(1);

                SendTCPData(_packet);
            }
        }
        public static void UDPTestReceived()
        {
            // using (Packet _packet = new Packet((int)ClientPackets.updTestReceived))
            // {
            //     _packet.Write("Received a UDP packet.");

            //     SendUDPData(_packet);
            // }
        }
        public static void SendMapDataToServer(MAPTYPE mapType) {

            print($"Wysłanie na serwer {mapType.ToString()}");

            Tilemap TILEMAP = new Tilemap();
            switch (mapType)
            {
                case MAPTYPE.GROUND_MAP:
                    TILEMAP = GameManager.instance._tileMap_GROUND;
                break;
                    case MAPTYPE.OBSTACLEMAP:
                    TILEMAP = GameManager.instance._tileMap;
                break;
            }

            Dictionary<Vector3, string> temp = new Dictionary<Vector3, string>();
            foreach (Vector3Int position in TILEMAP.cellBounds.allPositionsWithin) {
                Tile tile = (Tile)TILEMAP.GetTile(position);
                if (tile != null) 
                {
                    temp.Add(new Vector3(position.x, position.y, position.z), tile.name);
                }
            }

            using (Packet _packet = new Packet((int)ClientPackets.SEND_MAPDATA_TO_SERVER)) {
                _packet.Write(temp.Count); // SIZE
                _packet.Write((int)mapType); // INT maptype
                foreach (KeyValuePair<Vector3,string> data in temp) {
                    _packet.Write(data.Key); // Vector3
                    _packet.Write(data.Value); // tile name
                }

                SendTCPData(_packet);
            }
        }
        public static void DownloadLatestMapData()
    {
        print("Wysłanie proźby o przysłanie nowego pakietu mapy dla aktualnego update`a");
          using (Packet _packet = new Packet((int)ClientPackets.downloadLatestMapUpdate)) {
            _packet.Write(Client.instance.myId); // kto żdąda nowej mapki
            SendTCPData(_packet);
        }   
    }
       public static void DownloadLatestUpdateVersionNumber()
    {
        print("Wysłanie proźby oaktualny numer update'a");
          using (Packet _packet = new Packet((int)ClientPackets.download_recentMapVersion)) {
            SendTCPData(_packet);
        }   
    }
   #endregion
}
