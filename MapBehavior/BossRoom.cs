using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BossRoom : RoomInfo
    {
        // Start is called before the first frame update
        [Header("Boss Room")]
        public Transform bossPosition;
        public Transform rewardChestPositions;
        public Transform bossPatrolSpots;
        public List<GameObject> possibleBosses;

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
        }
        void Start()
        {
            GetDependencies();
            CheckBadSpawn();
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}