using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class PlayerSpawnBeaconHandler : MonoBehaviour
    {
        public SpawnerInfo spawnerInfo;
        public AudioManager soundEffects;
        public bool activated;
        public List<GameObject> objectsToActivate;
        public List<GameObject> disableOnStart;

        public bool startingSpawnBeacon;

        public WorldInfo worldInfo;


        WorkInfo workInfo;
        public 


        void GetDependencies()
        {
            if(GetComponentInParent<SpawnerInfo>())
            {
                spawnerInfo = GetComponentInParent<SpawnerInfo>();
                if(spawnerInfo.SoundEffect())
                {
                    soundEffects = spawnerInfo.SoundEffect();
                }
            }
            else
            {
                return;
            }



            if(GetComponentInParent<WorkInfo>())
            {
                workInfo = GetComponentInParent<WorkInfo>();
            }

            worldInfo = GMHelper.WorldInfo();

            if(startingSpawnBeacon)
            {
                worldInfo.lastPlayerSpawnBeacon = spawnerInfo;
                HandleActivateObjects();
            }
        }
        void HandleWork()
        {
            if(workInfo == null) { return; }

            workInfo.CreateTask(new TaskInfo()
            {
                workString = "Set Spawn Beacon",
                workType = Enums.WorkType.Use,
                workAmount = 3
            });

            workInfo.CreateTask(new TaskInfo()
            {
                workString = "Revive Allies",
                workType = Enums.WorkType.Use,
                workAmount = 10
            });


        }
        void HandleDisableOnStart()
        {
            foreach(var i in disableOnStart)
            {
                i.SetActive(false);
            }
        }
        void HandleActivateObjects()
        {
            foreach(var i in objectsToActivate)
            {
                i.SetActive(true);
            }
        }
        public void Start()
        {
            HandleDisableOnStart();
            GetDependencies();
            HandleWork();
        }

        public void OnSelectOption(Clickable clickable)
        {

        }


        public void OnActivate(ActivatorData eventData)
        {
            var entity = eventData.gameObject;
            if (!Entity.IsPlayer(entity)) { return; }
            activated = true;

            worldInfo.lastPlayerSpawnBeacon = spawnerInfo;
            HandleActivateObjects();
        }

    }

}

