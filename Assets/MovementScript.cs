using System.IO;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MovementScript : MonoBehaviour
{
    [SerializeField] Transform _transform;
    [SerializeField] int CurrentFloor = 2; 
    [SerializeField] public Vector3Int lastPosition_Grid = Vector3Int.zero;
    [SerializeField] PlayerManager PManager;
    [SerializeField] private float jumpFrames = 3;
    void Start()
    {
        lastPosition_Grid = PManager.CurrentPosition_GRID;
        _transform.position = GameManager.instance._tileMap_GROUND.CellToWorld(PManager.CurrentPosition_GRID);
        _transform.position += new Vector3(0,0,.9f);
        if(PManager.IsLocal) AssignFunctionToLocalPlayerButtons();
    }

    public bool moving;
    [SerializeField] float navigationButtonsDelay = .2f;
    float timer =  0.0f;
    [SerializeField] public bool waitingForServerAnswer = false;
    private void Update()
    {
        if(waitingForServerAnswer) return;
        
        if (Input.GetKeyDown(KeyCode.W))
            NavigationButtonPressed(KeyCode.W);
        else if (Input.GetKeyDown(KeyCode.S))           
            NavigationButtonPressed(KeyCode.S);
        else if (Input.GetKeyDown(KeyCode.A))
            NavigationButtonPressed(KeyCode.A);
        else if(Input.GetKeyDown(KeyCode.D))          
            NavigationButtonPressed(KeyCode.D);
        
    }
    private void AssignFunctionToLocalPlayerButtons()
    {
        GameObject.Find("W_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.W));
        GameObject.Find("S_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.S));
        GameObject.Find("A_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.A));
        GameObject.Find("D_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.D));
    } 
    public void NavigationButtonPressed(KeyCode key)
    {
        if(moving) return ;
        waitingForServerAnswer = true;
        
        bool[] _inputsFromButton = new bool[4];
        switch(key)
        {
            case KeyCode.W: // 0
                _inputsFromButton[0] = true;
                break;
            
            case  KeyCode.S: // 1
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
        if(moving) return; 

        float newX = 0;
        float newY = 0;
        var direction = lastPosition_Grid-newPosition_Grid;
        float newZ = CheckIfPlayerMakeJump(lastPosition_Grid.z, newPosition_Grid.z);
        lastPosition_Grid = newPosition_Grid;


        if(direction.x == 0 && direction.y == -1) // DOWN
        {
                newX = -0.5f;
                newY =  0.25f;
        }
        if(direction.x == 0 && direction.y == 1) // UP
{
            newX =  0.5f;
            newY = -0.25f;
}
        if(direction.x == 1 && direction.y == 0) // right
{
            newX = -0.5f;
            newY = -0.25f;
}
        if(direction.x == -1 && direction.y == 0) // left
        {
                newX =  0.5f;
                newY =  0.25f;
        }
        
        if(newZ == 0) StartCoroutine(WalkAnimation(newX,newY));
        if(newZ != 0) StartCoroutine(JumpAnimation(newPosition_Grid));
    }

    private float CheckIfPlayerMakeJump(int old_Z, int new_Z)
    {
        float heightDifferenceValue =  (float)(new_Z - old_Z);

        return heightDifferenceValue;
    }

    private IEnumerator WalkAnimation(float xShift = 0, float yShift = 0)
    {
        // skok na wysokosc +1L Vector3 Height_shift_Vector = new Vector3(0,0,1f/jumpFrames);
        // skok na wysokosc +2L Vector3 Height_shift_Vector = new Vector3(0,0,2f/jumpFrames);

        Vector3 UP_shift_Vector = new Vector3(((xShift/2)/jumpFrames),((yShift/2)/jumpFrames)+(0.25f/jumpFrames),0);
        Vector3 Down_shift_Vector = new Vector3(((xShift/2)/jumpFrames),((yShift/2)/jumpFrames)-(0.25f/jumpFrames),0);    
        Vector3 Height_shift_Vector = new Vector3(0,0,2f/jumpFrames);
        
        for (int i = 0; i < jumpFrames; i++)
        {
            _transform.position += UP_shift_Vector;
            _transform.position+= Height_shift_Vector;
            yield return new WaitForEndOfFrame();
        }
         for (int i = 0; i < jumpFrames; i++)
         {
                 _transform.position += Down_shift_Vector;
                 _transform.position -= Height_shift_Vector;
                yield return new WaitForEndOfFrame();
        }

        moving = false;
        yield return null;
    }
    private IEnumerator JumpAnimation(Vector3Int newPosition_Grid)
    {
        Vector3 startPosition = _transform.position;
        Vector3 endPosition = GameManager.instance._tileMap.CellToWorld(newPosition_Grid);

        _transform.position = new Vector3(endPosition.x,endPosition.y,(startPosition.z+2));

        // TODO: ogarnąć trajektorie skoku 

         for (int i = 0; i < jumpFrames; i++)
        {
            yield return new WaitForEndOfFrame();
        }
         for (int i = 0; i < jumpFrames; i++)
         {
                yield return new WaitForEndOfFrame();
        }

        moving = false;
        yield return null;
    }

}
