using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    [System.Serializable]
    public class DataMap
    {
        public static DataMap active;
        public ArchitomeID idDatabase;
        

        public class Maps
        {
            public Dictionary<int, Item> items;
            public Dictionary<int, EntityInfo> entities;
            public Dictionary<int, BuffInfo> buffs;
            public Dictionary<int, ArchClass> archClasses;
            
        }

        Maps maps;

        public Maps _maps
        {
            get
            {
                if (maps == null)
                {
                    maps = GetMaps();
                }
                return maps;
            }
        }

        public void SetData()
        {
            maps = GetMaps();
            active = this;
        }

        public Maps GetMaps()
        {

            var map = new Maps();

            map.items = new();
                
            foreach (var item in idDatabase.Items)
            {
                map.items.Add(item._id, item);
            }

            map.buffs = new();
                
            foreach (var buff in idDatabase.Buffs)
            {
                map.buffs.Add(buff._id, buff);
            }

            map.entities = new();
                
            foreach (var entity in idDatabase.Entities)
            {
                map.entities.Add(entity._id, entity);
            }

            map.archClasses = new();
                
            foreach (var archClass in idDatabase.Classes)
            {
                map.archClasses.Add(archClass._id, archClass);
            }

            return map;

        }
    }
}
