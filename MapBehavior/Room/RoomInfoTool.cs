using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class RoomInfoTool : MonoBehaviour
    {
        public static RoomInfo activeRoom;
        public RoomInfo activeStaticRoom;
        public bool setActiveRoom;

        public bool showEnemyPositions;
        public bool showPatrolSpots;
        public bool showPatrolGroupSpots;
        public bool showChestSpots;
        public bool updatePaths;

        public RoomInfo info;

        public bool showRoom;
        private void OnValidate()
        {
            if (GetComponent<RoomInfo>())
            {
                info = GetComponent<RoomInfo>();
            }

            HandleSetActiveRoom();

            ShowEnemiesPos(showEnemyPositions);
            ShowPatrolSpots(showPatrolSpots);
            ShowPatrolGroupSpots(showPatrolGroupSpots);
            ShowChestSpots(showChestSpots);
            UpdatePaths();
            ShowRoom();
        }

        void ShowRoom()
        {
            if (!showRoom) return;
            showRoom = false;

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = true;
            }
        }

        private void Start()
        {
            showEnemyPositions = false;
            showPatrolSpots = false;
            showPatrolSpots = false;

            //OnValidate();
            
        }

        void HandleSetActiveRoom()
        {
            if (!setActiveRoom) return;
            setActiveRoom = false;
            activeRoom = info;

            activeStaticRoom = activeRoom;
        }

        void UpdatePaths()
        {
            if (!updatePaths) return;
            updatePaths = false;

            info.paths = GetComponentsInChildren<PathInfo>().ToList();
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
            var entitySpawnPositions = info.EntitySpawnPositions();

            foreach(var spawnPos in entitySpawnPositions)
            {
                foreach(Transform child in spawnPos.parent)
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
