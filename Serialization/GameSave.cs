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
        static SaveGame _current;
        public static SaveGame current
        {
            get
            {
                if (_current == null)
                {
                    _current = new();
                }

                return _current;
            }
        }

        public string build;
        public string time;

        public List<EntityData> savedEntities;

        

        public void SaveEntities(List<EntityInfo> entities)
        {
            savedEntities = new();

            foreach (var entity in entities)
            {
                savedEntities.Add(new(entity));
            }
        }

        public void SaveEntity(EntityInfo entity)
        {
            if (savedEntities == null) savedEntities = new();

            var (data, index) = EntityData(entity);

            if (data == null)
            {
                data = new(entity);
                savedEntities.Add(data);
                
            }
            else
            {
                savedEntities[index] = new(entity);
            }


        }

        public (EntityData, int) EntityData(EntityInfo entity)
        {
            for(int i = 0; i < savedEntities.Count; i++)
            {
                var data = savedEntities[i];
                if (data.info.entityName == entity.entityName)
                {
                    return (data, i);
                }
            }

            return (null, 0);
        }

        public void Save(string saveName)
        {
            build = $"v{Application.version}";
            DateTime localDate = DateTime.Now;

            time = localDate.ToString("MM/dd/yyyy h:mm tt");
            

            Debugger.InConsole(52064, $"Build {build} Time:{time}");
            SerializationManager.Save(saveName, this);
        }

        public SaveGame LoadSave(string saveName)
        {
            var obj = SerializationManager.Load(saveName);

            var otherSave = (SaveGame)obj;

            Copy(otherSave);

            return otherSave;

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
