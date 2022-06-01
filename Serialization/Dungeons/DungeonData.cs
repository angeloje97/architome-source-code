using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    [Serializable]
    public class DungeonData : MonoBehaviour
    {
        public int dungeonSetId;
        public string seed;
        public List<int> skeletonRoomIDs;
        public List<int> randomRoomIDs;

        public DungeonData(Dungeon dungeon)
        {
            seed = dungeon.rooms.seed;
            skeletonRoomIDs = new();
            randomRoomIDs = new();

            dungeonSetId = dungeon.info.set._id;

            foreach (var room in dungeon.rooms.skeleton)
            {
                skeletonRoomIDs.Add(room._id);
            }

            foreach (var room in dungeon.rooms.random)
            {
                randomRoomIDs.Add(room._id);
            }
        }
    }
}
