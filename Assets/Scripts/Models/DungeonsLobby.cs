using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class DungeonsLobby
{
    [SerializeField] private int lobbyID;
    [SerializeField] private string lobbyOwner;
    [SerializeField] private DUNGEONS dungeonLocation;
    [SerializeField] private List<string> players = new List<string>();
    [SerializeField] private int maxPlayersCapacity;
    [SerializeField] private bool isStarted;

    public GameObject lobbySceneObjectRefference
    {
        get => DungeonManager.instance.ListOfDungeonLobby_GameObject
            .Where(room => room.name == ("ROOM_" + LobbyID))
            .FirstOrDefault();
    }

    public int LobbyID { get => lobbyID; private set => lobbyID = value; }
    public string LobbyOwner { get => lobbyOwner; private set => lobbyOwner = value; }
    public DUNGEONS DungeonLocation { get => dungeonLocation; private set => dungeonLocation = value; }
    public List<string> Players { get => players; set => players = value; }
    public int PlayersCount { get => Players.Count; }
    public int MaxPlayersCapacity { get => maxPlayersCapacity; private set => maxPlayersCapacity = value; }
    public bool IsFull { get => PlayersCount >= MaxPlayersCapacity ? true : false; }
    public bool IsStarted { get=>isStarted;  set => isStarted = value; }

    public DungeonsLobby(int lobbyID, string lobbyOwner, DUNGEONS dungeonLocation, int maxPlayersCapacity = 2, List<string> players = null, bool isStarted = false)
    {
        LobbyID = lobbyID;
        LobbyOwner = lobbyOwner;
        DungeonLocation = dungeonLocation;
        MaxPlayersCapacity = maxPlayersCapacity;
        IsStarted = isStarted;

        if (players == null)
            {
                Players.Add(LobbyOwner);
            }
        else
        {
            Players = players;
        }
    }
    public DungeonsLobby(int lobbyID, string lobbyOwner, LOCATIONS dungeonLocation, int maxPlayersCapacity = 2, List<string> players = null, bool isStarted = false)
    {
        LobbyID = lobbyID;
        LobbyOwner = lobbyOwner;
        DUNGEONS dungeonloc;
        Enum.TryParse<DUNGEONS>(dungeonLocation.ToString(), out dungeonloc);
        DungeonLocation = dungeonloc;
        MaxPlayersCapacity = maxPlayersCapacity;
        IsStarted = isStarted;

        if (players == null)
            {
                Players.Add(LobbyOwner);
            }
        else
        {
            Players = players;
        }
    }
}