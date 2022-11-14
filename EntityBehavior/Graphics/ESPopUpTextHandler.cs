using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class ESPopUpTextHandler : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Info
        {
            public SpawnerInfo spawner;
            public PopupTextManager popUpManager;
        }

        public Info info;

        void GetDependencies()
        {
            info.spawner = GetComponentInParent<SpawnerInfo>();
            info.popUpManager = PopupTextManager.active;

            if (info.spawner)
            {
                info.spawner.spawnEvents.OnStartRevivingParty += OnStartRevivingParty;
                info.spawner.spawnEvents.OnSetPlayerSpawnBeacon += OnSetPlayerSpawnBeacon;
            }

        }

        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame

        public void OnStartRevivingParty(int dead, int totalMembers)
        {
            if (info.popUpManager == null) return;

            var text = dead > 0 ? $"Reviving {dead} members" : "No dead party members to revive";
            info.popUpManager.GeneralPopUp(transform, text, Color.white);
        }

        public void OnSetPlayerSpawnBeacon(SpawnerInfo spawner)
        {
            if (info.popUpManager == null) return;

            info.popUpManager.GeneralPopUp(transform, $"Player Spawn Beacon Set", Color.white);
        }
    }


}