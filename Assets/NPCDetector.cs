﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class NPCDetector : MonoBehaviour
{
    static BoundsInt searchArea = new BoundsInt(-2,-2,0,5,5,1);
/*

    *   *    *    *   *
    *   *    *    *   *
    *   *   [P]   *   *
    *   *    *    *   *
    *   *    *    *   *

*/
    private static Dictionary<Vector3Int,GameObject> _npcInRange_borders = new Dictionary<Vector3Int, GameObject>();
    private static Vector3Int ostatnio_sprawdzana_pozycja = Vector3Int.zero;
    public static void CheckForNPC(Vector3Int playerPosition) 
    {
        if(ostatnio_sprawdzana_pozycja == playerPosition) return; // gracz nie ruszył sie ponowne sprawdzanie nie ejst koneiczne
        ostatnio_sprawdzana_pozycja = playerPosition;
        
        foreach(var position in searchArea.allPositionsWithin)
        {      
            Vector3Int sprawdzanaPozycja = playerPosition+position;
            Tile sprawdzanyTile = (Tile)GameManager.instance._tileMap.GetTile(sprawdzanaPozycja); 
            if(sprawdzanyTile != null)
            {
                if(sprawdzanyTile.name.Contains("NPC")){
                    Debug.Log($"w poblizu jest NPC => [{sprawdzanaPozycja}]");
                    // TODO: Wyślij do serwera zapytanie jaki to npc

                    // TODO: wyrównienie 'NPC' jako coś w zasięgu
                    // "TEST" będzie wartością zwróconą przez serwer
                    if(! _npcInRange_borders.ContainsKey(sprawdzanaPozycja))   // nie musimy dugi raz dodawac tego samego NPCta                 
                        {
                            var border = Instantiate(
                                GameManager.instance.NPC_Glowing_SPRITE_PREFAB,
                                GameManager.instance._tileMap.CellToWorld(sprawdzanaPozycja),
                                Quaternion.identity,
                                GameObject.Find("MapCanvas").transform);
                            _npcInRange_borders.Add(sprawdzanaPozycja,border);
                            border.name = "TEST";
                            border.GetComponentInChildren<Button>().onClick.AddListener(()=>OnClick_test());
                        }
                }
            }
        }
        // sprawdzmy czy ostatnio widziany npc nadal jest w zasięgu             
        foreach(var NPC in _npcInRange_borders.Keys)
        {
            if(searchArea.Contains(NPC - playerPosition) == false)
            {
                Destroy(_npcInRange_borders[NPC].gameObject);
                _npcInRange_borders.Remove(NPC);
            }
        }
    }
    
    public static void OnClick_test()
    {
       GameManager.instance.shopWindow.SetActive(true);
    }

}
