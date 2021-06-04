using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCWindowScript : MonoBehaviour
{
    public void OnClick_Close()
    {
        this.transform.gameObject.SetActive(false);
    }

    private void OnEnable() {
        // chowanie podswietlenia npc i napisu
           
    }


}
