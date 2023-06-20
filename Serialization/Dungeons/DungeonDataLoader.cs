using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public static class DungeonDataLoader
    {
        // Start is called before the first frame update

        public static List<Dungeon.Rooms> DungeonLevels(DungeonData data)
        {
            var levels = new List<Dungeon.Rooms>();



            foreach (var rooms in data.levelDatas)
            {
                var level = new Dungeon.Rooms();

                level.levelName = rooms.levelName;
                level.levelSeed = rooms.seed;
                var maps = DataMap.active;

                if (maps == null) return levels;

                var dungeons = maps._maps.dungeonRooms;

                if (dungeons == null) return levels;

                if (rooms.bossId != -1)
                {
                    var entities = maps._maps.entities;

                    if (entities.ContainsKey(rooms.bossId))
                    {
                        level.selectedBoss = entities[rooms.bossId];
                    }
                }

                var skeleton = new List<RoomInfo>();

                foreach (var id in rooms.skeletonRoomsIDs)
                {
                    if (!dungeons.ContainsKey(id)) continue;

                    var room = dungeons[id];

                    skeleton.Add(room);
                }

                var random = new List<RoomInfo>();

                foreach (var id in rooms.randomRoomIDs)
                {
                    if (!dungeons.ContainsKey(id)) continue;

                    var room = dungeons[id];

                    random.Add(room);
                }

                if (rooms.entranceRoomID != -1)
                {
                    if (dungeons.ContainsKey(rooms.entranceRoomID))
                    {
                        level.entrance = dungeons[rooms.entranceRoomID];
                    }
                }

                if (rooms.bossRoomID != -1)
                {
                    if (dungeons.ContainsKey(rooms.bossRoomID))
                    {
                        level.boss = dungeons[rooms.bossRoomID];
                    }
                }

                level.level = rooms.roomLevel;
                level.random = random;
                level.skeleton = skeleton;
                levels.Add(level);
            }
            

            return levels;
        }
          
        public static Dungeon LoadDungeon(DungeonData data, Dungeon dungeon)
        {
            if (data == null || dungeon == null) return null;

            var maps = DataMap.active._maps;

            if (maps == null) return null;

            dungeon.levels = new();

            dungeon.levels = DungeonLevels(data);


            dungeon.size = data.size;
            dungeon.SaveIndex = data.saveIndex;


            return dungeon;
        }
    }
}
