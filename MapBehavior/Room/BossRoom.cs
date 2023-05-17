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
        public List<GameObject> possibleBosses;
        public WorkInfo bossStation;
        private void OnValidate()
        {
            for (int i = 0; i < possibleBosses.Count; i++)
            {
                var bossToSpawn = possibleBosses[i];

                if (!bossToSpawn.GetComponent<EntityInfo>())
                {
                    possibleBosses.RemoveAt(i);
                    i--;
                }
            }

            if (pool)
            {
                possibleBosses = pool.bossEntities;
            }

            if (bossStation)
            {
                foreach (var task in bossStation.tasks)
                {
                    task.properties.hideFromPlayers = true;
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
            if (entities.inRoom == null) return;
            
            var bosses = 0f;
            foreach (var entity in entities.inRoom)
            {
                if (entity.rarity != EntityRarity.Boss) continue;
                bosses++;
                entity.OnCombatChange += OnBossCombatChange;
                entity.OnDeath += OnBossDeath;

            }

            Debugger.InConsole(45329, $"{bosses} bosses out of {entities.inRoom.Count} entities");
        }

        public void OnBossCombatChange(bool isInCombat)
        {
            SetPaths(!isInCombat);
        }

        public void OnBossDeath(CombatEventData eventData)
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