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
        public GameObject bossToSpawn;

        private void OnValidate()
        {
            if (bossToSpawn && bossToSpawn.GetComponent<EntityInfo>() == null)
            {
                bossToSpawn = null;
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