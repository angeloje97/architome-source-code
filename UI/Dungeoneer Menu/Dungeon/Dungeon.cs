using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class Dungeon : MonoBehaviour
    {
        int dungeonSaveIndex = -1;

        public int SaveIndex
        {
            get
            {
                return dungeonSaveIndex;
            }
            set
            {
                dungeonSaveIndex = value;
            }
        }

        [Serializable]
        public struct Rooms
        {
            public List<RoomInfo> skeleton;
            public List<RoomInfo> random;
            public RoomInfo entrance;
            public RoomInfo boss;
            
        }


            
        [Serializable]
        public struct Info
        {
            public Image border;
            public TextMeshProUGUI name, description;
            public Transform entityPortraitParent;
            public List<EntityInfo> entitiesInDungeons;

        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject genericEntityPortrait, specialEntityPortrait, eliteEntityPortrait;
        }

        public Prefabs prefabs;

        public Size size;
        public string seed;
        public DungeonTable.DungeonInfo dungeonInfo;
        public Info info;
        //public Rooms rooms;
        public List<Rooms> levels;
        public bool highlighted;
        public Action<Dungeon> OnSelectDungeon;

        public bool preset;


        public Size DungeonSize()
        {
            var size = Size.Small;

            var sum = levels.Count;

            if (sum > 2)
            {
                size = Size.Medium;
            }

            if (sum > 5)
            {
                size = Size.Large;
            }

            return size;

        }

        public void SetDungeon(DungeonTable.DungeonInfo info) // Fresh Dungeon
        {
            dungeonInfo = info;

            seed = RandomGen.RandomString(10);
            var size = RandomSize();

            for (int i = 0; i < size; i++)
            {
                FillRooms(info);
            }

            UpdateDungeonInfo();
        }

        

        public void SetDungeon(DungeonTable.DungeonInfo info, DungeonData savedDungeon)
        {
            dungeonInfo = info;
            DungeonDataLoader.LoadDungeon(savedDungeon, this);
            UpdateDungeonInfo();
        }

        public void SetDungeon(DungeonTable.DungeonInfo info, DungeonTable.DungeonInfo.PresetDungeons presetDungeon)
        {

            levels = presetDungeon.levels;
            preset = true;
            dungeonInfo = info;
            UpdateDungeonInfo();
        }

        public void SetHighlight(bool highlighted)
        {
            this.highlighted = highlighted;

            if (info.border)
            {
                info.border.enabled = highlighted;
            }
        }

        int RandomSize()
        {
            if (dungeonInfo.allowedSize.Count == 0) return 1;

            var sizes = Enum.GetValues(typeof(Size));

            do
            {
                size = (Size)sizes.GetValue(UnityEngine.Random.Range(0, sizes.Length));
            } while (!dungeonInfo.allowedSize.Contains(size));

            return size switch
            {
                Size.Small => 1,
                Size.Medium => 2,
                Size.Large => 3,
                _ => 1
            };
        }

        public void FillRooms(DungeonTable.DungeonInfo info)
        {
            
            var rooms = new Rooms();




            var (skeletonCount, availableCount) = (5, 5);

            var skeletonPresets = new List<RoomInfo>();
            var availablePresets = new List<RoomInfo>();
            var bossPresets = new List<RoomInfo>();
            var entrancePresets = new List<RoomInfo>();


            foreach (var room in info.set.rooms)
            {
                var list = room.type switch
                {
                    RoomType.Boss => bossPresets,
                    RoomType.Entrance => entrancePresets,
                    RoomType.Skeleton => skeletonPresets,
                    _=> availablePresets,
                };


                for (int i = 0; i < 10; i++)
                {
                    list.Add(room);
                }
            }


            var skeletonIndeces = Enumerable.Range(0, skeletonPresets.Count).ToList();
            var availableIndeces = Enumerable.Range(0, availablePresets.Count).ToList();

            rooms.skeleton = new();

            for (int i = 0; i < skeletonCount - 2; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, skeletonIndeces.Count);

                var skeletonIndex = skeletonIndeces[randomIndex];
                skeletonIndeces.RemoveAt(randomIndex);

                rooms.skeleton.Add(skeletonPresets[skeletonIndex]);
            }

            rooms.random = new();

            for (int i = 0; i < availableCount; i++)
            {
                var randomIndex = UnityEngine.Random.Range(0, availableIndeces.Count);

                var availableIndex = availableIndeces[randomIndex];
                availableIndeces.RemoveAt(randomIndex);

                rooms.random.Add(availablePresets[availableIndex]);
            }

            rooms.boss = bossPresets[UnityEngine.Random.Range(0, bossPresets.Count)];
            rooms.entrance = entrancePresets[UnityEngine.Random.Range(0, entrancePresets.Count)];

            levels.Add(rooms);

        }
        public void SelectDungeon()
        {
            OnSelectDungeon?.Invoke(this);
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

        public int DungeonRoomsCount()
        {
            int total = 0;

            foreach (var rooms in levels)
            {
                total += rooms.skeleton.Count;
                total += rooms.random.Count;

                total += rooms.entrance != null ? 1 : 0;
                total += rooms.boss != null ? 1 : 0;

            }

            return total;

        }
        
        public List<EntityInfo> EntitiesInDungeon()
        {
            var uniqueDungeonPools = new HashSet<RoomPool>();
            var allEntities = new List<EntityInfo>();

            var allRooms = new List<RoomInfo>();//rooms.random.Concat(rooms.skeleton).ToList();
            



            foreach (var level in levels)
            {
                allRooms.Add(level.entrance);
                allRooms.Add(level.boss);

                allRooms = allRooms.Concat(level.random).ToList();
                allRooms = allRooms.Concat(level.skeleton).ToList();

            }

            foreach (var room in allRooms)
            {
                if (uniqueDungeonPools.Contains(room.pool)) continue;
                uniqueDungeonPools.Add(room.pool);
                

                foreach (var field in room.pool.GetType().GetFields())
                {
                    if (field.FieldType != typeof(List<GameObject>)) continue;
                    var entityList = (List<GameObject>)field.GetValue(room.pool);

                    foreach (var entity in entityList)
                    {
                        var info = entity.GetComponent<EntityInfo>();

                        if (info == null) continue;

                        if (allEntities.Contains(info)) continue;

                        allEntities.Add(info);
                    }
                }

                foreach (var patrolGroup in room.pool.patrolGroups)
                {
                    foreach (var member in patrolGroup.entityMembers)
                    {
                        var info = member.GetComponent<EntityInfo>();
                        if (info == null) continue;
                        if (allEntities.Contains(info)) continue;
                        allEntities.Add(info);
                    }
                }
            }

            info.entitiesInDungeons = allEntities;

            return allEntities;
        }

        public void UpdateDungeonInfo()
        {
            info.name.text = dungeonInfo.set.dungeonSetName;
            info.description.text = $"Size: {size} | {DungeonRoomsCount()} Rooms | {EntitiesInDungeon().Count()} Unique Entities | Seed : {seed}";
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
