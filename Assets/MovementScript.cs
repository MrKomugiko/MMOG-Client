using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementScript : MonoBehaviour
{
    [SerializeField] Transform _transform;
    [SerializeField] int CurrentFloor = 2; 
    [SerializeField] Vector3Int currentPosition = new Vector3Int(0,0,2);

    void Start()
    {
        _transform.position = GameManager.instance._tileMap_GROUND.CellToWorld(currentPosition);
        _transform.position += new Vector3(0,0,.9f);
    }

    public bool moving;
     private void Update()
    {
        if(moving) return ;

        if (Input.GetKeyDown(KeyCode.W))
            Move(KeyCode.W);
        
        if (Input.GetKeyDown(KeyCode.S))           
            Move(KeyCode.S);
    
        if (Input.GetKeyDown(KeyCode.A))
            Move(KeyCode.A);
        
        if(Input.GetKeyDown(KeyCode.D))          
            Move(KeyCode.D);
        if(Input.GetKeyDown(KeyCode.Space))          
            StartCoroutine(JumpAnimation());
    }

    private void Move(KeyCode directionKey)
    {
        moving = true;
        print("move");
        float newX = 0;
        float newY = 0;

        switch (directionKey)
        {
            case KeyCode.W:
                newX = -0.5f;
                newY =  0.25f;
               
            break;
               case KeyCode.S:
                newX =  0.5f;
                newY = -0.25f;
            break;
              case KeyCode.A:
                newX = -0.5f;
                newY = -0.25f;
            break;
               case KeyCode.D:
                newX =  0.5f;
                newY =  0.25f;
            break;
        }
        
         StartCoroutine(JumpAnimation(newX,newY));
    }


    public float jumpFrames;
    public IEnumerator JumpAnimation(float xShift = 0, float yShift = 0)
    {

        // skok na wysokosc +1L Vector3 Height_shift_Vector = new Vector3(0,0,1f/jumpFrames);
        // skok na wysokosc +2L Vector3 Height_shift_Vector = new Vector3(0,0,2f/jumpFrames);

        Vector3 UP_shift_Vector = new Vector3(((xShift/2)/jumpFrames),((yShift/2)/jumpFrames)+(0.25f/jumpFrames),0);
        Vector3 Down_shift_Vector = new Vector3(((xShift/2)/jumpFrames),((yShift/2)/jumpFrames)-(0.25f/jumpFrames),0);
        Vector3 Height_shift_Vector = new Vector3(0,0,1f/jumpFrames);
        
        for (int i = 0; i < jumpFrames; i++)
        {
            _transform.position += UP_shift_Vector;
            _transform.position+= Height_shift_Vector;
            yield return new WaitForFixedUpdate();
        }
         for (int i = 0; i < jumpFrames; i++)
         {
                 _transform.position += Down_shift_Vector;
                 _transform.position -= Height_shift_Vector;
                yield return new WaitForFixedUpdate();
        }

        moving = false;
        yield return null;
    }
}
