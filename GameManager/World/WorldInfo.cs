using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class WorldInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public static WorldInfo active;

        

        public SpawnerInfo currentSpawnBeacon { get; private set; }

        public Action<SpawnerInfo> OnNewSpawnBeacon;
        public void Start()
        {
            active = this;
        }

        public void SetSpawnBeacon(SpawnerInfo spawner)
        {
            currentSpawnBeacon = spawner;
            OnNewSpawnBeacon?.Invoke(spawner);
        }

    }

}
