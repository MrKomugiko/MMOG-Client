using System.Security.Principal;
using System.Data.Common;
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
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt(); // nasze serwerowe id ( tymczasowe, zmienne )
        Client.instance.myId = _myId;

        Debug.Log($"Zwykle polaczenie z serwerem sukces ");
        print("server:"+_msg);
        
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
        UIManager.instance.LoadingAnimation.ReceivedMessageFromServer = true;
        UIManager.instance.startMenu.SetActive(true);
        UIManager.instance.reconnectWindow.SetActive(false);
        UIManager.instance.RegistrationWindow.GetComponent<WindowScript>().ShowServerMessage("CONNECTION-SUCCES");
        
        ClientSend.DownloadLatestUpdateVersionNumber();
    }
    
    
    public static void SpawnPlayer(Packet _packet)
    {
        print("spawn");
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();

        if(Client.instance.myId == _id)
        {
            UIManager.instance.LoadGameScene();
            ClientSend.DownloadLatestUpdateVersionNumber();
        }
 
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        LOCATIONS _currentLocation = (LOCATIONS)_packet.ReadInt();
        int _currentfloor = _packet.ReadInt();
        Vector3Int _tileMapCoordinates = new Vector3Int((int)_position.x,(int)_position.y,(int)_position.z);
        
        int _dungeonRoomID =_packet.ReadInt();

        GameManager.instance.SpawnPlayer(_id,_username,_position,_rotation, _tileMapCoordinates,_currentLocation, _currentfloor,_dungeonRoomID);

        UIManager.instance.PrintCurrentOnlineUsers();
    }
    private static void Teleport(Packet _packet)
    {
        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        
        print("teleport to: "+_position.ToString());

        if(GameManager.players[_id].dungeonRoom != null)
        {
            var room = GameManager.players[_id].dungeonRoom;
            if(room.Players.Contains(GameManager.players[_id].Username))
            {
                tp(_id, _position);
                return;
            }
            else
            {
                print("teleport blocked - stop spawn other people in your room ;d");
                return;
            }
        }

        tp(_id, _position);

        static void tp(int _id, Vector3 _position)
        {
            if (GameManager.instance.LocationMaps.ContainsKey(Vector3Int.CeilToInt(_position)))
            {
                GameManager.instance.EnterNewLocation(Vector3Int.CeilToInt(_position), GameManager.players[_id]);
            }
            GameManager.players[_id].TeleportToPositionInGrid(new Vector3Int((int)_position.x, (int)_position.y, (int)_position.z));
        }
    }
    public static void PlayerPosition(Packet _packet)
    {
        if(GameManager.instance.LogedIn == false) return;

        bool teleport = _packet.ReadBool();
        if(teleport)
        {
            Teleport(_packet);
            return;
        }

        int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();

        if (GameManager.instance.LocationMaps.ContainsKey(Vector3Int.CeilToInt(_position)))
        {
            GameManager.instance.EnterNewLocation(Vector3Int.CeilToInt(_position), GameManager.players[_id]);
        }
        //}
        // w przypadku gdy gracz dostanie w odpowiedzi niezmieniona aktualna pozycje nie ma potrzebny wykonywac animacji
        if(Vector3Int.CeilToInt(_position) == GameManager.players[_id].CurrentPosition_GRID)
        {
            GameManager.players[_id].movementScript.movingAnimationInProgress = false;
            GameManager.players[_id].movementScript.waitingForServerAnswer = false;
        }
        else
        {
            // zostanie wykonany jakis ruch
            GameManager.players[_id].MoveToPositionInGrid(Vector3Int.CeilToInt(_position));
        }
    }
    
    
    public static void UpdateChat(Packet _packet) 
    {
        string _msg = _packet.ReadString();
        //print("Odebrano wiadomosc od GM: " + _msg);

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

        if(Client.instance.myId == _id)
        {
            // to znaczy ze server nas Kicknął ;x
            print("zostałeś wyrzucony z serwera");

            Client.instance.Disconnect();
        }
        
        // usunięcie obiektu gracza
            Destroy(GameManager.players[_id].gameObject);
        // usunięcie afka z listy graczy
            GameManager.players.Remove(_id);

            UIManager.instance.PrintCurrentOnlineUsers();
    }
    public static void PingBackToServer(Packet _packet) 
    {
        // Heartbit d;
        ClientSend.PingReceived();
        UIManager.instance.PrintCurrentOnlineUsers();
    }

  
    public static void RemoveNonExistingRoomFromScene(Packet _packet)
    {
        Console.WriteLine("usuniecie obiektu pokoju ze sceny i pamieci");
        DUNGEONS dungeon = (DUNGEONS)_packet.ReadInt();
        int roomID = _packet.ReadInt();

        DungeonManager.instance.DiscposeRoom(dungeon,roomID);
    }
    public static void KickFromDungeonRoom(Packet _packet)
    {
        try
        {
            print("get info from server, you have to leave current room / kicked");
            var dungeon = DungeonManager.GetDungeonLobbyByRoomID(_packet.ReadInt());
            if(dungeon == null) return; 
            DungeonManager.instance.LeaveRoom(dungeon.LobbyID, Client.instance.myId);
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }
    public static void RetievedUpdatedDungeonList(Packet _packet)
    {
        print("otrzymano uaktualniona baze dungeon lobby rooms");
        string _action = _packet.ReadString();
        print("action =>"+_action);
           
        // odpakowywanie obiektu dungeonLobby
        List<DungeonsLobby> updatedDungeonLobbysListFromServer = new List<DungeonsLobby>();

        int listCount = _packet.ReadInt();
        print("olistCount => "+listCount);  

        for (int i = 0; i < listCount; i++)
        {
            DungeonsLobby lobby = new DungeonsLobby(
               lobbyID: _packet.ReadInt(),
               lobbyOwner: _packet.ReadString(),
               dungeonLocation: (LOCATIONS)_packet.ReadInt(),
               maxPlayersCapacity: _packet.ReadInt(),
               isStarted: _packet.ReadBool()
            
                );
               Debug.LogWarning(lobby.IsStarted);
            List<String> playersList = new List<string>();
            int playersCount = _packet.ReadInt();
            for (int j = 0; j < playersCount; j++)
            {
                playersList.Add(_packet.ReadString());
            }
            lobby.Players = playersList;
        
            switch(_action)
            {
                case "PlayerLeftRooom":
                    print("dodatkowa akcja do wykonania: "+_action);
                    print("np zablokowanie opcji startu, nie wiem :D?");
                    break;
                
                case "PlayerJoinToRoom":
                    print("gracz dolaczyl do pokoju");
                break;
                
                case "GameStarted":
                    print($"Room [{lobby.LobbyID}] wystartował grę. Następuje zablokowanie dostępu do tego pokoju.");   
                    lobby.IsStarted = true;
                break;
            }

            updatedDungeonLobbysListFromServer.Add(lobby);
        }

        foreach(DungeonsLobby lobbyRoom in updatedDungeonLobbysListFromServer)
        {
            DungeonManager.instance.UpdateLobbyData(lobbyRoom.LobbyID, lobbyRoom);
        } 
    }


    internal static void RemoveItemFromMap(Packet _packet)
    {
        print("gracz podniosl x item");
        LOCATIONS _location = (LOCATIONS)_packet.ReadInt();
        MAPTYPE _mapType = (MAPTYPE)_packet.ReadInt();
        Vector3Int position_grid = Vector3Int.CeilToInt(_packet.ReadVector3());


        var refference = GetReferencesByMaptype(_location, _mapType);        
        refference.mapdata[position_grid] = "";
        refference.tilemap.SetTile(position_grid,null);
    }
    internal static void CollectAndPickUPItem(Packet _packet)
    {
        int whiPickItem = _packet.ReadInt(); // INT server current player ID

        Item itemFromServer = new Item(
            _packet.ReadInt(),     // ID
            _packet.ReadString(),  // Name 
            _packet.ReadInt(),     // Value             
            _packet.ReadInt(),    // Level                                         
            _packet.ReadBool(),     // Stackable                                        
            _packet.ReadInt(),     // Stack size                             
            _packet.ReadString()   // Description                                                                                  
        );
        
        Console.WriteLine($"Podniosleś przedmiot: {itemFromServer.ToString()}");
        Inventory.Items_LIST.Add(itemFromServer);

        InventoryScript.instance.InventoryDATA.AddItemToInventory(itemFromServer);
        
    }


    public static void RetievedLoginResponse(Packet _packet)
    {
        string meessageFromServer = _packet.ReadString();
        print(meessageFromServer);
        UIManager.Login_InputUsername.interactable = true; // mozliwosc ponownego wproawdzenia danych
        UIManager.instance.RegistrationWindow.GetComponent<WindowScript>().ShowServerMessage(meessageFromServer);
    }

    public static void RetievedRegistrationResponse(Packet _packet)
    {
        string response = _packet.ReadString();
        print("Status rejestracji konta: "+response);
        UIManager.instance.RegistrationWindow.GetComponent<WindowScript>().ShowServerMessage(response);
        UIManager.instance.LoadingAnimation.ReceivedMessageFromServer = true;
    }

    
    public static void ReceivedUpdateNumber(Packet _packet)
    {
        UpdateChecker.CacheJsonDataFromServer(_packet.ReadString());
        UpdateChecker.FindOutdatedMAPDATAFilesVersion2();
    }
    public static void NewMapDataFromServerReceived(Packet _packet)
    {
        int newMapVersion = _packet.ReadInt();
        LOCATIONS location = (LOCATIONS)_packet.ReadInt();
        MAPTYPE mapType = (MAPTYPE)_packet.ReadInt();
        int mapSize = _packet.ReadInt();
        Dictionary<Vector3,string> brandNewMap = new Dictionary<Vector3, string>();
        for(int i =0;i<mapSize;i++)
        {
            brandNewMap.Add(_packet.ReadVector3(),_packet.ReadString());
        }

        print($"Otrzymales uaktualnioną [{location.ToString()}][{mapType.ToString()}] wielosc: [{brandNewMap.Count}]");

        SaveMapDataToFile(location, mapType, brandNewMap);

        if(UpdateChecker.CLIENT_UPDATE_VERSIONS == null) 
        {
            print("no pathnote file, assign exact file from server, but remove updateverions number");
            UpdateChecker.CLIENT_UPDATE_VERSIONS = UpdateChecker.GetUpdateNotesFromServerWithWipedOffVersionNumbers(UpdateChecker.SERVER_UPDATE_VERSIONS);
            UpdateChecker.SaveChangesToFile();
        }

        if(UpdateChecker.CLIENT_UPDATE_VERSIONS._Data._Locations.ElementAtOrDefault((int)location) == null)
        {
            Debug.LogWarning("dodano brakujaca lokacje z pamieci");
            UpdateChecker.CLIENT_UPDATE_VERSIONS._Data._Locations.Add(UpdateChecker.GetUpdateNotesFromServerWithWipedOffVersionNumbers(UpdateChecker.SERVER_UPDATE_VERSIONS)._Data[location]);
        }

        UpdateChecker.CLIENT_UPDATE_VERSIONS._Data[location][mapType]._Version = newMapVersion;
        UpdateChecker.SaveChangesToFile(); 
    
        LoadMapDataFromFile(location, mapType);
    }

   public static void FinishDungeonAndLeaveRoom(Packet _packet)
    {
        // run timer +
        print("odebrano pakiet stoperka ;d");
        int roomId = _packet.ReadInt();
        // int LobbyId = GameManager.GetLocalPlayer().dungeonRoom.LobbyID;
        print("FinishDungeonAndLeave setting counter");
        Action<int> action = new Action<int>(DungeonManager.instance.BackToTown);
        GameManager.instance.Counter.SetCounter(action, roomId, "leaviong dungeon...", 5);
    }

    private static (Dictionary<Vector3Int,string> mapdata,Tilemap tilemap) GetReferencesByMaptype(LOCATIONS _location, MAPTYPE _mapType) 
    {
        var mapdata = new Dictionary<Vector3Int, string>();
        var tilemap = new Tilemap();

        var mapContainer = GameManager.instance.ListaDostepnychMapTEST.Where(m=>m.MapName == _location).First();
                mapdata = mapContainer.GetMapdataRef(_mapType);
                tilemap = mapContainer.GetTilemapRef(_mapType);
      
        return (mapdata,tilemap);
    } 
    public static void LoadMapDataFromFile(LOCATIONS _location, MAPTYPE _mapType)
    {
            var references = GetReferencesByMaptype(_location, _mapType);
            Tilemap REFERENCE_TILEMAP = references.tilemap;
            Dictionary<Vector3Int,string> REFERENCE_MAPDATA = references.mapdata;

            int modifiedCounter = 0, wrongDataRecords = 0, deletedCounter = 0, newAddedCounter = 0;
            Dictionary<Vector3Int, string> TEMP_MAPDATA_FROM_FILE = ReadMapDataFromFile(_location, _mapType);

            references.tilemap.ClearAllTiles();
             // ---------- MODYFIKACJA ISTNIEJĄCYCH DANYCH SERVERA
            // --------- JEZELI NIE MA ZAPISANYCH DANYCH NA SERWERZE Z AUTOMATU WSZYSTKO PRZYPISUJEMY JAK Z PLIKU
            if (REFERENCE_MAPDATA.Count == 0)
            {
                REFERENCE_MAPDATA = TEMP_MAPDATA_FROM_FILE;
            }
            if (REFERENCE_MAPDATA.Count > 0)
            {
                if (TEMP_MAPDATA_FROM_FILE.Count == 0) print("Plik jest pusty -> Brak zapisanych danych mapy");

                // porownanie i dodanie/zamiana danych z istniejącym zapisem w pamiec
                foreach (var kvp in TEMP_MAPDATA_FROM_FILE)
                {
                    if (REFERENCE_MAPDATA.ContainsKey(kvp.Key))
                    {
                        if (REFERENCE_MAPDATA[kvp.Key] != kvp.Value)
                        {
                            REFERENCE_MAPDATA[kvp.Key] = kvp.Value;
                            modifiedCounter++;
                        }
                    }
                    else
                    {
                        REFERENCE_MAPDATA.Add(kvp.Key, kvp.Value);
                        newAddedCounter++;
                    }
                }
               // usuniecie nieaktualnych pól
               
                foreach (var pole in REFERENCE_MAPDATA.Where(pole => TEMP_MAPDATA_FROM_FILE.ContainsKey(pole.Key) == false).Select(pole => pole.Key).ToList()) 
                {
                    REFERENCE_MAPDATA.Remove(pole);
                    deletedCounter++;
                }
            }

        // ----------------------------------PODSUMOWANIE ----------------------------------
        GameManager.instance.ANDROIDLOGGER.text += $"\nOdczytano: {TEMP_MAPDATA_FROM_FILE.Count}\n";
            print(
                $"Odczytano: .................. {TEMP_MAPDATA_FROM_FILE.Count}\n" +
                $"Dodano: ..................... {newAddedCounter}\n" +
                $"Zmodyfikowano: .............. {modifiedCounter}\n" +
                $"Usunięto: ................... {deletedCounter}\n" +
                $"Uszkodzonych danych: ........ {wrongDataRecords}");

            PopulateTilemapWithCorrectTiles(_data: REFERENCE_MAPDATA, _tilemap: REFERENCE_TILEMAP);
    }
    private static void SaveMapDataToFile(LOCATIONS location, MAPTYPE mapType, Dictionary<Vector3, string> mapData)
    {
        
        string path = Constants.GetFilePath(DATATYPE.Locations,location,mapType);

       Constants.CreateFolder(DATATYPE.Locations,location);

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (TextWriter tw = new StreamWriter(fs))
                
            foreach (KeyValuePair<Vector3, string> kvp in mapData)
            {
                tw.WriteLine(string.Format("{0} {1}", kvp.Key, kvp.Value));
            }
        }
    }
    private static Dictionary<Vector3Int, string> ReadMapDataFromFile(LOCATIONS _location, MAPTYPE _mapType)
    {
     //   GameManager.instance.ANDROIDLOGGER.text += "ReadMapDataFromFile\n";
        var TEMP_MAPDATA_FROM_FILE = new Dictionary<Vector3Int, string>();


        string path =Constants. GetFilePath(DATATYPE.Locations, _location, _mapType);
        if (!File.Exists(path)) {
            //GameManager.instance.ANDROIDLOGGER.text += "Brak pliku z danymi mapy\n";
            print("Brak pliku z danymi mapy"); 
            using (StreamWriter sw = File.CreateText(path))
            {
                // sw.WriteLine();
            }	
       //     throw new Exception("brak pliku");
        };

        string line;
        StreamReader file = new StreamReader(path);
        
        while ((line = file.ReadLine()) != null)
        {
            string text = line.Replace("(", "").Replace(")", "");
            string[] data = text.Split(" ".ToCharArray());

            string x = data[0].Trim().Replace(",", ".");
            string y = data[1].Trim().Replace(",", ".");
            string z = data[2].Trim().Replace(",", ".");

            int iX = Int32.Parse(x.Remove(x.Length - 3));
            int iY = Int32.Parse(y.Remove(y.Length - 3));
            int iZ = Int32.Parse(z.Remove(z.Length - 2));

            string value = data[3];

            TEMP_MAPDATA_FROM_FILE.Add(new Vector3Int(iX, iY, iZ), value);

        }
            file.Close();
            

        return TEMP_MAPDATA_FROM_FILE;
    }
    private static void PopulateTilemapWithCorrectTiles(Dictionary<Vector3Int, string> _data, Tilemap _tilemap)
    {
        //GameManager.instance.ANDROIDLOGGER.text += "PopulateTilemapWithCorrectTiles\n";
        Console.WriteLine("zastosowywanie zmian w mapie, podmiana tilesów na aktualne");
        foreach (KeyValuePair<Vector3Int, string> kvp in _data)
        {
            Tile tile = (Tile)_tilemap.GetTile(kvp.Key);
            if (tile != null)
            {
                // Podmiana istniejącego tilesa na inny
                string tileName = GameManager.instance.listaDostepnychTilesow.Where(t => t.name == kvp.Value).First().name;
                _tilemap.SetTile(kvp.Key, GameManager.instance.listaDostepnychTilesow.Where(t => t.name == tileName).First());
            }
            else
            {
                // dodanie nowego tilesa w puste miejsce
                _tilemap.SetTile(kvp.Key, GameManager.instance.listaDostepnychTilesow.Where(t => t.name == kvp.Value).First());
            }
        }
    }
    public static void SendMapToServer(Packet _packet)
    {
        // determine what map type server need from us
        MAPTYPE mapType =  (MAPTYPE)_packet.ReadInt();
        LOCATIONS mapLocation = (LOCATIONS)_packet.ReadInt();
        print($"send map to server: [{mapLocation.ToString()}][{mapType.ToString()}]");
        ClientSend.SendMapDataToServer(mapType,mapLocation);
    }
}
