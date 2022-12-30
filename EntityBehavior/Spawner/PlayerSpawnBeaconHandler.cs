using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Linq;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;

namespace Architome
{
    public class PlayerSpawnBeaconHandler : MonoBehaviour
    {
        public SpawnerInfo spawnerInfo;
        public AudioManager soundEffects;
        public bool activated;

        public bool startingSpawnBeacon;

        public WorldInfo worldInfo;

        public UnityEvent OnSetSpawnBeacon;
        public UnityEvent OnUnsetSpawnBeacon;

        WorkInfo workInfo;


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




            workInfo = GetComponentInParent<WorkInfo>();

            worldInfo = GMHelper.WorldInfo();
        }
        public void Start()
        {
            GetDependencies();

            if (startingSpawnBeacon)
            {
                OnSetSpawnBeacon?.Invoke();
                worldInfo.SetSpawnBeacon(spawnerInfo);
            }
        }

        async public void ReviveDeadPartyMembers()
        {
            var lastSpawnBeacon = GMHelper.WorldInfo().currentSpawnBeacon;
            GMHelper.WorldInfo().SetSpawnBeacon(spawnerInfo);
            var members = GameManager.active.playableEntities.Where(entity => !entity.isAlive).ToList();

            spawnerInfo.spawnEvents.OnStartRevivingParty?.Invoke(members.Count, GameManager.active.playableEntities.Count);


            foreach(var member in members)
            {
                WorldActions.active.ReviveAtSpawnBeacon(member);
                spawnerInfo.spawnEvents.OnReviveEntity?.Invoke(member);
                await Task.Delay(500);
            }

            GMHelper.WorldInfo().SetSpawnBeacon(lastSpawnBeacon);

        }

        public void SetAsLastPlayerSpawnBeacon()
        {
            spawnerInfo.spawnEvents.OnSetPlayerSpawnBeacon?.Invoke(spawnerInfo);
            OnSetSpawnBeacon?.Invoke();
            worldInfo.SetSpawnBeacon(spawnerInfo);
        }

    }

}

