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

     public static void LoginRespondRecieved(Packet _packet)
    {
            //TODO:
            // bool isAccesGranted = _packet.ReadBool();
            // if(isAccesGranted)
            //{
                // initiate load process and load all player data from, server
            //}
            //else
            //{
                // back to start menu
            //}
    }
    
    public static void UDPTest(Packet _packet)
    {
        string _msg = _packet.ReadString();

        Debug.Log($"Received packet via UDP. Contains message: {_msg}");
        ClientSend.UDPTestReceived();
    }
    public static void SpawnPlayer(Packet _packet)
    {
        // TODO: wykonac po akceptacji logowania ze strony serwera
        UIManager.instance.LoadGameScene();
        ClientSend.DownloadLatestUpdateVersionNumber();
            //----------------------------------------

        print("spawn");
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        Vector3 _position = _packet.ReadVector3();
        Quaternion _rotation = _packet.ReadQuaternion();
        LOCATIONS _currentLocation = (LOCATIONS)_packet.ReadInt();
        int _currentfloor = _packet.ReadInt();
        Vector3Int _tileMapCoordinates = new Vector3Int((int)_position.x,(int)_position.y,(int)_position.z);
        
        GameManager.instance.SpawnPlayer(_id,_username,_position,_rotation, _tileMapCoordinates,_currentLocation, _currentfloor);

        UIManager.instance.PrintCurrentOnlineUsers();
    }
    private static void Teleport(Packet _packet)
    {
         int _id = _packet.ReadInt();
        Vector3 _position = _packet.ReadVector3();
        print("teleport to: "+_position.ToString());

            //TODO: teleportowanie na inną mape jest niemożliwe - chyba ze sprawdzane  czy pozycja znajduje sie wewnątrz mapy, a nie bramy
            // if (GameManager.instance.LocationMaps.ContainsKey(Vector3Int.CeilToInt(_position)))
            // {
            //     GameManager.instance.EnterNewLocation(Vector3Int.CeilToInt(_position), GameManager.players[_id]);
            // }
        
       GameManager.players[_id].TeleportToPositionInGrid(new Vector3Int((int)_position.x, (int)_position.y, (int)_position.z));
    }
    public static void PlayerPosition(Packet _packet)
    {
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
    public static void UpdateChat(Packet _packet) {
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
    public static void ReceivedUpdateNumber(Packet _packet)
    {
        // porownaj wersje mapy z serwera a aktualnie zapisanej , w przypadku różnicy, pobierz z serwera nową wersje
        // PAcket zawiera json patchnotes z servera

        UpdateChecker.CacheJsonDataFromServer(_packet.ReadString());
        UpdateChecker.FindOutdatedMAPDATAFilesVersion2();
        /////////////////////////////////////////////////////////////////////////////////////////GameManager.instance.CurrentUpdateVersion = _packet.ReadInt();
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
        try
        {
              try
              {
                UpdateChecker.CLIENT_UPDATE_VERSIONS._Data[location][mapType]._Version = newMapVersion;
                UpdateChecker.SaveChangesToFile(); 
                }
                catch(Exception )
                {
                    UpdateChecker.CLIENT_UPDATE_VERSIONS._Data[location][mapType] = UpdateChecker.GetUpdateNotesFromServerWithWipedOffVersionNumbers(UpdateChecker.SERVER_UPDATE_VERSIONS)._Data[location][mapType];
                    print("dodano wyzerowaną kopie z wersji serwerowej");
                    UpdateChecker.CLIENT_UPDATE_VERSIONS._Data[location][mapType]._Version = newMapVersion;
                    print("aktualizacja versji mapy");
                    UpdateChecker.SaveChangesToFile(); 
                    // print("Tutaj bedzie trzeba dodać wyzerowaną kopie z pliku serwerowego, \nnastępnie przepuścić checka jeszcze raz do czasu gdy nie bedzie wyzerwanych elementow");
                    // print("UpdateChecker.CLIENT_UPDATE_VERSIONS._Data[location][mapType]._Version = newMapVersion;"+ex.Message);
                }


        }
        catch (System.Exception)
        {
            print("emmm brak pliku json");
        }
        
    
        LoadMapDataFromFile(location, mapType);
    }

    private static (Dictionary<Vector3Int,string> mapdata,Tilemap tilemap) GetReferencesByMaptype(LOCATIONS _location, MAPTYPE _mapType) 
    {
        // TODO: do ogarnięcia kiedyindziej, dynamiczne tworzenie i generowanie sie mapy na podstawie pozycji gracza
        // aktualnie,, są tylko 2 piętra i jest szasa dopisac to z ręki , ale nie na dluzsza mete
        int key = GetKeyFromMapLocationAndType(_location,_mapType);

        Dictionary<Vector3Int,string> mapdata = new Dictionary<Vector3Int, string>();
        Tilemap tilemap = new Tilemap();

       // print(key);

        switch(key)
        {
            case 1:
            
                mapdata = GameManager.MAPDATA_Ground;
                tilemap = GameManager.instance._tileMap_GROUND;
            break;

            case 2:
                mapdata = GameManager.MAPDATA;
                tilemap = GameManager.instance._tileMap;
            break;
            
            case 11:
            
                mapdata = GameManager.MAPDATA2ndFloor_Ground;
                tilemap = GameManager.instance._tileMap3ndFloor_GROUND;
            break;

            case 12:
                mapdata = GameManager.MAPDATA2ndFloor;
                tilemap = GameManager.instance._tileMap2ndFloor;
            break;
        }
        return (mapdata,tilemap);
    }
    public static void LoadMapDataFromFile(LOCATIONS _location, MAPTYPE _mapType)
    {
        

      // GameManager.instance.ANDROIDLOGGER.text += $"LoadMapDataFromFile {_location}{_mapType}\n";
         //   print("ładowanie mapy");
            var references = GetReferencesByMaptype(_location, _mapType);
            Tilemap REFERENCE_TILEMAP = references.tilemap;
            Dictionary<Vector3Int,string> REFERENCE_MAPDATA = references.mapdata;

          //  print($"Ladowanie danych mapy [{_mapType.ToString()}] z pliku do pamięci");
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

    internal static void RetievedLoginResponse(Packet _packet)
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

    private static void SaveMapDataToFile(LOCATIONS location, MAPTYPE mapType, Dictionary<Vector3, string> mapData)
    {
        
        string path = GetFilePath(DATATYPE.Locations,location,mapType);
        //   print(path);
        // print($"Zapisywanie danych mapy[{location.ToString()}][{mapType.ToString()}] do pliku");
        //  if (!File.Exists(path)) 
        //     {

        //         Console.WriteLine(path);
        //         return;
        //     }
       // Directory.CreateDirectory(path);


       CreateFolder(DATATYPE.Locations,location);

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

        string path = GetFilePath(DATATYPE.Locations, _location, _mapType);
        if (!File.Exists(path)) {
            //GameManager.instance.ANDROIDLOGGER.text += "Brak pliku z danymi mapy\n";
            Console.WriteLine("Brak pliku z danymi mapy"); 
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
        print("server chce mape typu: "+mapType.ToString()+" dla lokalizacji: "+ mapLocation.ToString());
        ClientSend.SendMapDataToServer(mapType,mapLocation);
        
    }


        public static string GetFilePath(DATATYPE dataType, LOCATIONS locations, MAPTYPE mapType)
        {
           // return $"{ Application.persistentDataPath}\\DATA\\{dataType.ToString()}\\{locations.ToString()}\\{mapType.ToString()}.txt";
            return $"DATA\\{dataType.ToString()}\\{locations.ToString()}\\{mapType.ToString()}.txt";

    }
    public static void CreateFolder(DATATYPE? dataType, LOCATIONS? locations)
        {
        //    Directory.CreateDirectory($"{Application.persistentDataPath}\\DATA\\{dataType.ToString()}\\{locations.ToString()}");
        Directory.CreateDirectory($"DATA\\{dataType.ToString()}\\{locations.ToString()}");
    }
        

        public static int GetKeyFromMapLocationAndType(LOCATIONS location, MAPTYPE mapType) => (int)location * 10 + (int)mapType + 1;
}
