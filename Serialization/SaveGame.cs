using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using Architome.Enums;
using Architome.Serialization;
using UnityEngine.SceneManagement;

namespace Architome
{
    [Serializable]
    public class SaveGame
    {


        public int saveId;
        public string saveName;
        public string build;
        public string timeString;
        public string currentSceneName;
        public string fileLocation;
        public bool newBorn;
        public DateTime time;
        public Trilogy trilogy;


        public GameSettingsData gameSettings;
        public World.Time worldTime;

        public GuildData guildData;
        public List<EntityData> savedEntities;

        public List<DungeonData> savedDungeons;

        public List<int> selectedEntitiesIndex;
        public DungeonData currentDungeon;

        [SerializeField] SaveUI saveUI;
        public SaveUI UI { get { saveUI ??= new(); return saveUI; } }


        public void SaveEntity(EntityInfo entity)
        {
            var data = EntityData(entity);

            

            if (data == null)
            {
                entity.SaveIndex = savedEntities.Count;
                data = new(entity, entity.SaveIndex);
                savedEntities.Add(data);
            }
            else
            {
                savedEntities[entity.SaveIndex] = new(entity, entity.SaveIndex);
            }
        }

        public EntityData EntityData(EntityInfo entity)
        {
            if (savedEntities == null) savedEntities = new();

            if (entity.SaveIndex == -1)
            {
                return null;
            }

            if (entity.SaveIndex >= savedEntities.Count)
            {
                return null;
            }

            return savedEntities[entity.SaveIndex];
        }

        public void SaveDungeon(Dungeon dungeon)
        {
            var data = DungeonData(dungeon);

            if (data == null)
            {
                dungeon.SaveIndex = savedDungeons.Count;
                data = new(dungeon, dungeon.SaveIndex);
                savedDungeons.Add(data);
            }
            else
            {
                savedDungeons[dungeon.SaveIndex] = new(dungeon, dungeon.SaveIndex);
            }
        }

        //public void SaveNewDungeon(Dungeon dungeon)
        //{
        //    savedDungeons ??= new();
        //    var data = new DungeonData(dungeon, dungeon.SaveIndex);
        //    savedDungeons.Add(data);
        //}

        public SaveGame()
        {
            saveUI = new();
        }

        public DungeonData DungeonData(Dungeon dungeon)
        {
            if (savedDungeons == null) savedDungeons = new();
            if (dungeon.SaveIndex == -1) return null;
            if (dungeon.SaveIndex >= savedDungeons.Count) return null;

            return savedDungeons[dungeon.SaveIndex];

        }

        public void Save(string saveName = "")
        {
            var saves = Core.AllSaves();

            if (saveId == 0)
            {
                saveId = saves.Count + 1;
            }

            saveName = $"Save{saveId}";

            currentSceneName = SceneManager.GetActiveScene().name;
            build = $"{Application.version}";
            time = DateTime.Now;
            timeString = time.ToString("MM/dd/yyyy h:mm tt");
            

            Debugger.InConsole(52064, $"Build {build} Time:{time}");

            if (this.saveName == null || this.saveName.Length == 0)
            {
                this.saveName = saveName;
            }

            var (success, fileLocation) = SerializationManager.SaveGame(saveName, this);

            this.fileLocation = fileLocation;
        }

        public string SaveFileName()
        {
            return $"Save{saveId}.save";
        }

        public SaveGame LoadSave(string saveName)
        {
            var obj = SerializationManager.LoadGame(saveName);

            var otherSave = (SaveGame)obj;

            Copy(otherSave);

            return otherSave;
        }

        public string LastSave()
        {
            return time.ToString("MM/dd/yyyy h:mm tt");
        }

        public void SetCurrentSave(SaveGame saveGame)
        {
            Copy(saveGame);
        }

        void Copy(SaveGame otherSave)
        {
            foreach (var field in typeof(SaveGame).GetFields())
            {
                field.SetValue(this, field.GetValue(otherSave));
            }
        }


    }

    
}
