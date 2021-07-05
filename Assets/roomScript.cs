using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class roomScript : MonoBehaviour
{
    [SerializeField] public DungeonsLobby room;
    [SerializeField] public Button backButton;
    [SerializeField] public Button cancelButton;
    [SerializeField] public Button ernterButton;


    // Start is called before the first frame update
    void Start()
    {
        room = null;
    }

    private void OnEnable() 
    { 

    }

   public void OnClick_Back()
   {

   }

   public void OnClick_Cancel()
   {

   }

}
