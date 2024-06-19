using System.Collections;
using System.Collections.Generic;
using UnityEngine;



namespace Architome
{
    public class MapInfo : MonoBehaviour
    {
        public static MapInfo active;
        // Start is called before the first frame update
        public PathfinderOpt pathFinderOpt;
        public SeedGenerator seedGenerator;

        [Header("Map Settings")]
        public bool generateRooms;
        public bool generateEntities;

        [Header("Map Properties")]
        public List<RoomInfo> rooms;
        //public List<GameObject> entities;
        public List<Transform> patrolPoints;
        public Transform trash;
        public bool generationComplete;

        public void GetDependencies()
        {
            if (trash == null)
            {
                trash = new GameObject().transform;
                trash.SetParent(transform);
                trash.name = "Trash";
            }

            active = this;

            var entityGenerator = EntityGenerator();

            if (entityGenerator)
            {
                entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => { generationComplete = true; };
            }
        }

        void Awake()
        {
            GetDependencies();
        }
        // Update is called once per frame
        void Update()
        {

        }
        public MapRoomGenerator RoomGenerator()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MapRoomGenerator>())
                {
                    return child.GetComponent<MapRoomGenerator>();
                }
            }

            return null;
        }

        public MapEntityGenerator EntityGenerator()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<MapEntityGenerator>())
                {
                    return child.GetComponent<MapEntityGenerator>();
                }
            }

            return null;
        }



    }

}
