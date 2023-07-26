using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DataManager : MonoBehaviour
    {
        static DataManager active;

        public DataMap map;

        public static Dictionary<int, Item> Items => active.map._maps.items;
        public static Dictionary<int, EntityInfo> Entities => active.map._maps.entities;
        public static Dictionary<int, BuffInfo> Buffs => active.map._maps.buffs;
        public static Dictionary<int, ArchClass> Classes => active.map._maps.archClasses;
        public static Dictionary<int, RoomInfo> DungeonRooms => active.map._maps.dungeonRooms;

        private void Awake()
        {
            SingletonManger.HandleSingleton(GetType(), gameObject, true, onSuccess: () =>
            {
                active = this;
                map.SetData();
            });

            //if(active && active != this)
            //{
            //    Destroy(gameObject);
            //    return;
            //}

            //active = this;
            //ArchGeneric.DontDestroyOnLoad(gameObject);
            //map.SetData();
        }


    }
}
