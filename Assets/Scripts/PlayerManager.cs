﻿using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private string _username;
    [SerializeField] private Vector3Int currentPosition_GRID;
    [SerializeField] private bool isLocal;
    [SerializeField] NPCDetector nPCDetector;
    [SerializeField] BorderScript borderScript;
    [SerializeField] public MovementScript movementScript;
    [SerializeField] private LOCATIONS currentLocation;


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

            if (IsLocal)
            {
                nPCDetector.CheckForNPC(value);
                borderScript.ChangeBorderSortingOrder(movementScript.CurrentFloor);
            }
        }
    }
    public bool IsLocal
    {
        get => isLocal;
        set
        {
            isLocal = value;
            if (isLocal == true)
            {
                GameObject.Find("Main Camera").gameObject.transform.parent = this.gameObject.transform.Find("Player-xray-border").gameObject.transform; ;
                nPCDetector = GetComponentInChildren<NPCDetector>();
            }
        }
    }

    public LOCATIONS CurrentLocation { get => currentLocation; set => currentLocation = value; }

    private void Start()
    {
        ClearDuplicatedLocalPlayersOnStart();
        movementScript.InstallConponent(this);
    }
    private void ClearDuplicatedLocalPlayersOnStart()
    {
        foreach (var pm in GetComponentsInParent<PlayerManager>())
        {
            if (pm == this) continue;

            Destroy(pm.gameObject);
        }
    }
    public void MoveToPositionInGrid(Vector3Int newPosition)
    {
        //zapisywanie nowejaktualnej pozycji
        CurrentPosition_GRID = newPosition;
        // wykonanie animacji ruchu z wczesniejszej pozycji na aktualną
        movementScript.ExecuteMovingAnimation(newPosition_Grid: newPosition);
        
        if(isLocal) movementScript.moving = true;
    }

}
