using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace Architome
{
    public class DungeoneerManager : MonoBehaviour
    {
        [Serializable]
        public struct Entities
        {
            public Transform parent;
            public GameObject[] poolPrefabs;

            public List<EntityInfo> pool;
        }
        public Entities entities;

        [Serializable]
        public struct Info
        {
            public ArchButton startDungeonButton;
            public List<Icon> entityIcons;
            public TextMeshProUGUI partyLevel;
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject entityCard;
            public GameObject itemTemplate;
        }




        public Info info;
        public Prefabs prefabs;

        public SaveSystem saveSystem;
        public SaveGame currentSave;


        //Fields that will determine the dungeon.
        [SerializeField] List<EntityInfo> selectedEntities;
        [SerializeField] Dungeon currentDungeon;

        public string sceneToLoad;

        public Action<List<bool>> OnCheckCondition;
        public Action<List<bool>> BeforeCheckCondition;
        public Action<DungeoneerManager> BeforeUpdateParty;
        public Action<EntityInfo> OnNewEntity;
        public Action<SaveGame> OnNewSave;
        public Action<SaveGame> OnLoadSave;

        float dungeonLevel;
        float partyLevel;
        private void Start()
        {
            GetDependencies();
            LoadEntities();
            OnNewBorn();
            CheckCondition();
            UpdatePartyInfo();
        }

        void GetDependencies()
        {
            saveSystem = SaveSystem.active;
            currentSave = Core.currentSave;
        }

        void SpawnPresetEntities()
        {
            if (entities.parent == null) return;
            entities.pool = new();
            var currentSave = Core.currentSave;
            foreach (var entity in entities.poolPrefabs)
            {
                var newEntity = Instantiate(entity, entities.parent).GetComponent<EntityInfo>();

                entities.pool.Add(newEntity);

                ArchAction.Yield(() => {
                    if (currentSave != null)
                    {
                        currentSave.SaveEntity(newEntity);
                    }

                    OnNewEntity?.Invoke(newEntity);
                    GMHelper.GameManager().AddPlayableCharacter(newEntity);
                });

                //ArchAction.Delay(() => {

                //    if (currentSave != null)
                //    {
                //        currentSave.SaveEntity(newEntity);
                //    }

                //    OnNewEntity?.Invoke(newEntity);
                //    GMHelper.GameManager().AddPlayableCharacter(newEntity);
                //}, 1);

            }

        }


        async void LoadEntities()
        {
            if (currentSave == null) return;
            if (currentSave.newBorn) return;
            if (entities.parent == null) return;

            var dataMap = DataMap.active;

            if (dataMap == null) return;

            var db = dataMap._maps;

            if (db == null) return;

            var tasks = new List<Task<EntityInfo>>();

            for(int i = 0; i < currentSave.savedEntities.Count; i++)
            {
                var entityData = currentSave.savedEntities[i];



                //var newEntity = await EntityDataLoader.SpawnEntity(entityData, entities.parent);

                tasks.Add(EntityDataLoader.SpawnEntity(entityData, entities.parent));

                //entities.pool.Add(newEntity);

                //OnNewEntity?.Invoke(newEntity);

                //GMHelper.GameManager().AddPlayableCharacter(newEntity);
            }

            var entityList = await Task.WhenAll(tasks);


            foreach (var entity in entityList)
            {
                entities.pool.Add(entity);
                OnNewEntity?.Invoke(entity);
                GMHelper.GameManager().AddPlayableCharacter(entity);
            }
            OnLoadSave?.Invoke(currentSave);

        }

        void OnNewBorn()
        {
            if (currentSave == null)
            {
                SpawnPresetEntities();
                return;
            }

            if (!currentSave.newBorn) return;
            currentSave.newBorn = false;


            OnNewSave?.Invoke(currentSave);

            SpawnPresetEntities();
        }
        void UpdatePartyInfo()
        {
            if (selectedEntities == null) selectedEntities = new();
            for (int i = 0; i < info.entityIcons.Count; i++)
            {
                if (i >= selectedEntities.Count)
                {
                    info.entityIcons[i].SetIcon(new() { data = null });
                    continue;
                }

                info.entityIcons[i].SetIcon(new()
                {
                    sprite = selectedEntities[i].entityPortrait,
                    data = selectedEntities[i]
                });
            }

            info.partyLevel.text = partyLevel > 0 ? $"Party Level: {partyLevel}" : "";

        }

        void CheckCondition()
        {
            var newCondition = new List<bool>();



            newCondition.Add(partyLevel + 5 >= dungeonLevel);

            BeforeCheckCondition?.Invoke(newCondition);
            OnCheckCondition?.Invoke(newCondition);

            UpdateStartDungeonButton();

            void UpdateStartDungeonButton()
            {
                if (info.startDungeonButton == null) return;

                bool ready = true;

                foreach (var condition in newCondition)
                {
                    if (!condition)
                    {
                        ready = false;
                    }

                    Debugger.InConsole(5489, $"{condition}");
                }

                info.startDungeonButton.SetButton(ready);
            }
        }

        public void SetSelectedEntities(List<EntityInfo> entities, float partyLevel = 0f)
        {
            selectedEntities = entities;
            var currentSave = Core.currentSave;

            //if (currentSave == null) return;
            
            //if (selectedEntities.Count == 0)
            //{
            //    currentSave.selectedEntitiesIndex = new();
            //    return;
            //}

            if (currentSave != null)
            {
                currentSave.selectedEntitiesIndex = new(); /* selectedEntities.Select(entity => entity.SaveIndex).ToList();*/
            }
            
            for (int i = 0; i < info.entityIcons.Count; i++)
            {
                if (i >= selectedEntities.Count)
                {
                    info.entityIcons[i].SetIcon(new() { data = null });
                    continue;
                }

                if (currentSave != null)
                {
                    currentSave.selectedEntitiesIndex.Add(selectedEntities[i].SaveIndex);
                }

                info.entityIcons[i].SetIcon(new()
                {
                    data = selectedEntities[i],
                    sprite = selectedEntities[i].entityPortrait
                });
            }


            this.partyLevel = partyLevel;

            UpdatePartyInfo();
            CheckCondition();
        }


        public void SetDungeon(Dungeon dungeon)
        {
            currentDungeon = dungeon;

            dungeonLevel = dungeon.RecommendedLevel();

            CheckCondition();
        }

        public void StartDungeon()
        {
            if (currentDungeon == null) return;
            if (selectedEntities == null || selectedEntities.Count == 0) return;

            ArchSceneManager.active.LoadScene(sceneToLoad);
        }
    }
}
