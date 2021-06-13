using UnityEngine;

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
    [SerializeField] public SpriteRenderer SRenderer;


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

    public LOCATIONS CurrentLocation { 
        get => currentLocation;
        set {
            print("przeniesienie gracza:" + Username + " do "+value.ToString());
            switch(value) {
                case LOCATIONS.Start_First_Floor:
                    gameObject.transform.parent = GameManager.instance.StartLocationFirstFloorContainer.transform;
                    break;
                case LOCATIONS.Start_Second_Floor:
                    gameObject.transform.parent = GameManager.instance.StartLocationSeconFloorContainer.transform;
                    break;
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
       // print("zmiana pozycji na nową");
        //zapisywanie nowejaktualnej pozycji
        CurrentPosition_GRID = newPosition;
        // wykonanie animacji ruchu z wczesniejszej pozycji na aktualną
        movementScript.ExecuteMovingAnimation(newPosition_Grid: newPosition);
        
        
    }

}
