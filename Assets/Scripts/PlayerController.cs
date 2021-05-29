using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    private void Start() 
    {
        if(gameObject.transform.name.Contains("LocalPlayer"))
             AssignFunctionToLocalPlayerButtons();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
            NavigationButtonPressed(KeyCode.W);
        
        if (Input.GetKeyDown(KeyCode.S))           
            NavigationButtonPressed(KeyCode.S);
    
        if (Input.GetKeyDown(KeyCode.A))
            NavigationButtonPressed(KeyCode.A);
        
        if(Input.GetKeyDown(KeyCode.D))          
            NavigationButtonPressed(KeyCode.D);
    }

    // ANDROID keys maping to buttons ( no keyboard xd )
    // WINDOWS manual moving using a buttons
    bool lockMovement = false;
    public void NavigationButtonPressed(KeyCode key)
    {
      
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

    private void AssignFunctionToLocalPlayerButtons()
    {
        GameObject.Find("W_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.W));
        GameObject.Find("S_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.S));
        GameObject.Find("A_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.A));
        GameObject.Find("D_Btn").GetComponent<Button>().onClick.AddListener(()=>NavigationButtonPressed(KeyCode.D));
    }
}
