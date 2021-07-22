using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class roomScript : MonoBehaviour
{
    [SerializeField] public DungeonsLobby room;
    [SerializeField] public Button backButton;
    [SerializeField] public Button cancelButton;
    [SerializeField] public Button ernterButton;

    private void OnEnable() {
       
    }
    public void OnClick_Back()
    {

    }

    public void OnClick_Cancel()
    {

    }

    public void AssignRoomDataToWindow(DungeonsLobby _room)
    {
        room = null;
        room = _room;
        // update players data - assign them dungeonlobby
        foreach(var player in room.Players)
        {
            int PlayerID = GameManager.players.Where(p=>p.Value.Username == player).First().Key;
            GameManager.players[PlayerID].dungeonRoom = _room;
        }

         if(room != null)
            {
                if(room.DungeonLocation == DungeonManager.CurrentScrollingDungeonCategory)  
                    this.gameObject.SetActive(true);
                else
                    this.gameObject.SetActive(false);
            }
    }

}