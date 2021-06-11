using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MovementScript : MonoBehaviour
{
    public bool moving;
    public bool waitingForServerAnswer = false;
    public int CurrentFloor = 2;
    [SerializeField] Transform _transform;
    [SerializeField] Vector3Int lastPosition_Grid = Vector3Int.zero;
    [SerializeField] PlayerManager PManager;
    [SerializeField] float jumpFrames = 16;
    [SerializeField] float walkFrames = 6;


    public void InstallConponent(PlayerManager playermanager)
    {
        PManager = playermanager;
        Configure();
    }
    void Configure()
    {
        lastPosition_Grid = PManager.CurrentPosition_GRID;
        _transform = GetComponent<Transform>();
        _transform.position = GameManager.instance._tileMap_GROUND.CellToWorld(PManager.CurrentPosition_GRID);
        _transform.position += new Vector3(0, 0, .9f);
        if (PManager.IsLocal) AssignFunctionToLocalPlayerButtons();
    }
    
    private void Update()
    {
        // wyłączenie update'a na innych klientach
        if(PManager.IsLocal == false) return;
        if (waitingForServerAnswer) return;

        if (Input.GetKeyDown(KeyCode.W))
            NavigationButtonPressed(KeyCode.W);
        else if (Input.GetKeyDown(KeyCode.S))
            NavigationButtonPressed(KeyCode.S);
        else if (Input.GetKeyDown(KeyCode.A))
            NavigationButtonPressed(KeyCode.A);
        else if (Input.GetKeyDown(KeyCode.D))
            NavigationButtonPressed(KeyCode.D);
    }
    private void AssignFunctionToLocalPlayerButtons()
    {
        GameObject.Find("W_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.W));
        GameObject.Find("S_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.S));
        GameObject.Find("A_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.A));
        GameObject.Find("D_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.D));
    }
    public void NavigationButtonPressed(KeyCode key)
    {
        if (moving) return;
        waitingForServerAnswer = true;

        bool[] _inputsFromButton = new bool[4];
        switch (key)
        {
            case KeyCode.W: // 0
                _inputsFromButton[0] = true;
                break;

            case KeyCode.S: // 1
                _inputsFromButton[1] = true;
                break;

            case KeyCode.A: // 2
                _inputsFromButton[2] = true;
                break;

            case KeyCode.D: // 3
                _inputsFromButton[3] = true;
                break;
        }
        ClientSend.PlayerMovement(_inputsFromButton);
    }
    public void ExecuteMovingAnimation(Vector3Int newPosition_Grid)
    {
        if (moving) return;

        float newX = 0;
        float newY = 0;
        var direction = lastPosition_Grid - newPosition_Grid;
        int jumpDirection = CheckIfPlayerMakeJump(lastPosition_Grid.z, newPosition_Grid.z);

        if (direction.x == 0 && direction.y == -1) // DOWN
        {
            newX = -0.5f;
            newY = 0.25f;
        }
        if (direction.x == 0 && direction.y == 1) // UP
        {
            newX = 0.5f;
            newY = -0.25f;
        }
        if (direction.x == 1 && direction.y == 0) // right
        {
            newX = -0.5f;
            newY = -0.25f;
        }
        if (direction.x == -1 && direction.y == 0) // left
        {
            newX = 0.5f;
            newY = 0.25f;
        }

        if (jumpDirection == 0) StartCoroutine(WalkAnimation(newX, newY));
        if (jumpDirection != 0) StartCoroutine(JumpAnimation(newPosition_Grid, jumpDirection, (Vector3Int?)lastPosition_Grid));
        lastPosition_Grid = newPosition_Grid;
    }
    private int CheckIfPlayerMakeJump(int old_Z, int new_Z)
    {
        int heightDifferenceValue = (new_Z - old_Z);
        // print($"Gracz idzie {(heightDifferenceValue>0?"do góry":"na dół")}.");
        return heightDifferenceValue;
    }
    private IEnumerator WalkAnimation(float xShift = 0, float yShift = 0)
    {
        Vector3 UP_shift_Vector = new Vector3(((xShift / 2) / walkFrames), ((yShift / 2) / walkFrames) + (0.25f / walkFrames), 0);
        Vector3 Down_shift_Vector = new Vector3(((xShift / 2) / walkFrames), ((yShift / 2) / walkFrames) - (0.25f / walkFrames), 0);
        Vector3 Height_shift_Vector = new Vector3(0, 0, 2f / walkFrames);

        for (int i = 0; i < walkFrames; i++)
        {
            _transform.position += UP_shift_Vector;
            _transform.position += Height_shift_Vector;
            yield return new WaitForEndOfFrame();
        }
        for (int i = 0; i < walkFrames; i++)
        {
            _transform.position += Down_shift_Vector;
            _transform.position -= Height_shift_Vector;
            yield return new WaitForEndOfFrame();
        }

        moving = false;
        yield return null;
    }
    private IEnumerator JumpAnimation(Vector3Int newPosition_Grid, int direction, Vector3Int? startPodition_Grid = null)
    {
        Vector3 startPosition_World = GameManager.instance._tileMap.CellToWorld(startPodition_Grid.Value);
        Vector3 highestJumpPosition_World = GetMediumHightPoint(newPosition_Grid, direction, startPodition_Grid);
        Vector3 endPosition_World = GameManager.instance._tileMap.CellToWorld(newPosition_Grid);

        int z = 0;
        Vector3 _startPoint = new Vector3(startPosition_World.x, startPosition_World.y, transform.position.z);
        z = direction > 0 ? (startPodition_Grid.Value.z + 3) : (startPodition_Grid.Value.z + 3);
        Vector3 _highMiddlePoint = new Vector3(highestJumpPosition_World.x, highestJumpPosition_World.y, z + 1);
        Vector3 _finalPoint = new Vector3(endPosition_World.x, endPosition_World.y, z);

        float frames = direction > 0 ? jumpFrames: jumpFrames/2;
        for (float i = 0; i < 1.1; i += (1f / frames))
        {
            if(i<0.5)
                _transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1,1.1f,1),i*2);
            else 
                _transform.localScale = Vector3.Lerp(new Vector3(1,1.1f,1),Vector3.one,(i+1)/2);

            _transform.position = Vector3.Lerp(_startPoint, _highMiddlePoint, i);
            yield return new WaitForEndOfFrame();
        }
        frames = direction > 0 ? jumpFrames/2: jumpFrames;
      
        for (float i = 0; i < 1.1; i += (1f / (frames)))
        {
            // wgniatanie
            if(i>0.5)_transform.localScale = Vector3.Lerp(Vector3.one, direction>0?new Vector3(1,.9f,1):new Vector3(1,.8f,1),(i+1)/2);
            
            _transform.position = Vector3.Lerp(_highMiddlePoint, _finalPoint, i);
            yield return new WaitForEndOfFrame();
        }
        for (float i = 0; i < 1.1; i += (1f / (frames/2)))
        {
            // dogniatanie
            if(i<0.5) _transform.localScale = Vector3.Lerp(direction>0?new Vector3(1,.9f,1):new Vector3(1,.8f,1),direction>0?new Vector3(1,.8f,1):new Vector3(1,.6f,1),i*2);

        // wyprostowywanie sie
            _transform.localScale = Vector3.Lerp(direction>0?new Vector3(1,.8f,1):new Vector3(1,.6f,1),Vector3.one,i);
            yield return new WaitForEndOfFrame();
        }
        
        CurrentFloor += direction > 0 ? 2 : -2;
        moving = false;
        yield return null;

    }
    static Vector3 GetMediumHightPoint(Vector3Int newPosition_Grid, int direction, Vector3Int? startPodition_Grid)
    {
        Vector3 h_pos_1 = GameManager.instance._tileMap.CellToWorld(startPodition_Grid.Value + new Vector3Int(0, 0, direction > 0 ? 2 : 2));
        Vector3 h_pos_2 = GameManager.instance._tileMap.CellToWorld(newPosition_Grid + new Vector3Int(0, 0, direction > 0 ? 2 : 2));

        Vector3 highestJumpPosition_World = (h_pos_1 + h_pos_2) / 2;
        return highestJumpPosition_World;
    }

}
