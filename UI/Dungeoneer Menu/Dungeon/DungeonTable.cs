using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class DungeonTable : MonoBehaviour
    {
        [Serializable]
        public class DungeonInfo
        {
            public DungeonSet set;
            public int amount;
            public List<Size> allowedSize;
            public List<Dungeon> dungeons;
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject dungeon;
        }

        [Serializable]
        public struct Info
        {
            public Transform dungeonParent;
        }

        public Prefabs prefabs;
        public Info info;
        public List<DungeonInfo> dungeonInfos;



        public void CreateDungeonSets()
        {
            if (info.dungeonParent == null || prefabs.dungeon == null) return;

            foreach (var dungeonInfo in dungeonInfos)
            {
                if (dungeonInfo.dungeons == null)
                {
                    dungeonInfo.dungeons = new();
                }
                while (dungeonInfo.dungeons.Count < dungeonInfo.amount)
                {
                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();

                    newDungeon.SetDungeon(dungeonInfo);
                    dungeonInfo.dungeons.Add(newDungeon);
                }

            }
        }

        


    }
}
