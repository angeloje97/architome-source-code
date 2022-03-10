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


        Clickable clickable;


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


            if(GetComponentInParent<Clickable>())
            {
                clickable = GetComponentInParent<Clickable>();
            }

            worldInfo = GMHelper.WorldInfo();

            if(startingSpawnBeacon)
            {
                worldInfo.lastPlayerSpawnBeacon = spawnerInfo;
                HandleActivateObjects();
            }
        }
        void HandleClickable()
        {
            if(clickable == null) { return; }

            clickable.options.Add("Gaze into the fire (Set Spawn)");
            clickable.options.Add("Revive Fallen Allies");

            clickable.OnSelectOption += OnSelectOption;
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
            HandleClickable();
        }

        public void OnSelectOption(string option)
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

