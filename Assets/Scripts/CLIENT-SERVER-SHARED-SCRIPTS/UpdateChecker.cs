using System.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
public class UpdateChecker : MonoBehaviour
{

    public static UPDATE_NOTES CLIENT_UPDATE_VERSIONS;
    public static UPDATE_NOTES SERVER_UPDATE_VERSIONS;

    public static int ElementsCount = 0;
    public static bool isReady = false;

    private void Awake() {
        SERVER_UPDATE_VERSIONS = new UPDATE_NOTES();
        CLIENT_UPDATE_VERSIONS = new UPDATE_NOTES();
        CLIENT_UPDATE_VERSIONS = ReadDataFromFile(Constants.PATH_NOTES_CLIENT);

        //SERVER_UPDATE_VERSIONS = ReadDataFromFile(@"DATA\UpdateNotes_TEST_SERVER.json");
    }

   [ContextMenu("init")]
    public void IniciateData_TEST() {
        // if (isReady) return;
        // isReady = true;

        CLIENT_UPDATE_VERSIONS = new UPDATE_NOTES();
        CLIENT_UPDATE_VERSIONS._Data =
            new DATA() {
                _Locations = new List<Locations>()
                {
                        new Locations() {
                            _Id = 0,
                            _Name = "Start_First_Floor",
                            _Coordinates = new Vector3_json(7, -2, 14),
                            _Type = new List<Maptypes>()
                        {
                                new Maptypes() {
                                    _Type = "Ground_MAP",
                                    _Version = 1001
                                },
                                new Maptypes() {
                                    _Type = "Obstacle_MAP",
                                    _Version = 1001
                                }
                            }
                        },
                        new Locations() {
                            _Id = 1,
                            _Name = "Start_Second_Floor",
                            _Coordinates = new Vector3_json(7, -2, 14),

                            _Type = new List<Maptypes>()
                        {
                                new Maptypes() {
                                    _Type = "Ground_MAP",
                                    _Version = 1001
                                },
                                new Maptypes() {
                                    _Type = "Obstacle_MAP",
                                    _Version = 1001
                                }
                            }
                        }
                },
                _Items = new List<Items>() {
                        new Items(
                            id: (int)ITEMS.Armor,
                            name: ITEMS.Armor.ToString(),
                            type: "Wearable"
                        ),
                        new Items(
                            id: (int)ITEMS.Stone,
                            name: ITEMS.Stone.ToString(),
                            type: "Trash"
                        ),
                        new Items(
                            id : (int)ITEMS.Health_Potion,
                            name : ITEMS.Health_Potion.ToString(),
                            type :"Consumable"
                        )
                }
            };
        string clientString = JsonUtility.ToJson(CLIENT_UPDATE_VERSIONS,true);
            print(clientString);
    }
    
    public static void SaveChangesToFile(){
       string jsonText = (JsonUtility.ToJson(CLIENT_UPDATE_VERSIONS));

        using (FileStream fs = new FileStream(Constants.PATH_NOTES_CLIENT, FileMode.Create)) {
            using (TextWriter tw = new StreamWriter(fs)) {
                tw.WriteAsync(jsonText);
            }
        }
    }
    public static UPDATE_NOTES ReadDataFromFile(string path) {
        //D:\Programowanie\Unity\MMOG-Client\DATA\Client_UpdateNotes.json
        if(File.Exists(path) == false)
        {
            print("plik 'DATA\\Client_UpdateNotes.json' nie istnieje");

            return null;
        } 
        string jsonText = File.ReadAllText(path);
        return JsonUtility.FromJson<UPDATE_NOTES>(jsonText);
    }
    public static void CacheJsonDataFromServer(string dataFromServer) {

        SERVER_UPDATE_VERSIONS = JsonUtility.FromJson<UPDATE_NOTES>(dataFromServer);

    }

