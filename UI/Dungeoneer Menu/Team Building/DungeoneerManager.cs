using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Linq;

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
            public GameObject entityIcon;
        }




        public Info info;
        public Prefabs prefabs;

        public SaveSystem saveSystem;

        public SaveGame currentSave;


        //Fields that will determine the dungeon.
        public List<EntityInfo> selectedEntities;
        public Dungeon currentDungeon;

        public string sceneToLoad;

        public Action<List<bool>> OnCheckCondition;
        public Action<List<bool>> BeforeCheckCondition;
        public Action<EntityInfo> OnNewEntity;
        public Action<SaveGame> OnNewSave;

        void GetDependencies()
        {
            saveSystem = SaveSystem.active;

            if (saveSystem)
            {
                saveSystem.BeforeSave += BeforeSave;
            }

            currentSave = Core.currentSave;
        }

        void SpawnEntities()
        {
            if (entities.parent == null) return;
            entities.pool = new();
            var currentSave = Core.currentSave;
            foreach (var entity in entities.poolPrefabs)
            {
                var newEntity = Instantiate(entity, entities.parent).GetComponent<EntityInfo>();

                entities.pool.Add(newEntity);

                ArchAction.Delay(() => {

                    if (currentSave != null)
                    {
                        var (entityData, index) = currentSave.EntityData(newEntity);

                        EntityDataLoader.LoadEntity(entityData, newEntity);
                    }

                    OnNewEntity?.Invoke(newEntity);
                    GMHelper.GameManager().AddPlayableCharacter(newEntity);
                }, 1);

            }

            if (currentSave != null && currentSave.newBorn)
            {
                OnNewSave?.Invoke(currentSave);
                currentSave.newBorn = false;
            }
        }

        public void CheckCondition()
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
                        break;
                    }
                }

                info.startDungeonButton.SetButton(ready);
            }
        }

        

        private void Start()
        {
            GetDependencies();
            SpawnEntities();
            CheckCondition();
        }

        public void BeforeSave(SaveGame currentSave)
        {
            foreach (var entity in entities.pool)
            {
                currentSave.SaveEntity(entity);
            }
        }

        public void SetSelectedEntities(List<EntityInfo> entities)
        {
            selectedEntities = entities;
        }


        public void StartDungeon()
        {
            if (currentDungeon == null) return;
            if (selectedEntities == null || selectedEntities.Count == 0) return;

            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
