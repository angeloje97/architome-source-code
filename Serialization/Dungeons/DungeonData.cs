using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    [Serializable]
    public class DungeonData
    {

        [Serializable]
        public class RoomData
        {
            public string levelName;
            public int roomLevel;
            public int entranceRoomID = -1;
            public int bossRoomID = -1;
            public int bossId = -1;
            public List<int> skeletonRoomsIDs;
            public List<int> randomRoomIDs;
            public string seed;

            public RoomData(Dungeon.Rooms level)
            {
                levelName = level.levelName;
                skeletonRoomsIDs = new();
                randomRoomIDs = new();
                seed = level.levelSeed;
                roomLevel = level.level;


                if (level.selectedBoss != null)
                {
                    bossId = level.selectedBoss._id;
                }
                else
                {
                    bossId = -1;
                }

                foreach (var room in level.skeleton)
                {
                    skeletonRoomsIDs.Add(room._id);
                }

                foreach (var room in level.random)
                {
                    randomRoomIDs.Add(room._id);
                }

                if (level.entrance)
                {
                    entranceRoomID = level.entrance._id;
                }

                if (level.boss)
                {
                    bossRoomID = level.boss._id;
                }
            }
        }

        public List<RoomData> levelDatas;

        public int currentLevel;
        public string seed;
        public int dungeonInfosIndex = -1;
        public int saveIndex = -1;
        public bool completed;
        public Size size;


        public DungeonData(Dungeon dungeon, int saveIndex)
        {
            levelDatas = new();
            foreach (var level in dungeon.levels)
            {
                levelDatas.Add(new(level));
            }


            size = dungeon.size;

            this.saveIndex = saveIndex;


            var dungeonTable = dungeon.GetComponentInParent<DungeonTable>();
            if (dungeonTable)
            {
                dungeonInfosIndex = dungeonTable.dungeonInfos.IndexOf(dungeon.dungeonInfo);
            }

            //foreach (var room in dungeon.rooms.skeleton)
            //{
            //    skeletonRoomIDs.Add(room._id);
            //}

            //foreach (var room in dungeon.rooms.random)
            //{
            //    randomRoomIDs.Add(room._id);
            //}



            //if (dungeon.rooms.entrance)
            //{
            //    entranceRoomID = dungeon.rooms.entrance._id;
            //}

            
            //if (dungeon.rooms.boss != null)
            //{
            //    bossRoomID = dungeon.rooms.boss._id;
            //}
        }

        public static bool Exists(DungeonData data)
        {
            if (data == null) return false;
            if (data.levelDatas == null) return false;
            if (data.levelDatas.Count == 0) return false;

            return true;
        }
    }
}
