using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        Vector3Int _tileMapCoordinates = new Vector3Int((int)_position.x,(int)_position.y,(int)_position.z);
        
        GameManager.instance.SpawnPlayer(_id,_username,_position,_rotation, _tileMapCoordinates);
    }

    public static void PlayerPosition(Packet _packet) {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

//        print(_position.x+" "+_position.y+" "+_position.z);
       // GameManager.players[_id].transform.position = _position;
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

        // sprawdzenie czy id gracza istnieje 
        if(!GameManager.players.ContainsKey(_id)) return;

        // usunięcie offline tilesa
        // ostatnia zarejestrowana pozycja gracza:
        var offlinePosition = GameManager.players[_id].CurrentPosition_GRID;
        var isThereMorePlayersInOnePlace = PlayerManager.CheckIfMorePlayersStayOnThisPosition(offlinePosition);
        if(isThereMorePlayersInOnePlace)
        {   
            // ktos stoi na miejscu afka, trzeba usunac kolor afka i przypisac kolor aktywnego
            GameManager.instance._tileMap.SetTile(offlinePosition,PlayerManager.OtherAvaiablePlayerTileAtThisPosition(offlinePosition,_id));
        }
        else
        {
            // nikogo innego nie ma w miejscu afka, czyscimy pole klasycznie
            GameManager.instance._tileMap.SetTile(offlinePosition,null);
        }

        // usunięcie obiektu gracza
        Destroy(GameManager.players[_id].gameObject);
        // usunięcie afka z listy graczy
       // GameManager.players.Remove(_id);

    }

    public static void PingBackToServer(Packet _packet) {
        ClientSend.PingReceived();
    }

    public static void ReceivedUpdateNumber(Packet _packet)
    {
        print($"Current update version is [{GameManager.CurrentUpdateVersion}]. New update on server is {_packet.ReadInt()}");
        // Poproś o nową wersje mapy
        ClientSend.DownloadLatestMapUpdate();
    }

    public static void NewMapDataFromServerReceived(Packet _packet)
    {
        int mapSize = _packet.ReadInt();
        Dictionary<Vector3,string> brandNewMap = new Dictionary<Vector3, string>();
        for(int i =0;i<mapSize;i++)
        {
            brandNewMap.Add(_packet.ReadVector3(),_packet.ReadString());
        }

        print($"Otrzymales nowiutką mape: [{brandNewMap.Count}]");

        // TODO: zapisanie mapy w pamieci ?
            ZapiszMapeDoPliku(brandNewMap);
            LoadMapDataFromFile();
        // TODO: zapis i odczyt mapy z pliku => zeby nie sciągać jej później // luz server i tak sprawdza ze swoją orginalną kopią ;d

    }
       private static void ZapiszMapeDoPliku(Dictionary<Vector3, string> mapData)
        {
            string path = "MAPDATA2.txt";
            print(path);
            print("Zapisywanie danych mapy do pliku");
            using (FileStream fs = new FileStream(path, FileMode.Create))
            {
                using (TextWriter tw = new StreamWriter(fs))
                    
                foreach (KeyValuePair<Vector3, string> kvp in mapData)
                {
                    tw.WriteLine(string.Format("{0} {1}", kvp.Key, kvp.Value));
                }
            }
        }
        public static void LoadMapDataFromFile()
        {
            string path = "MAPDATA2.txt";

            print("Ladowanie danych mapy z pliku do pamięci");
            var mapData = new Dictionary<Vector3Int,string>();
            if (!File.Exists(path)) return;
            // ----------------------------------ZCZYTYWANIE Z PLIKU ----------------------------------
            string line;

            int modifiedCounter = 0;
            int wrongDataRecords = 0;
            int deletedCounter = 0;
            int newAddedCounter = 0;

            
            StreamReader file = new StreamReader(path);  
            while((line = file.ReadLine()) != null)  
            {  
                string text = line.Replace("(","").Replace(")","");
                string[] data = text.Split(" ".ToCharArray());

               try {
                    string x = data[0].Trim().Replace(",", ".");
                    string y=data[1].Trim().Replace(",", ".");
                    string z =data[2].Trim().Replace(",", ".");
                    // print($"[{x}][{y}][{z}]");
                    x = x.Remove(x.Length-3);
                    y = y.Remove(y.Length-3);
                    z = z.Remove(z.Length-2); 
                    // print($"[{x}][{y}][{z}]");
                    string value = data[3];

                    mapData.Add(new Vector3Int(Int32.Parse(x),Int32.Parse(y),Int32.Parse(z)), value);
               }
               catch(System.FormatException ex) {
                   print("zły format, zle zaladowana lokalizacja Vector3 => "+text+" Error: "+ex.Message );
                   wrongDataRecords++;
               }
               catch(Exception ex) {
                   print(ex.Message);
               }
            }  
            file.Close();

            // ----------------------------------ZAPISYWANIE W PAMIECI KLIENTA ----------------------------------
            // --------- JEZELI NIE MA ZAPISANYCH DANYCH NA SERWERZE
            if (GameManager.MAPDATA.Count == 0) 
            {
                GameManager.MAPDATA = mapData;
            }
            // ---------- MODYFIKACJA ISTNIEJĄCYCH DANYCH SERVERA
            if (GameManager.MAPDATA.Count > 0) 
            {
                if (mapData.Count == 0) print("Plik jest pusty -> Brak zapisanych danych mapy");

                // porownanie i dodanie/zamiana danych z istniejącym zapisem w pamiec
                foreach (var kvp in mapData) {
                    if (GameManager.MAPDATA.ContainsKey(kvp.Key)) {
                        if (GameManager.MAPDATA[kvp.Key] != kvp.Value) {
                            GameManager.MAPDATA[kvp.Key] = kvp.Value;
                            modifiedCounter++;
                        }
                    }else {
                        GameManager.MAPDATA.Add(kvp.Key, kvp.Value);
                        newAddedCounter++;
                    }
                }

                // usuniecie nieaktualnych pól
                foreach (var pole in GameManager.MAPDATA.Where(pole => mapData.ContainsKey(pole.Key) == false).Select(pole => pole.Key).ToList()) {
                    GameManager.MAPDATA.Remove(pole);
                    deletedCounter++;
                }
            }

           // ----------------------------------PODSUMOWANIE ----------------------------------
            print(
                $"Odczytano: .................. {mapData.Count}\n"+
                $"Dodano: ..................... {newAddedCounter}\n" +
                $"Zmodyfikowano: .............. {modifiedCounter}\n" +
                $"Usunięto: ................... {deletedCounter}\n" +
                $"Uszkodzonych danych: ........ {wrongDataRecords}");


        // PODMIANA DANYCH MAPY 
            foreach(var kvp in GameManager.MAPDATA)
            {
                var tile = (Tile)GameManager.instance._tileMap.GetTile(kvp.Key);
                if(tile != null)
                {
                    // nie podmieniac pozycji graczy ( moze kiedys ale zakladajac ze mapa bedzie aktualizowana w bezpieczny sposob dla innych graczy, zeby nie zabudowac ich w scianie czy cos)
                    if(tile.name == "player") continue;
                    if(tile.name == "localPlayer") continue;

                    string name1 = GameManager.instance.listaDostepnychTilesow.Where(t=>t.name == kvp.Value).First().name;
                    
                    GameManager.instance._tileMap.SetTile(kvp.Key, GameManager.instance.listaDostepnychTilesow.Where(t=>t.name == name1).First());
                }
                else
                {
                    // trzeba wstawic nowego tilesa
                    if(kvp.Value == "player") continue;
                    if(kvp.Value == "localPlayer") continue;
                    GameManager.instance._tileMap.SetTile(kvp.Key, GameManager.instance.listaDostepnychTilesow.Where(t=>t.name == kvp.Value).First());
                }

            }
        } 
}
