using System.Runtime.InteropServices.ComTypes;
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
    [SerializeField] NPCDetector nPCDetector;
    [SerializeField] public MovementScript movementScript;
    public int Id 
    { 
        get => _id; 
        set => _id = value; 
    }
    public string Username 
    { 
        get => _username; 
        set => _username = value; 
    }
    public Vector3Int CurrentPosition_GRID 
    { 
        get => currentPosition_GRID; 
        set 
        {
            currentPosition_GRID = value;

            if(IsLocal)
            {
                nPCDetector.CheckForNPC(value);
            }
     }
      }
    public bool IsLocal 
    { 
        get => isLocal; 
        set 
        {
            isLocal = value; 
            if(isLocal == true)  
            {
                GameObject.Find("Main Camera").gameObject.transform.parent = this.gameObject.transform.Find("Player-xray-border").gameObject.transform;;    
                nPCDetector = GetComponentInChildren<NPCDetector>();
            }
        } 
    }   
    private void Start() 
     {
         ClearDuplicatedLocalPlayersOnStart();
     }
    private void ClearDuplicatedLocalPlayersOnStart()
    {
        foreach(var pm in GetComponentsInParent<PlayerManager>())
        {
            if(pm == this) continue;

            Destroy(pm.gameObject);
        }
    }
    public void MoveToPositionInGrid(Vector3Int newPosition)
    {
        //zapisywanie nowejaktualnej pozycji
        CurrentPosition_GRID = newPosition;
        // wykonanie animacji ruchu z wczesniejszej pozycji na aktualną
        movementScript.ExecuteMovingAnimation(newPosition_Grid: newPosition);
        movementScript.moving = true;
    }
}
