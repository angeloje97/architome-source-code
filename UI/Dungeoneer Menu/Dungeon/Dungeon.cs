using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class Dungeon : MonoBehaviour
    {
        public DungeonTable.DungeonInfo info;

        [Serializable]
        public struct Rooms
        {
            public string seed;
            public List<RoomInfo> skeleton;
            public List<RoomInfo> random;
        }

        public Rooms rooms;

        public Size DungeonSize()
        {
            var size = Size.Small;

            var sum = rooms.skeleton.Count + rooms.random.Count;

            if (sum > 15)
            {
                size = Size.Medium;
            }

            if (sum > 30)
            {
                size = Size.Large;
            }

            return size;

        }

        public void SetDungeon(DungeonTable.DungeonInfo info)
        {
            this.info = info;

            FillRooms(info);
        }

        public void FillRooms(DungeonTable.DungeonInfo info)
        {

            var sizes = Enum.GetValues(typeof(Size));


            var size = (Size)sizes.GetValue(UnityEngine.Random.Range(0, sizes.Length));

            if (info.allowedSize != null || info.allowedSize.Count > 0)
            {
                while (!info.allowedSize.Contains(size))
                {
                    size = (Size)sizes.GetValue(UnityEngine.Random.Range(0, sizes.Length));
                }
            }

            var (skeletonCount, availableCount) = DungeonRooms(size);

            var skeletonPresets = info.set.rooms.Where(room => room.type == RoomType.Skeleton).ToList();
            var availablePresets = info.set.rooms.Where(room => room.type == RoomType.Random).ToList();


        }

        public (int, int) DungeonRooms(Size size)
        {
            return size switch
            {
                Size.Small => (5, 5),
                Size.Medium => (10, 15),
                Size.Large => (15, 30),
                _ => (5, 5),
            };

        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }


    }
}
