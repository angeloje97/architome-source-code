using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System;
using UnityEngine.UI;
using Architome.Enums;

namespace Architome
{
    [Serializable]
    public class SaveGame
    {
        //static SaveGame _current;
        //public static SaveGame current
        //{
        //    get
        //    {
        //        if (_current == null)
        //        {
        //            _current = new();
        //        }

        //        return _current;
        //    }

        //    private set
        //    {
        //        _current = value;
        //    }
        //}

        public int saveId;
        public string build;
        public string timeString;
        public DateTime time;
        public string saveName;
        public Trilogy trilogy;

        public GameSettingsData gameSettings;
        public List<EntityData> savedEntities;

        


        public void SaveEntity(EntityInfo entity)
        {
            var (data, index) = EntityData(entity);


            savedEntities[index] = new(entity);
            //if (data == null)
            //{
            //    data = new(entity);
            //    savedEntities.Add(data);
            //}
            //else
            //{
            //    savedEntities[index] = new(entity);
            //}
        }

        public (EntityData, int) EntityData(EntityInfo entity)
        {
            if (savedEntities == null) savedEntities = new();

            for(int i = 0; i < savedEntities.Count; i++)
            {
                var data = savedEntities[i];
                if (data.info.entityName == entity.entityName) //Identifies which data that the entity belongs to.
                {
                    return (data, i);
                }
            }

            var newData = new EntityData(entity);

            savedEntities.Add(newData);

            return (newData, savedEntities.IndexOf(newData));
        }

        public void Save(string saveName = "")
        {
            var saves = Core.AllSaves();

            if (saveId == 0)
            {
                saveId = saves.Count + 1;
            }

            saveName = $"Save{saveId}";


            build = $"{Application.version}";
            time = DateTime.Now;
            timeString = time.ToString("MM/dd/yyyy h:mm tt");
            

            Debugger.InConsole(52064, $"Build {build} Time:{time}");

            if (this.saveName == null || this.saveName.Length == 0)
            {
                this.saveName = saveName;
            }

            SerializationManager.SaveGame(saveName, this);
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
