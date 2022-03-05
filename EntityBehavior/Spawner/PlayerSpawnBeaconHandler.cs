using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


            worldInfo = GMHelper.WorldInfo();

            if(startingSpawnBeacon)
            {
                worldInfo.lastPlayerSpawnBeacon = spawnerInfo;
                HandleActivateObjects();
            }
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

