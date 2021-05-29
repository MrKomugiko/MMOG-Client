using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{

    bool right => Input.GetAxis("Horizontal")>0f;
    bool left => Input.GetAxis("Horizontal")<0f;
    bool up => Input.GetAxis("Vertical")>0f;
    bool down => Input.GetAxis("Vertical")<0f;

    private void Start() 
    {
        if(gameObject.transform.name.Contains("LocalPlayer"))
             AssignFunctionToLocalPlayerButtons();
    }
    private void FixedUpdate()
    {
        
        SendInputToServer();
    }

    private void SendInputToServer()
    {
        bool[] _inputs = new bool[4];
        if (Input.GetKeyDown("w") || up)    _inputs[0] = true;
        
        if (Input.GetKeyDown("s") || down)  _inputs[1] = true;
        
        if (Input.GetKeyDown("a") || left)  _inputs[2] = true;
        
        if (Input.GetKeyDown("d") || right) _inputs[3] = true;
        
        if(_inputs[0] || _inputs[1] || _inputs[2] || _inputs[3]) {
            ClientSend.PlayerMovement(_inputs);
       }
    }

    // ANDROID keys maping to buttons ( no keyboard xd )
    // WINDOWS manual moving using a buttons
    public void NavigationButtonPressed(string key)
    {
        bool[] _inputsFromButton = new bool[4];
        switch(key)
        {
            case "W": // 0
                _inputsFromButton[0] = true;
                break;
            
            case "S": // 1
                _inputsFromButton[1] = true;
                break;
            
            case "A": // 2
                _inputsFromButton[2] = true;
                break;
            
            case "D": // 3
                _inputsFromButton[3] = true;
                break;
        }

        ClientSend.PlayerMovement(_inputsFromButton);
    }

    private void AssignFunctionToLocalPlayerButtons()
    {
        GameObject.Find("W_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed("W"));
        GameObject.Find("S_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed("S"));
        GameObject.Find("A_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed("A"));
        GameObject.Find("D_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed("D"));
    }
}
