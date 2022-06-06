using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Threading.Tasks;

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
        public Action<EntityInfo> OnNewEntity;
        public Action<SaveGame> OnNewSave;
        public Action<SaveGame> OnLoadSave;

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

        void CheckCondition()
        {
            var newCondition = new List<bool>();
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

        

        private void Start()
        {
            GetDependencies();
            LoadEntities();
            OnNewBorn();
            CheckCondition();
        }

        public void SetSelectedEntities(List<EntityInfo> entities)
        {
            selectedEntities = entities;
            var currentSave = Core.currentSave;

            if (currentSave == null) return;
            
            if (selectedEntities.Count == 0)
            {
                currentSave.selectedEntitiesIndex = new();
                return;
            }

            currentSave.selectedEntitiesIndex = selectedEntities.Select(entity => entity.SaveIndex).ToList();

            CheckCondition();
        }

        public void SetDungeon(Dungeon dungeon)
        {
            currentDungeon = dungeon;

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
