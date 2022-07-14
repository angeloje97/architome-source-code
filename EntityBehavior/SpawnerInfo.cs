using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SpawnerInfo : EntityInfo
    {
        // Start is called before the first frame update
        [Header("Spawner Properties")]
        public Transform spawnPoint;
        public float spawnRadius;
        public float spawnHeight;
        public float maxLiveTime;
        public float spawnIntervals;
        public int spawnPerIntervals;
        public int maxSpawnedEntities;
        public bool active;

        public List<EntityInfo> currentlySpawned;
        public List<EntityInfo> spawnPool;

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
            EntityStart();
            HandleSpawner();
        }

        private void Update()
        {
            HandleEventTriggers();
        }

        async void HandleSpawner()
        {
            if (active) return;
            if (spawnPool == null || spawnPool.Count == 0)
            {
                active = false;
                return;
            }

            await Task.Delay((int)(spawnIntervals * 1000));

            while (isAlive)
            {
                SpawnEntities(spawnPerIntervals);
                await Task.Delay((int)(spawnIntervals * 1000));
            }

            active = false;
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

        public void SpawnEntities(int count = 1)
        {
            if (spawnPool == null || spawnPool.Count == 0) return;
            if (currentlySpawned == null) currentlySpawned = new();
            
            if (currentlySpawned.Count + count > maxSpawnedEntities)
            {
                count = maxSpawnedEntities - currentlySpawned.Count;
            }
            if (count <= 0) return;
            var worldAction = WorldActions.active;
            if (worldAction == null) return;

            for (int i = 0; i < count; i++)
            {
                if (currentlySpawned.Count >= maxSpawnedEntities) break;
                var entity = ArchGeneric.RandomItem(spawnPool);

                var spawnedEntity = worldAction.SpawnEntity(entity.gameObject, RandomPosition()).GetComponent<EntityInfo>();

                spawnedEntity.SetSummoned(new()
                {
                    master = this,
                    liveTime = maxLiveTime
                });

                currentlySpawned.Add(spawnedEntity);

                spawnedEntity.OnLifeChange += (bool isAlive) => {
                    if (!isAlive)
                    {
                        currentlySpawned.Remove(spawnedEntity);
                    }
                };
            }

            
        }

        public struct SummonData {
            public AbilityInfo sourceAbility;
            public EntityInfo master;
            public float liveTime;
        }
    }
}
