using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentSpawn : AugmentType
    {
        
        [Serializable]
        public struct SpawnInfo
        {
            public GameObject spawnable;
            public CatalystEvent catalystEvent;
            public bool attachToGround;
        }

        [Header("Augment Spawn")]
        public List<SpawnInfo> spawnInfos;

        LayerMask groundLayer;

        void Start()
        {
            GetDependencies();
        }
        void Update()
        {

        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableCatalyst();

            var layerMaskData = LayerMasksData.active;
            if (layerMaskData)
            {
                groundLayer = layerMaskData.walkableLayer;
            }
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            foreach (var spawnInfo in spawnInfos)
            {
                switch (spawnInfo.catalystEvent)
                {
                    case CatalystEvent.OnAwake:
                        HandleSpawnObject(spawnInfo, catalyst);
                        break;

                    case CatalystEvent.OnAssist:
                        catalyst.OnAssist += (CatalystInfo catalyst, EntityInfo target) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnCatalingRelease:
                        catalyst.OnCatalingRelease += (CatalystInfo original, CatalystInfo cataling) => {
                            HandleSpawnObject(spawnInfo, original);
                        };
                        break;

                    case CatalystEvent.OnDestroy:
                        catalyst.OnCatalystDestroy += (CatalystDeathCondition condition) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnHarm:
                        catalyst.OnDamage += (CatalystInfo catalyst, EntityInfo target) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnHeal:
                        catalyst.OnHeal += (CatalystInfo catalyst, EntityInfo target) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnHit:
                        catalyst.OnHit += (CatalystInfo catalyst, EntityInfo target) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnInterval:
                        catalyst.OnInterval += (CatalystInfo catalyst) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;

                    case CatalystEvent.OnStop:
                        catalyst.OnCatalystStop += (CatalystKinematics kinematics) => {
                            HandleSpawnObject(spawnInfo, catalyst);
                        };
                        break;


                }
            }
        }

        void HandleSpawnObject(SpawnInfo spawn, CatalystInfo catalyst)
        {
            SetCatalyst(catalyst, true);
            augment.TriggerAugment(new(this));

            var position = !spawn.attachToGround ? catalyst.transform.position : V3Helper.GroundPosition(catalyst.transform.position, groundLayer);

            var room = Entity.Room(catalyst.transform.position);

            var newObject = Instantiate(spawn.spawnable, position, spawn.spawnable.transform.rotation);

            if (room)
            {
                newObject.transform.SetParent(room.transform);
            }

            
        }

        // Update is called once per frame
        
    }
}
