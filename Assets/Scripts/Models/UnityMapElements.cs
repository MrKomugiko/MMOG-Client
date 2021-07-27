using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
    [Serializable] public class UnityMapElements
    {
        public LOCATIONS MapName;
        public Tilemap Obstacle_Tilemap;
        public Tilemap Ground_Tilemap;
        public GameObject Container;

        public Dictionary<Vector3Int, string> Obstacle_MAPDATA;
        public Dictionary<Vector3Int, string> Ground_MAPDATA;

        
        public UnityMapElements(LOCATIONS mapName, Transform parent, GameObject TilemapPrefab)
        {
            Debug.Log("Creating tilemaps object for "+mapName.ToString());
            
            MapName = mapName;
            Container = UnityEngine.Object.Instantiate(TilemapPrefab, parent);
            Container.name = MapName.ToString();

            foreach(var tilemap in Container.GetComponentsInChildren<Tilemap>())
            {
                if(Obstacle_Tilemap == null)
                {
                    Obstacle_Tilemap = tilemap;
                    Obstacle_Tilemap.name = mapName+"_Obstalce";
                    continue;
                }

                if(Ground_Tilemap == null)
                {
                    Ground_Tilemap = tilemap;
                    Ground_Tilemap.name = mapName+"_Ground";
                    continue;
                }
            }

            Obstacle_MAPDATA = new Dictionary<Vector3Int, string>();
            Ground_MAPDATA = new Dictionary<Vector3Int, string>();
        }

        public ref Tilemap GetTilemapRef(MAPTYPE maptype)
        {
            switch(maptype)
            {
                case MAPTYPE.Ground_MAP:
                return ref this.Ground_Tilemap;

                case MAPTYPE.Obstacle_MAP:
                return ref this.Obstacle_Tilemap;
            }

            throw new Exception("wrong tilemap type, cannot return tilemap");
        }

         public ref Dictionary<Vector3Int, string> GetMapdataRef(MAPTYPE maptype)
        {
            switch(maptype)
            {
                case MAPTYPE.Ground_MAP:
                Debug.Log("size " +this.Ground_MAPDATA.Count);
                return ref this.Ground_MAPDATA;

                case MAPTYPE.Obstacle_MAP:
                Debug.Log("size " +this.Obstacle_MAPDATA.Count);
                return ref this.Obstacle_MAPDATA;
            }
            
            throw new Exception("wrong tilemap type cannot return  mapdata");
        }

    }
