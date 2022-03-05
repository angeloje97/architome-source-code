using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class WorldInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public SpawnerInfo lastPlayerSpawnBeacon;


        //Static variables
        public static WorldInfo active;

        public void Start()
        {
            active = this;
        }



    }

}
