using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MockSaveSystem : MonoBehaviour
    {
        public string saveGame;
        public bool save;
        public bool load;

        public List<EntityInfo> entitiesToSave;

        public List<EntityData> entityData;

        public SaveGame GameSave;

        private void Update()
        {
            HandleSave();
            HandleLoad();
        }

        void HandleSave()
        {
            if (!save) return;
            save = false;

            if (entitiesToSave.Count == 0) return;

            foreach (var entity in entitiesToSave)
            {
                SaveGame.current.SaveEntity(entity);
            }
            SaveGame.current.Save(saveGame);
            entityData = SaveGame.current.savedEntities;
            GameSave = SaveGame.current;
        }

        void HandleLoad()
        {
            if (!load) return;
            load = false;

            GameSave = SaveGame.current.LoadSave(this.saveGame);

            entityData = GameSave.savedEntities;

            EntityDataLoader.LoadEntities(entityData, entitiesToSave);

        }
    }
}
