using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private string _username;
    [SerializeField] private Vector3Int currentPosition_GRID;
    [SerializeField] private bool isLocal;
    [SerializeField] Tile myTile;

    public int Id { get => _id; set => _id = value; }
    public string Username { get => _username; set => _username = value; }
    public Vector3Int CurrentPosition_GRID { get => currentPosition_GRID; set => currentPosition_GRID = value; }
    public bool IsLocal { get => isLocal; set => isLocal = value; }
    public Tile MyTile { get => myTile; set => myTile = value; }

    internal void MoveToPositionInGrid(Vector3Int newPosition)
    {
        // 1# sprawdz czy jakis inny gracz stoi na tm samym miejscu co ty 
        //  ( jeżeli tak, podstaw na swoim miejscu tilesa bohatera, a samego siebie wstaw dalej)
        
        // czyszczenie ogona
        // sprawdzenie czy staliśmy na czimś miejscu
        if(CheckIfMorePlayersStayOnThisPosition(CurrentPosition_GRID))
        {
            GameManager.instance._tileMap.SetTile(currentPosition_GRID,OtherAvaiablePlayerTileAtThisPosition(CurrentPosition_GRID,Id));
        }
        else
        {
            GameManager.instance._tileMap.SetTile(currentPosition_GRID,null);
        }
        // wstawianie aktualnej pozycji
        GameManager.instance._tileMap.SetTile(newPosition,MyTile);
        //zapisywanie nowejaktualnej pozycji
        CurrentPosition_GRID = newPosition;
    }

    public static bool CheckIfMorePlayersStayOnThisPosition(Vector3Int position) => 
        GameManager.players.Values
            .Where(pos=>pos.CurrentPosition_GRID == position)
            .Count() > 1;
    
    public static Tile OtherAvaiablePlayerTileAtThisPosition(Vector3Int position, int ignoredId) => 
        GameManager.players.Values
            .Where(p => p.CurrentPosition_GRID == position && p.Id != ignoredId)
            .FirstOrDefault().myTile;
}
