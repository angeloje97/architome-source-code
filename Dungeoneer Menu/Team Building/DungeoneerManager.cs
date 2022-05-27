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
            public EntitySlot[] slots;

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

        public List<EntityInfo> selectedEntities
        {
            get
            {
                if (entities.slots == null) return null;

                return entities.slots.Select(slot => slot.entity).ToList();
            }
        }



        public Info info;
        public Prefabs prefabs;

        public SaveSystem saveSystem;

        public Mission currentMission;

        public string sceneToLoad;

        public Action<List<bool>> OnCheckCondition;
        public Action<List<bool>> BeforeCheckCondition;

        public Action<EntityInfo> OnNewEntity;

        void GetDependencies()
        {
            saveSystem = SaveSystem.active;

            if (saveSystem)
            {
                saveSystem.BeforeSave += BeforeSave;
            }
        }

        void SpawnEntities()
        {
            if (entities.parent == null) return;
            entities.pool = new();
            foreach (var entity in entities.poolPrefabs)
            {
                var newEntity = Instantiate(entity, entities.parent).GetComponent<EntityInfo>();

                entities.pool.Add(newEntity);

                var currentSave = Core.currentSave;

                if (currentSave != null)
                {
                    var (entityData, index) = currentSave.EntityData(newEntity);

                    EntityDataLoader.LoadEntity(entityData, newEntity);

                }


                newEntity.gameObject.SetActive(false);

                OnNewEntity?.Invoke(newEntity);

            }
        }

        public void CheckCondition()
        {
            var newCondition = new List<bool>();
            BeforeCheckCondition?.Invoke(newCondition);
            OnCheckCondition?.Invoke(newCondition);
        }

        private void Start()
        {
            GetDependencies();
            SpawnEntities();
        }

        public void BeforeSave(SaveGame currentSave)
        {
            foreach (var entity in entities.pool)
            {
                currentSave.SaveEntity(entity);
            }
        }


        public void PlayGame()
        {
            if (currentMission == null) return;
            if (selectedEntities == null || selectedEntities.Count == 0) return;

            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
