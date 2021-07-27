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
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.Login_InputUsername.text);

            SendTCPData(_packet);
        }
    }
    public static void SendLoginCreditionals(string username, string password, string MODE)
    {
        using (Packet _packet = new Packet((int)ClientPackets.LogMeIn))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(username);
            _packet.Write(password);
            _packet.Write(MODE); // rodzaj , czy logowanie czy rejestracja gracza

            SendTCPData(_packet);
        }
    }
    public static void PlayerMovement(bool[] _inputs)
    {
        using (Packet _packet = new Packet((int)ClientPackets.playerMovement))
        {
            _packet.Write(_inputs.Length);
            foreach (bool _input in _inputs)
            {
                _packet.Write(_input);
            }
            _packet.Write(GameManager.players[Client.instance.myId].transform.rotation);
            SendTCPData(_packet);
        }
    }
    public static void SendChatMessage(string _message)
    {
        using (Packet _packet = new Packet((int)ClientPackets.SendChatMessage))
        {
            _packet.Write(Client.instance.myId);
            _packet.Write(_message);

            SendTCPData(_packet);
        }
    }
    public static void PingReceived()
    {
        using (Packet _packet = new Packet((int)ClientPackets.PingReceived))
        {
            _packet.Write(1);

            SendTCPData(_packet);
        }
    }

    internal static void SendCancellationCounting(int roomId)
    {
         using (Packet _packet = new Packet((int)ClientPackets.CancelCounter_leavingRoom))
        {
            _packet.Write(roomId);  // zmienic to na proźbe o wszystkie dostepne dungeony, a nie pojedycnzo

            SendTCPData(_packet);
        }
        
    }

    internal static void GetCurrentDungeonLobbysData(DUNGEONS dungeonType)
    {
        DungeonManager.CurrentScrollingDungeonCategory = dungeonType;
        using (Packet _packet = new Packet((int)ClientPackets.InitDataDungeonLobby))
        {
            _packet.Write((int)dungeonType);  // zmienic to na proźbe o wszystkie dostepne dungeony, a nie pojedycnzo

            SendTCPData(_packet);
        }
    }
    public static void SendMapDataToServer(MAPTYPE mapType, LOCATIONS mapLocation)
    {
        //GameObject locationContainer = GameManager.instance.ListaDostepnychMapTEST.Where(n => n.MapName == mapLocation).FirstOrDefault().Container;
        //Tilemap TILEMAP = locationContainer.GetComponentsInChildren<Tilemap>().Select(t => t).Where(t => t.gameObject.name == mapType.ToString()).FirstOrDefault();
        Tilemap TILEMAP = GameManager.instance.ListaDostepnychMapTEST.Where(n => n.MapName == mapLocation).FirstOrDefault().GetTilemapRef(mapType);

        Dictionary<Vector3, string> temp = new Dictionary<Vector3, string>();
        foreach (Vector3Int position in TILEMAP.cellBounds.allPositionsWithin)
        {
            Tile tile = (Tile)TILEMAP.GetTile(position);
            if (tile != null)
            {
                temp.Add(new Vector3(position.x, position.y, position.z), tile.name);
            }
        }

        using (Packet _packet = new Packet((int)ClientPackets.SEND_MAPDATA_TO_SERVER))
        {
            _packet.Write(temp.Count); // SIZE
            _packet.Write((int)DATATYPE.Locations);
            _packet.Write((int)mapLocation);
            _packet.Write((int)mapType); // INT maptype
            foreach (KeyValuePair<Vector3, string> data in temp)
            {
                _packet.Write(data.Key); // Vector3
                _packet.Write(data.Value); // tile name
            }

            SendTCPData(_packet);
        }
    }
    public static void DownloadLatestMapData()
    {
        using (Packet _packet = new Packet((int)ClientPackets.downloadLatestMapUpdate))
        {
            _packet.Write(false); // false -> informacja że gracz chce wszystkie mapy 
            _packet.Write(Client.instance.myId); // kto żdąda nowej mapki
            SendTCPData(_packet);
        }
    }
    public static void DownloadLatestMapData(LOCATIONS _location, MAPTYPE _maptype)
    {
        // print($"Wysłanie proźby o przysłanie nowego update dla [{_location.ToString()}][{_maptype.ToString()}]");
        using (Packet _packet = new Packet((int)ClientPackets.downloadLatestMapUpdate))
        {
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
        using (Packet _packet = new Packet((int)ClientPackets.download_recentMapVersion))
        {
            SendTCPData(_packet);
        }
    }
    public static void SendServerPlayerNewLocalisation(LOCATIONS enterNewLocation)
    {
        print("Wysylanie na serwer info o zmianie lokalizacji gracza");
        using (Packet _packet = new Packet((int)ClientPackets.clientChangeLocalisation))
        {
            _packet.Write((int)enterNewLocation); // int
            SendTCPData(_packet);
        }
    }
    public enum PlayerActions
    {
        TransformToStairs = 1
    }
    public static void SendServerPlayerActionCommand(PlayerActions playerAction, bool isActive)
    {
        using (Packet _packet = new Packet((int)ClientPackets.PlayerMakeAction))
        {
            print("WYSŁANIE DO SERWERA INFORMACJI O WYSKONANIU AKCJI: " + playerAction.ToString());
            _packet.Write((int)playerAction);   // int
            _packet.Write(isActive);            // bool
            SendTCPData(_packet);
        }
    }
    public static void TeleportMe(LOCATIONS dungeonName)
    {
        using (Packet _packet = new Packet((int)ClientPackets.TeleportMe))
        {
            _packet.Write((int)dungeonName);   // int - dungeon number
            SendTCPData(_packet);
        }
    }
    public static void CreateNewDungeonLobby(LOCATIONS dungeon, int roomId)
    {
        using (Packet _packet = new Packet((int)ClientPackets.CreateLobby))
        {
            print("wysłano do serwera proźbę o utworzenie nowego serwera");
            _packet.Write(roomId);
            _packet.Write((int)dungeon);   // int - dungeon number
            SendTCPData(_packet);
        }
    }
    public static void RemoveExistingDungeonLobby(DungeonsLobby _dungeonLobby)
    {
        using (Packet _packet = new Packet((int)ClientPackets.RemoveLobby))
        {
            print("wysłano do serwera proźbę o anulowanie/usuniecie serwera z listy ");
            _packet.Write((int)_dungeonLobby.DungeonLocation);   // int - dungeon number
            _packet.Write(_dungeonLobby.LobbyID);   // int - dungeon room id

            SendTCPData(_packet);
        }
    }
    public static void JoinLobby(DungeonsLobby _dungeonLobby)
    {
        using (Packet _packet = new Packet((int)ClientPackets.JoinLobby))
        {
            print("wysłano do serwera info ze gracz dołącza do pokoju ");
            _packet.Write(_dungeonLobby.LobbyID);   // int - dungeon room id

            SendTCPData(_packet);
        }
    }
    public static void LeaveLobbyRoom(DungeonsLobby _dungeonLobby)
    {
        using (Packet _packet = new Packet((int)ClientPackets.LeaveRoomBylayer))
        {
            print("wysłano do serwera info ze gracz wyszedł z pokoju ");
            _packet.Write(_dungeonLobby.LobbyID);   // int - dungeon room id

            SendTCPData(_packet);
        }
    }
    internal static void GroupTeleportPlayersInRoom(LOCATIONS location, int lobbyID)
    {
        using (Packet _packet = new Packet((int)ClientPackets.GroupTeleport))
        {
            _packet.Write((int)location);   // int - dungeon number
            _packet.Write(lobbyID);   // int - lobby id
            SendTCPData(_packet);
        }
    }
    internal static void GroupEnteredDungeon(int lobbyID)
    {
        // wyslanie info do wszystkich innych graczy, aby ci schowali graczy xD
        print("wyslanie info do serwera, że grupa rozpoczela gre, i trzeba Grupe zablokowac");
        using (Packet _packet = new Packet((int)ClientPackets.GroupEnteredDungeon))
        {
            _packet.Write(lobbyID);   // int - lobby id
            SendTCPData(_packet);
        }
    }

    internal static void GroupLeaveTeleport(int lobbyID)
    {
         // wyslanie info do wszystkich innych graczy, aby ci schowali graczy xD
        print("wyslanie info do serwera, że grupa rozpoczela gre, i trzeba Grupe zablokowac");
        using (Packet _packet = new Packet((int)ClientPackets.GroupLeaveTeleport))
        {
            _packet.Write(lobbyID);   // int - lobby id
            SendTCPData(_packet);
        }
    }
    #endregion
}
