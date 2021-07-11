using System;
using System.Linq;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private bool isLocal;
    [SerializeField] private int _id;
    [SerializeField] private string _username;
    [SerializeField] private Vector3Int currentPosition_GRID;
    [SerializeField] BorderScript borderScript;
    [SerializeField] private LOCATIONS currentLocation;
    NPCDetector nPCDetector;
    public MovementScript movementScript;
    public DungeonsLobby dungeonRoom;
    public SpriteRenderer SRenderer;

    InventoryScript myInventory;


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
                GameManager.instance.cam.transform.parent = this.gameObject.transform.Find("Player-xray-border").gameObject.transform; ;
                GameManager.instance.cam.transform.localPosition = Vector3.zero;
                nPCDetector = GetComponentInChildren<NPCDetector>();
                myInventory = UIManager.instance.PlayerInventroy;
            }
        }
    }
    public LOCATIONS CurrentLocation
    {
        get => currentLocation;
        set
        {
            var parentLoc = GameManager.instance.ListaDostepnychLokalizacji
                .Where(loc => loc.name == value.ToString())
                .First();

            gameObject.transform.parent = parentLoc.transform;
            
            if (parentLoc.name.Contains("DUNGEON"))
            {
                // miejsce gdzie znajdowac sie beda ukrycki gracze z innych pokoi dungeona
                gameObject.transform.parent = parentLoc.transform.Find("PlayersFromOtherRooms").transform;

                if(GameManager.GetLocalPlayer() != null)
                {
                    if (dungeonRoom.Players.Contains(GameManager.GetLocalPlayer().Username))
                    {
                        // print("przeniesienie gracza:" + Username + " do "+value.ToString());
                        gameObject.transform.parent = parentLoc.transform;
                    }
                    else
                    {
                        // na starcie kazdy lokalny gracz bedzie w glownym katalogu dungeona
                        gameObject.transform.parent = parentLoc.transform;
                    }
                }
            }
                
            currentLocation = value;
        }
    }

    private void Awake() {
        SRenderer = GetComponent<SpriteRenderer>();
        
    }
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
        if(isLocal) GameManager.instance.cam.transform.localPosition = Vector3.zero;
    
        CurrentPosition_GRID = newPosition;
        movementScript.ExecuteMovingAnimation(newPosition_Grid: newPosition);        
    }

    internal void TeleportToPositionInGrid(Vector3Int newPosition)
    {
         CurrentPosition_GRID = newPosition;
         movementScript.Teleport(newPosition);
    }

    private void OnDestroy() 
    {
    //    if(GameManager.players[Client.instance.myId].isLocal)
    //     {
    //         // w przypadku kikcka z serwera obbiekt gracza zostanie usunięty, 
    //         //  dlatego najpierw trzeba odczepić od niego kamere        
    //         print("detach camera from local player object");
    //        //TODO: odczepianie sie kamery : GameManager.instance.cam.transform.SetParent(null);
    //     }
    }

    public void TransformIntoStairs(bool isActive)
    {
        ClientSend.SendServerPlayerActionCommand(ClientSend.PlayerActions.TransformToStairs, isActive);  
    }
}



