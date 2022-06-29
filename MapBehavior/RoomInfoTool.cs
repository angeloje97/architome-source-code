using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class RoomInfoTool : MonoBehaviour
    {
        // Start is called before the first frame update

        public bool showEnemyPositions;
        public bool showPatrolSpots;
        public bool showPatrolGroupSpots;
        public bool showChestSpots;

        public RoomInfo info;
        private void OnValidate()
        {
            if (GetComponent<RoomInfo>())
            {
                info = GetComponent<RoomInfo>();
            }

            ShowEnemiesPos(showEnemyPositions);
            ShowPatrolSpots(showPatrolSpots);
            ShowPatrolGroupSpots(showPatrolGroupSpots);
            ShowChestSpots(showChestSpots);
        }

        private void Start()
        {
            showEnemyPositions = false;
            showPatrolSpots = false;
            showPatrolSpots = false;

            OnValidate();
        }

        void ShowChestSpots(bool val)
        {
            if (info.chestPos == null) return;

            foreach (Transform child in info.chestPos)
            {
                child.gameObject.SetActive(val);
            }
        }

        void ShowEnemiesPos(bool val)
        {
            if (info.tier1EnemyPos)
            {
                foreach (Transform child in info.tier1EnemyPos)
                {
                    child.gameObject.SetActive(val);
                }
            }

            if (info.tier2EnemyPos)
            {
                foreach (Transform child in info.tier2EnemyPos)
                {
                    child.gameObject.SetActive(val);
                }
            }

            if (info.tier3EnemyPos)
            {
                foreach (Transform child in info.tier3EnemyPos)
                {
                    child.gameObject.SetActive(val);
                }
            }
        }

        void ShowPatrolSpots(bool val)
        {
            if (info.patrolPoints)
            {
                foreach (Transform child in info.patrolPoints)
                {
                    child.gameObject.SetActive(val);
                }
            }

            if (info.GetType() != typeof(BossRoom)) return;

            var bossRoom = (BossRoom)info;

            if (bossRoom.bossPatrolSpots)
            {
                foreach (Transform child in bossRoom.bossPatrolSpots)
                {
                    child.gameObject.SetActive(val);
                }
            }
        }

        void ShowPatrolGroupSpots(bool val)
        {
            if (info.patrolGroups)
            {
                foreach (Transform group in info.patrolGroups)
                {
                    foreach (Transform child in group)
                    {
                        child.gameObject.SetActive(val);
                    }
                }
            }
        }
    }

}
