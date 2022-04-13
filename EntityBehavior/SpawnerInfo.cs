using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class SpawnerInfo : EntityInfo
    {
        // Start is called before the first frame update
        [Header("Spawner Properties")]
        public Transform spawnPoint;
        public float spawnRadius;
        public float spawnHeight;
        public bool isActive;

        public struct SpawnerEvents
        {
            public Action<EntityInfo> OnSpawnEntity;
            public Action<EntityInfo> OnReviveEntity;
            public Action<int, int> OnStartRevivingParty;
            public Action<SpawnerInfo> OnSetPlayerSpawnBeacon;
        }

        //Events
        public SpawnerEvents spawnEvents;

        private void Start()
        {
            
        }

        private void Update()
        {
            
        }

        public Vector3 RandomPosition()
        {
            if(spawnPoint == null)
            {
                spawnPoint = transform;
            }

            float x = spawnPoint.position.x + UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            float z = spawnPoint.position.z + UnityEngine.Random.Range(-spawnRadius, spawnRadius);
            float y = spawnPoint.position.y + UnityEngine.Random.Range(-spawnHeight, spawnHeight);

            

            return new Vector3(x, y, z);

        }
    }
}
