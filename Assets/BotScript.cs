using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotScript : MonoBehaviour
{
    [SerializeField] MovementScript _movementScript;
    [SerializeField] bool isRunning = false;
    
    IEnumerator ClickDirectionButtonsRoutine()
    {
      //  print("starting new routine");
        while(isRunning)
        {
            if(_movementScript.waitingForServerAnswer) continue;

            _movementScript.NavigationButtonPressed(key: GetDirectionKey(Random.Range(1,5)));

            yield return new WaitForSeconds(0.253f);
        }
    }
    private KeyCode GetDirectionKey(int value)
    {
       // print("bot click button");
        switch (value)
        {
            case 1: return KeyCode.W;
            case 2: return KeyCode.S;
            case 3: return KeyCode.A;
            case 4: return KeyCode.D;

            default: return KeyCode.None;
        }
    }
    

    public void OnClick_TurnOnOfBot()
    {
        if(_movementScript == null )
        {
            _movementScript = GameObject.Find("Player v2 _ LocalPlayer(Clone)").GetComponent<MovementScript>();
            print("Movement script assigned");
        }

        isRunning = !isRunning;
        if(isRunning) StartCoroutine(ClickDirectionButtonsRoutine());
    } 

}
