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
            public int entranceRoomID = -1;
            public int bossRoomID = -1;
            public List<int> skeletonRoomsIDs;
            public List<int> randomRoomIDs;

            public RoomData(Dungeon.Rooms level)
            {
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
        public int dungeonSetID;
        public int saveIndex = -1;
        public Size size;

        public DungeonData(Dungeon dungeon, int saveIndex)
        {
            levelDatas = new();
            foreach (var level in dungeon.levels)
            {
                levelDatas.Add(new(level));
            }


            seed = dungeon.seed;
            size = dungeon.size;

            this.saveIndex = saveIndex;

            dungeonSetID = dungeon.dungeonInfo.set._id;

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
    }
}
