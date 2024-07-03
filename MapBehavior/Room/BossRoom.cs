using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BossRoom : RoomInfo
    {
        // Start is called before the first frame update
        [Header("Boss Room")]
        public Transform bossPosition;
        public Transform bossPatrolSpots;
        public List<EntityInfo> possibleBosses;
        public List<WorkInfo> bossStations;


        #region Spawn Positions

        public override RoomSpawnPositions SpawPositionFromTier(EntityTier tier)
        {
            UpdateSpawnPosititionMap();

            return spawnPositionMap[tier];
        }

        #endregion

        private void OnValidate()
        {
            for (int i = 0; i < possibleBosses.Count; i++)
            {
                var bossToSpawn = possibleBosses[i];

                if (!bossToSpawn)
                {
                    possibleBosses.RemoveAt(i);
                    i--;
                }
            }

            if (pool)
            {
                possibleBosses = pool.bossEntities;
            }

            if (bossStations != null)
            {
                foreach (var station in bossStations)
                {
                    station.HideStationFromPlayers();
                }
            }

        }
        void Start()
        {
            GetDependencies();
            //CheckBadSpawn();
        }


        public override async void OnEntitiesGenerated(MapEntityGenerator generator)
        {
            base.OnEntitiesGenerated(generator);
            await World.Delay(2f);
            var entitiesInRoom = entities.EntitiesInRoom;
            if (entitiesInRoom == null) return;
            
            var bosses = 0f;


            foreach (var entity in entitiesInRoom)
            {
                if (entity.rarity != EntityRarity.Boss) continue;
                bosses++;
                entity.OnCombatChange += OnBossCombatChange;
                entity.combatEvents.AddListenerHealth(eHealthEvent.OnDeath, OnBossDeath, this);

            }

        }

        public void OnBossCombatChange(bool isInCombat)
        {
            SetPaths(!isInCombat);
        }

        public void OnBossDeath(HealthEvent eventData)
        {
            if (chestPos == null) return;
            if (pool == null) return;
            if (pool.chests == null) return;



            //foreach (Transform trans in chestPos)
            //{
            //    var randomChest = ArchGeneric.RandomItem(pool.chests);

            //    var newChest = Instantiate(randomChest, trans.position, trans.rotation);
            //    newChest.transform.SetParent(transform);
            //}
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}