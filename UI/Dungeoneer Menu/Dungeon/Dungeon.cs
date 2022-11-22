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
            public int level;
            public string levelName;
            public EntityInfo selectedBoss;
            public List<RoomInfo> skeleton;
            public List<RoomInfo> random;
            public RoomInfo entrance;
            public RoomInfo boss;
            public string levelSeed;
        }


            
        [Serializable]
        public struct Info
        {
            public Image border;
            public TextMeshProUGUI name, description;
            public Transform entityPortraitParent;
            public List<EntityInfo> entitiesInDungeons;
            public List<EntityInfo> bossesInDungeon;

        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject genericEntityPortrait, specialEntityPortrait, eliteEntityPortrait;
        }

        public Prefabs prefabs;

        public Size size;
        public DungeonTable.DungeonInfo dungeonInfo;
        public Info info;
        //public Rooms rooms;
        public List<Rooms> levels;
        public bool highlighted;
        public Action<Dungeon> OnSelectDungeon { get; set; }

        public bool preset;
        public List<EntityInfo> uniqueEntities
        {
            get
            {
                if (info.entitiesInDungeons == null || info.entitiesInDungeons.Count == 0)
                {
                    info.entitiesInDungeons = EntitiesInDungeon();
                }

                return info.entitiesInDungeons;
            }
        }
        public List<EntityInfo> uniqueBosses
        {
            get
            {
                if (info.bossesInDungeon == null)
                {
                    info.bossesInDungeon = Bosses();
                }

                return info.bossesInDungeon;
            }
        }
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

        public void SetDungeon(DungeonTable.DungeonInfo info, int size = 0) // Fresh Dungeon
        {
            dungeonInfo = info;
            var randomBosses = false;


            if (size == 0)
            {
                randomBosses = true;
                size = RandomSize();
            }

            for (int i = 0; i < size; i++)
            {
                FillRooms(info, i, randomBosses);
            }

            UpdateDungeonInfo();
        }

        void Start()
        {

        }
        void Update()
        {

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

        public void FillRooms(DungeonTable.DungeonInfo info, int setIndex, bool randomBoss)
        {
            var originalIndex = setIndex;

            if (setIndex < 0 || setIndex >= info.sets.Count)
            {
                setIndex = info.sets.Count - 1;
            }

            var rooms = new Rooms();

            rooms.levelName = info.sets[setIndex].dungeonSetName;
            rooms.levelSeed = RandomGen.RandomString(10);
            rooms.level = info.sets[setIndex].dungeonLevel;


            var (skeletonCount, availableCount) = (5, 5);

            var skeletonPresets = new List<RoomInfo>();
            var availablePresets = new List<RoomInfo>();
            var bossPresets = new List<RoomInfo>();
            var entrancePresets = new List<RoomInfo>();

            var set = info.sets[setIndex];

            if (setIndex >= 0 && setIndex < info.sets.Count)
            {
                set = info.sets[setIndex];
            }


            foreach (var room in set.rooms)
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

            HandleLevelBoss();

            rooms.entrance = entrancePresets[UnityEngine.Random.Range(0, entrancePresets.Count)];

            levels.Add(rooms);

            void HandleLevelBoss()
            {
                rooms.boss = bossPresets[UnityEngine.Random.Range(0, bossPresets.Count)];

                var bosses = rooms.boss.pool.bossEntities;

                if (randomBoss)
                {
                    rooms.selectedBoss = ArchGeneric.RandomItem(bosses).GetComponent<EntityInfo>();
                }
                else if(originalIndex >= bosses.Count && bosses.Count > 0)
                {
                    rooms.selectedBoss = bosses[bosses.Count - 1].GetComponent<EntityInfo>();
                }
                else
                {
                    rooms.selectedBoss = bosses[originalIndex].GetComponent<EntityInfo>();
                }
            }

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
        
        List<EntityInfo> EntitiesInDungeon()
        {
            var uniqueDungeonPools = new HashSet<RoomPool>();
            var allEntities = new List<EntityInfo>();
            var allRooms = new List<RoomInfo>();//rooms.random.Concat(rooms.skeleton).ToList();
            var bosses = new List<EntityInfo>();
            
            foreach (var level in levels)
            {
                if (!allRooms.Contains(level.entrance))
                {
                    allRooms.Add(level.entrance);
                }

                if (!allRooms.Contains(level.boss))
                {
                    allRooms.Add(level.boss);
                }
                //allRooms.Add(level.entrance);
                //allRooms.Add(level.boss);

                foreach (var room in level.random)
                {
                    if (allRooms.Contains(room)) continue;
                    allRooms.Add(room);
                }

                foreach (var room in level.skeleton)
                {
                    if (allRooms.Contains(room)) continue;
                    allRooms.Add(room);
                }

                //allRooms = allRooms.Concat(level.random).ToList();
                //allRooms = allRooms.Concat(level.skeleton).ToList();

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

                        if (info.rarity == EntityRarity.Boss)
                        {
                            continue;
                        }


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

            //info.entitiesInDungeons = allEntities;

            return allEntities;
        }

        public List<EntityInfo> Bosses()
        {
            
            var bosses = new List<EntityInfo>();

            foreach (var level in levels)
            {
                if (level.selectedBoss == null) continue;
                if (bosses.Contains(level.selectedBoss)) continue;
                bosses.Add(level.selectedBoss);
            }

            return bosses;
        }

        public void UpdateDungeonInfo()
        {
            info.name.text = LevelNames();
            info.description.text = $"Dungeon Level : {RecommendedLevel()}\nFloors: {levels.Count}";
        }

        public float RecommendedLevel()
        {
            float recommendedLevel = 0f;

            for (int i = 0; i < levels.Count; i++)
            {
                var level = levels[i];

                var adjustedLevel = level.level + (2 * i);

                if (recommendedLevel < adjustedLevel)
                {
                    recommendedLevel = adjustedLevel;
                }
            }

            //for (int i = 0; i < levels.Count; i++)
            //{
            //    var levelBoss = levels[i].selectedBoss;
            //    if (levelBoss == null) continue;
            //    var predictedLevel = (i * 2) + levelBoss.entityStats.Level;

            //    if (predictedLevel > recommendedLevel)
            //    {
            //        recommendedLevel = predictedLevel;
            //    }
            //}


            return recommendedLevel;
        }

        string LevelNames()
        {
            var names = new List<string>();
            foreach (var level in levels)
            {
                if (names.Contains(level.levelName)) continue;
                names.Add(level.levelName);
            }

            return ArchString.StringList(names);
        }


        

    }
}
