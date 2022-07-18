using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;


namespace Architome
{
    public class World : MonoBehaviour
    {

        public static World active;

        public ArchitomeID database;
        // Start is called before the first frame update
        public GameObject defaultCatalyst;

        public float baseMovementSpeed;
        public float baseWalkSpeed;

        

        [Serializable]
        public class RarityProperties
        {
            public Rarity name;
            public Color color;
            public float valueMultiplier;
        }
        public List<RarityProperties> rarities;
        [Serializable]
        public class NPCProperty
        {
            public NPCType npcType;
            public Color color;
        }

        public List<NPCProperty> npcProperties;

        public SpawnerInfo currentSpawnBeacon { get; private set; }

        public Action<SpawnerInfo> OnNewSpawnBeacon;

        public Time time;


        void Start()
        {
            var gameManager = GameManager.active;
            if (gameManager.GameState == GameState.Menu) return;

            GetDependencies();
            LoadTime();
            StartTime();
        }

        void StartTime()
        {
            if (time == null) return;
            time.HandleTimer();
        }
        
        void GetDependencies()
        {
            var saveSystem = SaveSystem.active;
            var archSceneManager = ArchSceneManager.active;

            if (saveSystem)
            {
                saveSystem.BeforeSave += (SaveGame save) => { save.worldTime = time; };
            }

            if (archSceneManager)
            {
                archSceneManager.BeforeLoadScene += (ArchSceneManager sceneManager) => { SaveTime(); };
            }
        }
        private void Awake()
        {
            //if (active)
            //{
            //    foreach (var field in typeof(World).GetFields())
            //    {
            //        field.SetValue(this, field.GetValue(active));
            //    }

            //    foreach (var property in typeof(World).GetProperties())
            //    {
            //        property.SetValue(this, property.GetValue(active));
            //    }
                
            //}

            active = this;
        }
        void Update()
        {
        }

        public void SetSpawnBeacon(SpawnerInfo spawner)
        {
            currentSpawnBeacon = spawner;
            OnNewSpawnBeacon?.Invoke(spawner);
        }


        private void OnValidate()
        {
            CreateProperties();
        }

        void SaveTime()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            currentSave.worldTime = time;
        }

        void LoadTime()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;
            if (currentSave.worldTime == null) return;
            time = currentSave.worldTime;
        }

        void CreateProperties()
        {
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                if (ContainsRarity(rarity))
                {
                    continue;
                }

                rarities.Add(new() { name = rarity});
            }

            bool ContainsRarity(Rarity rarity)
            {
                foreach (var property in rarities)
                {
                    if (property.name == rarity)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public RarityProperties RarityProperty(Rarity rarity)
        {
            foreach (var property in rarities)
            {
                if (property.name == rarity)
                {
                    return property;
                }
            }

            return new RarityProperties();
        }

        public NPCProperty NPCPRoperty(NPCType type)
        {
            foreach (var property in npcProperties)
            {
                if (property.npcType == type)
                {
                    return property;
                }
            }

            return null;
        }

        [Serializable]
        public class Time
        {
            public float currentTime, deltaTime, lengthOfDay;

            public int timeScale = 1;
            public int day;

            public int week { get { return day % 7; } }
            public float hour { get { return (currentTime / lengthOfDay) * 24; } }
            public float minute { get { return (hour - (int)hour) * 60; } }

            public float clockHour;
            public float clockMinute;
            async public void HandleTimer()
            {

                while (Application.isPlaying)
                {
                    var next = currentTime + UnityEngine.Time.deltaTime * timeScale;
                    deltaTime = next - currentTime;
                    currentTime = next;
                    clockHour = hour;
                    clockMinute = minute;

                    if (currentTime > lengthOfDay)
                    {
                        day++;
                        currentTime = 0;

                    }

                    await Task.Yield();
                }
            }
        }
    }

}