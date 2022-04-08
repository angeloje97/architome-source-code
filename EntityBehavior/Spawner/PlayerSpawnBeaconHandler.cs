using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;

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

        public 

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
        public void OnSelectOption(Clickable clickable)
        {

        }
        public void OnActivate(ActivatorData eventData)
        {
            var entity = eventData.gameObject;
            if (!Entity.IsPlayer(entity)) { return; }
            activated = true;

            SetAsLastPlayerSpawnBeacon();
            HandleActivateObjects();
        }

        async public void ReviveDeadPartyMembers()
        {
            var lastSpawnBeacon = GMHelper.WorldInfo().lastPlayerSpawnBeacon;
            GMHelper.WorldInfo().lastPlayerSpawnBeacon = spawnerInfo;
            var members = GameManager.active.playableEntities.Where(entity => !entity.isAlive).ToList();

            spawnerInfo.spawnEvents.OnStartRevivingParty?.Invoke(members.Count, GameManager.active.playableEntities.Count);


            foreach(var member in members)
            {
                WorldActions.active.ReviveAtSpawnBeacon(member.gameObject);
                spawnerInfo.spawnEvents.OnReviveEntity?.Invoke(member);
                await Task.Delay(500);
            }

            GMHelper.WorldInfo().lastPlayerSpawnBeacon = lastSpawnBeacon;

        }

        public void SetAsLastPlayerSpawnBeacon()
        {
            spawnerInfo.spawnEvents.OnSetPlayerSpawnBeacon?.Invoke(spawnerInfo);
            worldInfo.lastPlayerSpawnBeacon = spawnerInfo;
        }

    }

}

