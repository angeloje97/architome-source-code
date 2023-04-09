using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Architome.Enums;

namespace Architome
{
    public class DungeoneerManager : MonoBehaviour
    {
        public static DungeoneerManager active;

        [Serializable]
        public struct Entities
        {
            public Transform parent;
            public GameObject[] poolPrefabs;

            public List<EntityInfo> pool;
        }
        public Entities entities;

        

        [Serializable]
        public struct Prefabs
        {
            public GameObject entityCard, itemTemplate, ability, entitySelector, entityIcon, inventorySlot;
        }




        public Prefabs prefabs;

        public SaveSystem saveSystem;
        public SaveGame currentSave;


        //Fields that will determine the dungeon.
        [SerializeField] public List<EntityInfo> selectedEntities;
        [SerializeField] Dungeon currentDungeon;

        public ArchScene sceneToLoad;

        public Action<DungeoneerManager> BeforeUpdateParty;
        public Action<DungeoneerManager, List<EntityInfo>> OnSetSelectedMembers { get; set; }
        public Action<EntityInfo> OnNewEntity;
        public Action<SaveGame> OnNewSave;
        public Action<SaveGame> OnLoadSave;

        public float dungeonLevel { get; set; }
        public float partyLevel { get; set; }
        private void Start()
        {
            GetDependencies();
            LoadEntities();
            OnNewBorn();
        }

        private void Awake()
        {
            active = this;
        }

        void GetDependencies()
        {
            saveSystem = SaveSystem.active;
            currentSave = Core.currentSave;

        }

        #region Entities
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



                tasks.Add(EntityDataLoader.SpawnEntity(entityData, entities.parent));

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
        public void SetSelectedEntities(List<EntityInfo> entities, float partyLevel = 0f)
        {
            selectedEntities = entities;
            this.partyLevel = partyLevel;
            var currentSave = Core.currentSave;


            if (currentSave != null)
            {
                currentSave.selectedEntitiesIndex = new(); /* selectedEntities.Select(entity => entity.SaveIndex).ToList();*/
                foreach(var entity in selectedEntities)
                {
                    currentSave.selectedEntitiesIndex.Add(entity.SaveIndex);
                }
            }

            OnSetSelectedMembers?.Invoke(this, selectedEntities);

        }
        #endregion

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

        public void SetDungeon(Dungeon dungeon)
        {
            currentDungeon = dungeon;

            dungeonLevel = dungeon.RecommendedLevel();

        }

        public void StartDungeon()
        {
            if (currentDungeon == null) return;
            if (selectedEntities == null || selectedEntities.Count == 0) return;

            ArchSceneManager.active.LoadScene(sceneToLoad);
        }
    }
}
