using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BorderScript : MonoBehaviour
{
    Transform _transform;    
    PlayerManager player;
    Vector3Int borderPosition;

    public Vector3Int BorderPosition 
    {
        get => borderPosition; 
        set 
        {
       
            borderPosition = value; 
        }
    }

    private void Awake() {
        SRenderer= GetComponent<SpriteRenderer>();
        _transform = GetComponent<Transform>();    
        player = GetComponentInParent<PlayerManager>();
    }
    private void Start() {
        ShowBorder();
    }
    void Update()
    {
        if(PositionChangeChangeCheck() == false) return;

        _transform.position = GameManager.instance._tileMap.CellToWorld(player.CurrentPosition_GRID);
        BorderPosition = player.CurrentPosition_GRID;
    }

    bool PositionChangeChangeCheck(){
        if(BorderPosition == player.CurrentPosition_GRID) return false;
        return true;
     }

SpriteRenderer SRenderer;
    public void ShowBorder()
    {
        SRenderer.enabled = true;
    }
    public void HideBorder()
    {
        SRenderer.enabled = false;
    }
    
}