    public static int GetVersionOf(UPDATE_NOTES source, LOCATIONS _location, MAPTYPE _maptype, DATATYPE _datatype = DATATYPE.Locations) => source._Data[_location][_maptype]._Version;
    public static int GetVersionOf(UPDATE_NOTES source, ITEMS _item, DATATYPE _datatype = DATATYPE.Items) => source._Data[_item]._Version;

    public static UPDATE_NOTES GetPathNoteWithClearMAPDATASVersionNumbers(UPDATE_NOTES sERVER_UPDATE_VERSIONS)
    {
        var newTempUpdateNotes = new UPDATE_NOTES();
        newTempUpdateNotes = sERVER_UPDATE_VERSIONS;
        
        var LocationCount = Enum.GetNames(typeof(LOCATIONS)).Length;
        var mapTypesCount = Enum.GetNames(typeof(MAPTYPE)).Length;
        for (int location = 0; location < LocationCount; location++) {
            for (int maptype = 0; maptype < mapTypesCount; maptype++) {
                newTempUpdateNotes._Data[(LOCATIONS)location][(MAPTYPE)maptype]._Version = 0000;
            }
       }
        return newTempUpdateNotes ;
    }


    //[ContextMenu("Find outdated files")]
    //public void FindOutdatedMAPDATAFiles() {
    //    LOCATIONS LOCATIONS_Enum;
    //    MAPTYPE MAPTYPE_Enum;

    //    foreach (var S_Location in SERVER_UPDATE_VERSIONS._Data._Locations) {
    //        foreach (var S_Maptype in S_Location._Type) {
    //            var C_Maptype = CLIENT_UPDATE_VERSIONS._Data._Locations
    //                .Where(location => location._Id == S_Location._Id)
    //                .Select(maptype => maptype._Type
    //                    .Where(type => type._Type == S_Maptype._Type)
    //                    .First())
    //                .First();

    //            if (S_Maptype._Version != C_Maptype._Version) {
    //                print($"wyślij żądanie uaktualnienia dla: [{S_Location._Name}] [{C_Maptype._Type}] [client:{C_Maptype._Version} | server:{S_Maptype._Version}]");
    //                LOCATIONS_Enum = (LOCATIONS)Enum.Parse(typeof(LOCATIONS), S_Location._Name);
    //                MAPTYPE_Enum = (MAPTYPE)Enum.Parse(typeof(MAPTYPE), C_Maptype._Type);
    //                ClientSend.DownloadLatestMapData(LOCATIONS_Enum, MAPTYPE_Enum);
    //            }
    //        }
    //    };
    //}

    [ContextMenu("Find outdated files [Version 2.]")]
    public static void FindOutdatedMAPDATAFilesVersion2() {
        var LocationCount = Enum.GetNames(typeof(LOCATIONS)).Length;
        var mapTypesCount = Enum.GetNames(typeof(MAPTYPE)).Length;

        // sprawdzenie czy dane zostały załadowane i czy istnieja
        if(CLIENT_UPDATE_VERSIONS == null)
         {
            print("pusty klient patchnotes, pomin sprawdzanie, pobierz z serwera cały pakiet xD");
            ClientSend.DownloadLatestMapData();
            return;
        }

        for (int location = 0; location < LocationCount; location++) {
            for (int maptype = 0; maptype < mapTypesCount; maptype++) {
            
                int clientVersion = GetVersionOf(CLIENT_UPDATE_VERSIONS, (LOCATIONS)location, (MAPTYPE)maptype);
                int servertVersion = GetVersionOf(SERVER_UPDATE_VERSIONS, (LOCATIONS)location, (MAPTYPE)maptype);

                print($"MAPA: [{(LOCATIONS)location}][{(MAPTYPE)maptype}]  Client:{clientVersion} | Server:{servertVersion}");

                if(clientVersion != servertVersion) ClientSend.DownloadLatestMapData((LOCATIONS)location, (MAPTYPE)maptype);
            }
        }
    }

  
}