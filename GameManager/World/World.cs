using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Runtime.CompilerServices;

namespace Architome
{

    //public class Time
    //{
    //    public static float deltaTime { get; set; }
    //}
    public class World : MonoBehaviour
    {

        public static World active;

        public static float deltaTime { get; private set; }


        public ArchitomeID database;
        // Start is called before the first frame update
        public GameObject defaultCatalyst;

        public float baseMovementSpeed;
        public float baseWalkSpeed;
        public bool noDisappearOnDeath;

        ArchSceneManager sceneManager;

        Action<float> OnUpdate { get; set; }



        [Serializable]
        public class UIPrefabs
        {
            public ItemInfo item;
            public ItemInfo worldItem;
            public InventorySlot inventorySlot;

            public GameObject ability;
            public GameObject entitySelector;
            public GameObject entityIcon;
            public GameObject icon;
        }

        public UIPrefabs prefabsUI;

        [Serializable]
        public class RarityProperties
        {
            public Rarity name;
            public Color color;
            public float valueMultiplier;
            [Range(0, 100)]
            public float rarityChance;

            public AudioClip revealAudio;
            public AudioClip lingerSound;
        }
        public List<RarityProperties> rarities;
        [Serializable]
        public class NPCProperty
        {
            public NPCType npcType;
            public Color color;
        }

        [Serializable]
        public class EntityRarityProperties
        {
            public EntityRarity rarity;
            public Color color;
            public int stars;
            public float valueMultiplier;
            [Range(0f, 100f)]
            public float chance;
        }
        public List<EntityRarityProperties> entityProperties;
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
            sceneManager= ArchSceneManager.active;

            if (saveSystem)
            {
                saveSystem.AddListener(SaveEvent.BeforeSave, BeforeSave, this);
            }

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }

        }

        private void OnDestroy()
        {
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
            deltaTime = time.timeScale * UnityEngine.Time.deltaTime;

            OnUpdate?.Invoke(deltaTime);
        }
        private void OnValidate()
        {
            //CreateProperties();
        }

        public void SetSpawnBeacon(SpawnerInfo spawner)
        {
            currentSpawnBeacon = spawner;
            OnNewSpawnBeacon?.Invoke(spawner);
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveTime();
        }

        public void BeforeSave(SaveSystem system, SaveGame save)
        {
            save.worldTime = time;
        }

        void SaveTime()
        {
            var currentSave = SaveSystem.current;
            if (currentSave == null) return;
            currentSave.worldTime = time;

        }

        void LoadTime()
        {
            var currentSave = SaveSystem.current;
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

        public RarityProperties RarityRoll(float chanceMultiplier = 1f)
        {
            var rarity = RarityProperty(Rarity.Poor);

            foreach (var prop in rarities)
            {
                if (prop.rarityChance == 0) continue;
                var roll = UnityEngine.Random.Range(0f, 100f);
                if (roll > prop.rarityChance*chanceMultiplier) continue;

                rarity = prop;
            }

            return rarity;
        }

        public RarityProperties RarityRoll(EntityRarity entityRarity)
        {
            var entityProperty = EntityRarityProperty(entityRarity);

            var rarity = RarityRoll(entityProperty.valueMultiplier);

            return rarity;
        }

        public RarityProperties RarityRoll(Rarity rarity)
        {
            var rarityProperty = RarityProperty(rarity);

            var newRarity = RarityRoll(rarityProperty.valueMultiplier);

            return newRarity;
        }

        public EntityRarityProperties EntityRarityProperty(EntityRarity rarity)
        {
            foreach (var entityRarity in entityProperties)
            {
                if (entityRarity.rarity != rarity) continue;

                return entityRarity;
            }

            return null;
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

            public Action<int, int> OnNewTimeScale { get; set; }
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

            public void ChangeTimeScale(int newTimeScale)
            {
                if (timeScale == newTimeScale) return;

                OnNewTimeScale?.Invoke(timeScale, newTimeScale);

                timeScale = newTimeScale;
            }
        }

        #region Static functions
        public static async Task Delay(float seconds)
        {
            var world = active;
            if(world == null)
            {
                await Task.Delay((int)(seconds * 1000));
                
                return;
            }

            var currentTime = 0f;
            var waiting = true;
            world.OnUpdate += HandleUpdate;

            while (waiting)
            {
                await Task.Yield();
            };

            world.OnUpdate -= HandleUpdate;

            void HandleUpdate(float deltaTime)
            {
                if(currentTime < seconds)
                {
                    currentTime += deltaTime;
                    return;
                }
                waiting = false;

            }
        }

        public static async Task Yield()
        {
            var world = active;
            if(world == null)
            {
                await Task.Yield();
                return;
            }

            var waiting = true;

            world.OnUpdate += HandleUpdate;
            while (waiting)
            {
                await Task.Yield();
            }
            world.OnUpdate -= HandleUpdate;


            void HandleUpdate(float deltaTime)
            {
                waiting = false;
            }
        }

        public static async Task ActionInterval(Predicate<float> interval, float timeBetween, bool invokeAtStart = true)
        {
            var running = true;

            var currentTime = invokeAtStart ? timeBetween : 0f;
            var world = active;
            if (world == null) return;

            world.OnUpdate += HandleInterval;

            while (running)
            {
                await Task.Yield();
            }

            world.OnUpdate -= HandleInterval;

            void HandleInterval(float deltaTime)
            {
                if(currentTime < timeBetween)
                {
                    currentTime += deltaTime;
                    return;
                }

                currentTime = 0f;
                if (!interval(deltaTime))
                {
                    running = false;
                }
            }
        }

        public static async Task UpdateAction(Predicate<float> action)
        {

            var world = active;
            var running = true;

            world.OnUpdate += MiddleWare;


            while (running)
            {
                await Task.Yield();
            }

            if (world == null) return;

            world.OnUpdate -= MiddleWare;

            void MiddleWare(float deltaTime)
            {
                if (!running) return;
                if (!action(deltaTime))
                {
                    running = false;
                }
            }
        }

        public static async Task UpdateComponent(Action<float> action, Component component)
        {
            await UpdateAction((float deltaTime) => {
                if (component == null) return false;

                action(deltaTime);

                return true;
            });
        }


        public static async Task UntilMatch(Predicate<object> focus, bool target)
        {
            while(focus(null) != target)
            {
                await Task.Yield();
            }
        }
        #endregion
    }

}