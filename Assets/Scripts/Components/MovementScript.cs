using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MovementScript : MonoBehaviour
{
    public bool movingAnimationInProgress;
    public bool waitingForServerAnswer = false;
    [SerializeField] private int currentFloor = 2;
    [SerializeField] Transform _transform;
    [SerializeField] Vector3Int lastPosition_Grid = Vector3Int.zero;
    [SerializeField] PlayerManager PManager;
    [SerializeField] float jumpFrames = 16;
    [SerializeField] float walkFrames = 6;
    [SerializeField] FixedJoystick Joystick;
    public int CurrentFloor 
    { 
        get => currentFloor; 
        set {
           //TODO: z ręki zakodowane 3 piętra, ale mozna zrobic to automatycznie
            if(value>=0 && value<14)
            {
                PManager.SRenderer.sortingOrder = 0;
            }
             if(value>=14 && value<28)
            {
                PManager.SRenderer.sortingOrder = 2;
            }
              if(value>=28 && value<42)
            {
                PManager.SRenderer.sortingOrder = 4;
            }
            currentFloor = value;
        }
    }
    public void InstallConponent(PlayerManager playermanager)
    {
        PManager = playermanager;
        Configure();
    }
    Button X_Btn, Y_Btn, B_Btn, A_Btn;
    ColorBlock originalColorBlock = ColorBlock.defaultColorBlock;
    ColorBlock pressedColorBlock = ColorBlock.defaultColorBlock;
    void Configure()
    {
        lastPosition_Grid = PManager.CurrentPosition_GRID;
        _transform = GetComponent<Transform>();
        _transform.position = GameManager.instance._tileMap_GROUND.CellToWorld(PManager.CurrentPosition_GRID);
        _transform.position += new Vector3(0, 0, .9f);
        if (PManager.IsLocal) {
          //  AssignFunctionToLocalPlayerButtons();
            // dodanie obslugi joisticka
            Joystick = UIManager.instance.joystick;

        X_Btn = GameObject.Find("X_Btn").GetComponent<Button>();
        Y_Btn = GameObject.Find("Y_Btn").GetComponent<Button>();
        B_Btn = GameObject.Find("B_Btn").GetComponent<Button>();
        A_Btn = GameObject.Find("A_Btn").GetComponent<Button>();
        }

        

    }
    private void Update()
    {        
        // wyłączenie update'a na innych klientach
        if(PManager.IsLocal == false) return;

        HandleGamepadButtons();

        if (waitingForServerAnswer == true) return;
        if(movingAnimationInProgress) return;
        if(ExecutingActionWhichBlockMovement) return;

        NavigationButtonPressed(GetDirectionFromKeyboardKeyPressed());
        NavigationButtonPressed(GetDirectionFromJoystickInput()); 
        NavigationButtonPressed(GetDirectionFromGamepadJoystickInput());       
    }
    private KeyCode GetDirectionFromJoystickInput()
    {    // W 

        if(Joystick.Direction.x == 0 && Joystick.Direction.y == 0) return KeyCode.None;

        if(Joystick.Direction.x >= -1 && Joystick.Direction.x <= 0 && Joystick.Direction.y >= 0 && Joystick.Direction.y <= 1)
        {
            return KeyCode.W;
        }
        // S
          if(Joystick.Direction.x >=0 && Joystick.Direction.x <= 1 && Joystick.Direction.y >= -1 && Joystick.Direction.y <= 0)
        {
            return KeyCode.S;
        }
        // A
          if(Joystick.Direction.x >= -1 && Joystick.Direction.x <= 0 && Joystick.Direction.y >= -1 && Joystick.Direction.y <= 0)
        {
            return KeyCode.A;
        }
        // D
          if(Joystick.Direction.x >= 0 && Joystick.Direction.x <= 1 && Joystick.Direction.y >= 0 && Joystick.Direction.y <= 1)
        {
            return KeyCode.D;
        }
         
         return KeyCode.None;
    }
    private KeyCode GetDirectionFromGamepadJoystickInput()
    {    
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical"); 
        if(x==0 && y ==0 ) return KeyCode.None;
        // W
        if( x >= -1.0f &&  x <= 0 && y >= 0 && y <= 1.0f) return KeyCode.W;
        // S
        if( x >= 0 &&  x <= 1.0f && y >= -1.0f && y <= 0) return KeyCode.S;
        // A
        if( x >= -1.0f &&  x <= 0 && y >= -1.0f && y <= 0) return KeyCode.A;
        // D
        if( x >= 0 &&  x <= 1.0f && y >= 0 && y <= 1.0f) return KeyCode.D;
       
        return KeyCode.None;
    }
    private KeyCode GetDirectionFromKeyboardKeyPressed()
    {
        if (Input.GetKeyDown(KeyCode.W)) return KeyCode.W; 
        else if (Input.GetKeyDown(KeyCode.S)) return KeyCode.S;
        else if (Input.GetKeyDown(KeyCode.A)) return KeyCode.A;   
        else if (Input.GetKeyDown(KeyCode.D)) return KeyCode.D;   
        else return KeyCode.None;
    }
    private bool ExecutingActionWhichBlockMovement = false;
    private void  HandleGamepadButtons()
    {
   // X = transformacja w schodek xD
        if(Input.GetKeyDown("joystick button "+2))
        {
            ExecutingActionWhichBlockMovement = true;
            pressedColorBlock.normalColor = Color.blue;
            PManager.TransformIntoStairs(true);
            X_Btn.colors = pressedColorBlock;
        }
        else if(Input.GetKeyUp("joystick button "+2))
        {
            ExecutingActionWhichBlockMovement = false;
            X_Btn.colors = originalColorBlock;
            PManager.TransformIntoStairs(false);
            print("koeniec wciskania X");
        }
        
        if(Input.GetKeyDown("joystick button "+3))
        {
            pressedColorBlock.normalColor = Color.yellow;
            print("y jest wcisniete");
            Y_Btn.colors = pressedColorBlock;
        }
        else if(Input.GetKeyUp("joystick button "+3))
        {
            Y_Btn.colors = originalColorBlock;
            print("koeniec wciskania y");
        }

        if(Input.GetKeyDown("joystick button "+1))
        {
            pressedColorBlock.normalColor = Color.red;
            print("b jest wcisniete");
            B_Btn.colors = pressedColorBlock;
        }
        else if(Input.GetKeyUp("joystick button "+1))
        {
            B_Btn.colors = originalColorBlock;
            print("koeniec wciskania b");
        }

        if(Input.GetKeyDown("joystick button "+0))
        {
                pressedColorBlock.normalColor = Color.green;
            print("a jest wcisniete");
            A_Btn.colors = pressedColorBlock;
        }
        else if(Input.GetKeyUp("joystick button "+0))
        {
            A_Btn.colors = originalColorBlock;
            print("koeniec wciskania a");
        }
    }
    // private void AssignFunctionToLocalPlayerButtons()
    // {
    //     GameObject.Find("W_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.W));
    //     GameObject.Find("S_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.S));
    //     GameObject.Find("A_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.A));
    //     GameObject.Find("D_Btn").GetComponent<Button>().onClick.AddListener(() => NavigationButtonPressed(KeyCode.D));

    //     X_Btn.onClick.AddListener(() => NavigationButtonPressed(KeyCode.X));
    //     Y_Btn.onClick.AddListener(() => NavigationButtonPressed(KeyCode.Y));
    //     B_Btn.onClick.AddListener(() => NavigationButtonPressed(KeyCode.B));
    //     A_Btn.onClick.AddListener(() => NavigationButtonPressed(KeyCode.A));
    // }
    public void NavigationButtonPressed(KeyCode key)
    {
        if(key == KeyCode.None) return;
        if(PManager.IsLocal == false) return;
        if (waitingForServerAnswer == true) return;
        if(movingAnimationInProgress) return;

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
    public void Teleport(Vector3Int vector3Int)
    {
        // TODO: Teleport na inne piętra niemożliwy, do zrobienia przeliczanie Z względem wysokosci
        Vector3 worldPosition = GameManager.instance._tileMap.CellToWorld(Vector3Int.CeilToInt(vector3Int));
        lastPosition_Grid = vector3Int;
        _transform.position =  new Vector3(worldPosition.x,worldPosition.y,_transform.position.z );

    }
    public void ExecuteMovingAnimation(Vector3Int newPosition_Grid)
    {
        if(Client.instance.myId == PManager.Id )
        {
            movingAnimationInProgress = true;
        }
        
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

        if(this.gameObject.activeInHierarchy== true)
        {
            if (jumpDirection == 0) StartCoroutine(WalkAnimation(newX, newY));
            if (jumpDirection != 0) StartCoroutine(JumpAnimation(newPosition_Grid, jumpDirection, (Vector3Int?)lastPosition_Grid));
        }
        else
        {
//           print("inactive object : "+jumpDirection);
            if (jumpDirection == 0){ // walk
                _transform.position += new Vector3(newX, newY,0);
            }
            if (jumpDirection != 0){ // jump

                _transform.position =  GameManager.instance._tileMap.CellToWorld(newPosition_Grid);
                _transform.position += new Vector3(0,0,(jumpDirection>0?2:4));
              //  CurrentFloor += jumpDirection > 0 ? 2 : -2;
            }
        }
        

        lastPosition_Grid = newPosition_Grid;
     
    }
    private int CheckIfPlayerMakeJump(int old_Z, int new_Z)
    {
        int heightDifferenceValue = (new_Z - old_Z);
   
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
            yield return new WaitForFixedUpdate();
        }
        for (int i = 0; i < walkFrames; i++)
        {
            _transform.position += Down_shift_Vector;
            _transform.position -= Height_shift_Vector;
            yield return new WaitForFixedUpdate();
        }

        movingAnimationInProgress = false;

        GameManager.players[PManager.Id].movementScript.waitingForServerAnswer = false;
        yield return null;
    }
    private IEnumerator JumpAnimation(Vector3Int newPosition_Grid, int direction, Vector3Int? startPodition_Grid = null)
    {
        Vector3 startPosition_World = GameManager.instance._tileMap.CellToWorld(startPodition_Grid.Value);
        Vector3 highestJumpPosition_World = GetMediumHightPoint(newPosition_Grid, direction, startPodition_Grid);
        Vector3 endPosition_World = GameManager.instance._tileMap.CellToWorld(newPosition_Grid);

        float z = 0;
        Vector3 _startPoint = new Vector3(startPosition_World.x, startPosition_World.y, transform.position.z+(direction>0?2:4));
        z = direction > 0 ? (_transform.position.z + 2) : (_transform.position.z - 2);
        Vector3 _highMiddlePoint = new Vector3(highestJumpPosition_World.x, highestJumpPosition_World.y, z+(direction>0?2:4));
        Vector3 _finalPoint = new Vector3(endPosition_World.x, endPosition_World.y, z+(direction>0?2:4));

        float frames = direction > 0 ? jumpFrames: jumpFrames/2;
        for (float i = 0; i < 1; i += (1f / frames))
        {
            if(i<0.5)
                _transform.localScale = Vector3.Lerp(Vector3.one, new Vector3(1,1.1f,1),i*2);
            else 
                _transform.localScale = Vector3.Lerp(new Vector3(1,1.1f,1),Vector3.one,(i+1)/2);

            _transform.position = Vector3.Lerp(_startPoint, _highMiddlePoint, i);
            
            yield return new WaitForFixedUpdate();
        }
        frames = direction > 0 ? jumpFrames/2: jumpFrames;
      
        for (float i = 0; i < 1; i += (1f / (frames)))
        {
            // wgniatanie
            if(i>0.5)_transform.localScale = Vector3.Lerp(Vector3.one, direction>0?new Vector3(1,.9f,1):new Vector3(1,.8f,1),(i+1)/2);
            
            _transform.position = Vector3.Lerp(_highMiddlePoint, _finalPoint, i);
            yield return new WaitForFixedUpdate();
        }
        _transform.position = new Vector3(_finalPoint.x,_finalPoint.y,z);

        for (float i = 0; i < 1; i += (1f / (frames/2)))
        {
            // dogniatanie
            if(i<0.5) _transform.localScale = Vector3.Lerp(direction>0?new Vector3(1,.9f,1):new Vector3(1,.8f,1),direction>0?new Vector3(1,.8f,1):new Vector3(1,.6f,1),i*2);

        // wyprostowywanie sie
            _transform.localScale = Vector3.Lerp(direction>0?new Vector3(1,.8f,1):new Vector3(1,.6f,1),Vector3.one,i);
            yield return new WaitForFixedUpdate();
        }
         _transform.localScale = Vector3.one;
        CurrentFloor += direction > 0 ? 2 : -2;
        movingAnimationInProgress = false;
        GameManager.players[PManager.Id].movementScript.waitingForServerAnswer = false;
        yield return null;

    }
  //--------------------------------------------------------------------------------------
   static Vector3 GetMediumHightPoint(Vector3Int newPosition_Grid, int direction, Vector3Int? startPodition_Grid)
    {
        Vector3 h_pos_1 = GameManager.instance._tileMap.CellToWorld(startPodition_Grid.Value + new Vector3Int(0, 0, direction > 0 ? 2 : 2));
        Vector3 h_pos_2 = GameManager.instance._tileMap.CellToWorld(newPosition_Grid + new Vector3Int(0, 0, direction > 0 ? 2 : 2));

        Vector3 highestJumpPosition_World = (h_pos_1 + h_pos_2) / 2;
        return highestJumpPosition_World;
    }
}
