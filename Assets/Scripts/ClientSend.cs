using System;
using System.Collections.Generic;
using System.Linq;
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
            // TODO:  podzielenie na 2 opcje odesłąnia info welcome, jendo standardowe, logowanie jako guest, 
            //          a drugie chęc sprawdzenia zgodności danych i logowanie do swojego konta
          
                print("Wysłanie do serwera pingu Welcom z id i nickiem");
                using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
                {
                    _packet.Write(Client.instance.myId);
                    _packet.Write(UIManager.Login_InputUsername.text);

                    SendTCPData(_packet);
                }

                
        }
        public static void SendLoginCreditionals(string username, string password, string MODE)
        {
            print("Wysłanie do serwera podanych danych do logowania");
                using (Packet _packet = new Packet((int)ClientPackets.LogMeIn))
                {
                    _packet.Write(Client.instance.myId);
                    _packet.Write(username);
                    _packet.Write(password);
                    _packet.Write(MODE); // rodzaj , czy logowanie czy rejestracja gracza

                    SendTCPData(_packet);
                }
                    // TODO: CZEKANIEZ A ODEBRANIEM PAKIETU< YOU R ALLOWED TO JOIN XD
        }
        public static void PlayerMovement(bool[] _inputs) 
        {
        // print("send input to server");
            using (Packet _packet = new Packet((int)ClientPackets.playerMovement)) {
                _packet.Write(_inputs.Length);
                foreach(bool _input in _inputs) {
                    _packet.Write(_input);
                }
                _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);

                SendTCPData(_packet);

            }
        }
        public static void SendChatMessage(string _message)
        {
         //   print("Wysłanie wiadomości na serwer.");
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
        public static void SendMapDataToServer(MAPTYPE mapType, LOCATIONS mapLocation) {

            GameObject locationContainer = GameManager.instance.ListaDostepnychLokalizacji.Where(n=>n.name == mapLocation.ToString()).FirstOrDefault();
           // print("1."+locationContainer.ToString());
            Tilemap TILEMAP = locationContainer.GetComponentsInChildren<Tilemap>().Select(t=>t).Where(t=>t.gameObject.name == mapType.ToString()).FirstOrDefault();
             //print("2."+TILEMAP.ToString());
            print($"Wysłanie na serwer {locationContainer.ToString()} / tilemap {TILEMAP.ToString()}");

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
                _packet.Write((int)DATATYPE.Locations);
                _packet.Write((int)mapLocation);
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
      //  print("Wysłanie proźby o przysłanie nowego pakietu mapy dla aktualnego update`a");
          using (Packet _packet = new Packet((int)ClientPackets.downloadLatestMapUpdate)) {
            _packet.Write(false); // false -> informacja że gracz chce wszystkie mapy 
            _packet.Write(Client.instance.myId); // kto żdąda nowej mapki
            SendTCPData(_packet);
        }   
    }
       public static void DownloadLatestMapData(LOCATIONS _location, MAPTYPE _maptype)
    {
        print($"Wysłanie proźby o przysłanie nowego update dla [{_location.ToString()}][{_maptype.ToString()}]");
          using (Packet _packet = new Packet((int)ClientPackets.downloadLatestMapUpdate)) {
            _packet.Write(true); // true-> informacja ze gracz ma sprecyzowane żądania
            _packet.Write(Client.instance.myId); // kto żdąda nowej mapki
            _packet.Write((int)_location);
            _packet.Write((int)_maptype);
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
 

    public static void SendServerPlayerNewLocalisation(LOCATIONS enterNewLocation)
    {
       print("Wysylanie na serwer info o zmianie lokalizacji gracza");
        using (Packet _packet = new Packet((int)ClientPackets.clientChangeLocalisation)) {
            _packet.Write((int)enterNewLocation); // int
            SendTCPData(_packet);
        }
    }

    #endregion
}
