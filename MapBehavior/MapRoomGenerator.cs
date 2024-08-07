using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class MapRoomGenerator : MonoBehaviour
    {
        #region Common Data
        public static MapRoomGenerator active { get; private set; }

        public bool useCoreInfo;

        public MapInfo mapInfo;
        public SeedGenerator seedGenerator;
        public MapEntityGenerator entityGenerator;
        public Transform roomList;
        public int roomsDestroyed;
        public bool generatedRooms;

        [Header("Rooms and Paths")]
        public RoomInfo startingRoom;
        public List<RoomInfo> skeletonRooms;
        public List<RoomInfo> availableRooms;
        public List<RoomInfo> badSpawnRooms;
        public List<RoomInfo> roomsInUse;
        public List<PathInfo> paths;

        public Transform roomAnchor;
        public Transform lastActivePath;


        [Header("Room Generator Properties")]

        [Range(0, 1)]
        public float roomRevealPercent;
        public bool roomsHidden;
        public bool hideRooms;
        public bool generatingSkeleton;
        public bool generatingAvailable;
        public bool ignoreBackgorund;

        public float spawnDelay = 1f;
        public float startDelay = 0f;
        public float fixDelay = 0f;
        public float hideDelay;

        public Action<MapRoomGenerator> OnRoomsGenerated { get; set; }
        public Action<MapRoomGenerator> OnAllRoomsHidden { get; set; }
        public Action<MapRoomGenerator, RoomInfo> OnSpawnRoom;
        public Action<MapRoomGenerator> BeforeEndGeneration { get; set; }
        public Action<MapRoomGenerator> AfterEndGeneration { get; set; }
        public Action<MapRoomGenerator> OnCreateStartingRoom { get; set; }

        public int roomsGenerated;
        public int roomsToGenerate;

        VectorCluster<Transform> roomGeneratorVectorCluster;
        TaskQueueHandler taskHandler;

        #endregion
        
        #region Initialization

        void Start()
        {
            GetDependencies();
            HandleHideRooms();
            HandleCoreDungeons();
            GenerateRooms();
        }
        private void Awake()
        {
            active = this;
        }

        void GetDependencies()
        {
            taskHandler = new(TaskType.Parallel);
            mapInfo = GetComponentInParent<MapInfo>();

            
            if (mapInfo)
            {
                entityGenerator = mapInfo.GetComponentInChildren<MapEntityGenerator>();

            }

            seedGenerator = GetComponentInParent<SeedGenerator>();


            if (startingRoom)
            {
                roomsToGenerate++;
            }


            roomsToGenerate += availableRooms.Count;
            roomsToGenerate += skeletonRooms.Count;
        }

        async void HandleHideRooms()
        {
            if (!hideRooms)
            {
                OnAllRoomsHidden?.Invoke(this);
                return;
            }

            if (entityGenerator)
            {
                taskHandler.AddTask(entityGenerator.UntilEntityGeratorInitialized);
            }

            taskHandler.AddTask(UntilRoomsGenerated);

            await taskHandler.UntilTasksFinished();

            await HideRooms();

            async Task HideRooms()
            {
                ClearNullRooms();
                var tasks = new List<Task>();
                foreach (var info in roomsInUse)
                {
                    if (info == null) continue;
                    if (info.ignoreHideOnStart) continue;
                    if (info.entities.PlayerIsInRoom()) continue;

                    Debugger.System(67960, $"Map Room Generator hiding : {info}");


                    info.ShowRoom(false);
                    tasks.Add(info.VisibilityChanges());
                }

                await Task.WhenAll(tasks);

                roomsHidden = true;
                OnAllRoomsHidden?.Invoke(this);
            }
        }
        async public void GenerateRooms()
        {
            if (mapInfo.generateRooms)
            {
                await Task.Yield();
                UpdateStartingRoom();
                await Task.Delay((int) (startDelay * 1000));

                await Task.Yield();

                if (roomsInUse.Count > 0)
                {
                    await UpdateskeletonRooms();
                    await UpdateAvailableRooms();
                    await Task.Yield();
                }

                await HandleEndGeneration();

                generatedRooms = true;
                OnRoomsGenerated?.Invoke(this);
            }
            else
            {
                await Task.Delay(250);
                generatedRooms = true;
                await HandleEndGeneration();
                OnRoomsGenerated?.Invoke(this);
            }
        }

        void UpdateStartingRoom()
        {
            var room = startingRoom;

            if (room == null)
            {
                if (skeletonRooms.Count > 0)
                {
                    room = skeletonRooms[0];
                    skeletonRooms.RemoveAt(0);
                }
            }

            if (room == null) return;

            var newRoom = Instantiate(room, roomList);

            newRoom.spawnedByGenerator = true;

            newRoom.isEntranceRoom = true;

            AddRoom(newRoom);

            OnCreateStartingRoom?.Invoke(this);
        }

        void HandleCheckPaths()
        {
            ClearNullPaths();
            foreach (PathInfo path in paths)
            {
                path.CheckPath();
            }
        }

        #region Core methods
        void HandleCoreDungeons()
        {
            if (!useCoreInfo) return;

            var levels = Core.currentDungeon;

            if (levels == null) return;

            var dungeonInfo = levels[Core.dungeonIndex];

            if (dungeonInfo.skeleton == null || dungeonInfo.entrance == null) return;

            skeletonRooms = new();
            availableRooms = new();

            roomsToGenerate = 0;

            foreach (var room in dungeonInfo.skeleton)
            {
                skeletonRooms.Add(room);
                roomsToGenerate++;
            }

            if (dungeonInfo.entrance)
            {
                startingRoom = dungeonInfo.entrance;
                roomsToGenerate++;
            }

            if (dungeonInfo.boss)
            {
                skeletonRooms.Add(dungeonInfo.boss);
                roomsToGenerate++;

            }

            foreach (var room in dungeonInfo.random)
            {
                availableRooms.Add(room);
                roomsToGenerate++;

            }


        }
        #endregion

        #endregion

        #region Spawning Rooms
        async Task UpdateskeletonRooms()
        {
            generatingSkeleton = true;

            do
            {
                if (!mapInfo.generateRooms) { break; }

                if (!CanGenerate()) return;
                

                await HandleSekeletonRooms();
                await Task.Delay((int) (spawnDelay * 1000));

            } while (skeletonRooms.Count > 0);

            await Task.Delay((int)(spawnDelay * 1000));
            generatingSkeleton = false;
            


            async Task HandleSekeletonRooms()
            {
                if (roomsInUse.Count == 0 || seedGenerator == null || skeletonRooms.Count == 0)
                {
                    skeletonRooms.Clear();
                    return;
                }

                ClearNullPaths();
                ClearNullRooms();

                var currentCount = roomsInUse.Count;
                var spawnedRoom = false;

                do
                {
                    var availableRandomPaths = ArchGeneric.Shuffle(PreviousPaths(currentCount));

                    if (availableRandomPaths.Count <= 0)
                    {
                        return;
                    }

                    var attempt = 1;

                    foreach (var path in availableRandomPaths)
                    {
                        var skeletonRoom = skeletonRooms[0];
                        var newRoom = await path.SpawnRoom(skeletonRoom, roomList, false);

                        if (newRoom.badSpawn)
                        {
                            newRoom.originPath.isUsed = false;
                            attempt++;
                            DestroyRoom(newRoom);
                            continue;
                        }

                        AddRoom(newRoom);
                        OnSpawnRoom?.Invoke(this, newRoom);
                        spawnedRoom = true;
                        break;
                    }

                } while (currentCount >= 0 && !spawnedRoom);

                skeletonRooms.RemoveAt(0);

                //var pathSeed = UnityEngine.Random.Range(0, availablePaths.Count);
                //var pathSeed = seedGenerator.Factor2(skeletonRooms.Count, availablePaths.Count);
                //Debugger.Environment(9532, $"{pathSeed}");

                //var newRoom = await availablePaths[pathSeed].SpawnRoom(skeletonRooms[0], roomList);
                //skeletonRooms.RemoveAt(0);
                //var badSpawn = newRoom.badSpawn;


                //while (badSpawn)
                //{
                //    var badRoom = newRoom;
                //    ClearNullPaths();
                //    ClearNullRooms();
                //    var avaiablePaths = AvailablePaths(roomsInUse[roomsInUse.Count - 1].GetComponent<RoomInfo>());

                //    ClearIncompatablePaths(badRoom, availablePaths);

                //    if (avaiablePaths.Count == 0)
                //    {
                //        availablePaths = PreviousPaths(roomsInUse.Count - 2);
                //    }

                //    if (availablePaths.Count == 0)
                //    {
                //        DestroyRoom(badRoom);
                //        return;
                //    }

                //    var randomPath = availablePaths[UnityEngine.Random.Range(0, availablePaths.Count)];

                //    newRoom = await randomPath.SpawnRoom(badRoom.gameObject, roomList, true);


                //    badSpawn = newRoom.badSpawn;

                //    if (badSpawn)
                //    {
                //        badRoom.originPath.isUsed = false;
                //    }
                //}

                //AddRoom(newRoom);
                //OnSpawnRoom?.Invoke(this, newRoom);
            }
        }
        async Task UpdateAvailableRooms()
        {
            generatingAvailable = true;
            do
            {
                if (!mapInfo.generateRooms) { break; }

                if (!CanGenerate()) return;


                await HandleAvailableRooms();
                await Task.Delay((int)(spawnDelay * 1000));

            } while (availableRooms.Count > 0);

            generatingAvailable = false;



            //HandleEndGeneration();

            async Task HandleAvailableRooms()
            {
                ClearNullPaths();
                ClearNullRooms();

                if (availableRooms.Count <= 0) return;
                if (AvailablePaths().Count <= 0)
                {
                    availableRooms.Clear();
                    return;
                }
                var availablePaths = AvailablePaths();
                //var randomPathIndex = Random.Range((int)0, (int)AvailablePaths().Count);
                //var seedPathIndex = mapInfo.seedGenerator.factors[AvailablePaths().Count];
                //var seedRoomIndex = mapInfo.seedGenerator.factors[availableRooms.Count];
                //var randomRoomIndex = Random.Range((int)0, (int)availableRooms.Count);
                var seedPathIndex = UnityEngine.Random.Range(0, availablePaths.Count);
                var seedRoomIndex = UnityEngine.Random.Range(0, availableRooms.Count);

                var availableRoom = availableRooms[seedRoomIndex];

                var newRoom = await availablePaths[seedPathIndex].SpawnRoom(availableRoom, roomList);
                var badSpawn = newRoom.badSpawn;
                availableRooms.Remove(availableRooms[seedRoomIndex]);



                while (badSpawn)
                {
                    ClearNullRooms();
                    ClearNullPaths();
                    var badRoom = newRoom;


                    availablePaths = AvailablePaths(badRoom.incompatablePaths);

                    if (availablePaths.Count == 0)
                    {
                        DestroyRoom(badRoom);
                        return;
                    }

                    var randomPath = ArchGeneric.RandomItem(availablePaths);

                    newRoom = await randomPath.SpawnRoom(badRoom, roomList, true);
                    badSpawn = newRoom.badSpawn;
                }

                AddRoom(newRoom);
                OnSpawnRoom?.Invoke(this, newRoom);

            }
        }
        async Task HandleBackgroundAdjustment()
        {
            if (ignoreBackgorund) return;
            var mapAdjustment = GetComponentInParent<MapAdjustments>();
            if (mapAdjustment == null) return;

            await mapAdjustment.AdjustBackground(Cluster());

            VectorCluster<Transform> Cluster()
            {
                var roomObjects = new List<Transform>();
                foreach (var roomInfo in roomList.GetComponentsInChildren<RoomInfo>())
                {
                    foreach (var roomObject in roomInfo.allObjects)
                    {
                        roomObjects.Add(roomObject);
                    }
                }

                roomGeneratorVectorCluster = new VectorCluster<Transform>(roomObjects);

                return roomGeneratorVectorCluster;
            }
        }
        async Task HandleEndGeneration()
        {
            BeforeEndGeneration?.Invoke(this);

            ClearNullRooms();
            HandleCheckPaths();
            AssignRooms();
            AssignPatrolPoints();

            await HandleBackgroundAdjustment();

            AfterEndGeneration?.Invoke(this);

            void AssignRooms()
            {
                roomsInUse = roomsInUse.Distinct().ToList();
                mapInfo.rooms = roomsInUse;
            }

            void AssignPatrolPoints()
            {
                foreach (var i in roomsInUse)
                {
                    if (!i.GetComponent<RoomInfo>()) { continue; }
                    var info = i.GetComponent<RoomInfo>();

                    if (info.patrolPoints != null)
                    {
                        foreach (Transform child in info.patrolPoints.transform)
                        {
                            mapInfo.patrolPoints.Add(child);
                        }
                    }
                }
            }
        }

        #endregion

        #region Misc

        public Transform RoomAnchor()
        {
            if (roomAnchor == null)
            {
                var anchorObject = new GameObject("RoomAnchor");
                roomAnchor = anchorObject.transform;
                roomAnchor.transform.SetParent(transform);
            }

            return roomAnchor;
        }

        public async Task UntilRoomsGenerated()
        {
            await ArchAction.WaitUntil((deltaTime) => generatedRooms, true);
        }
        
        #endregion

        #region Properties

        public List<PathInfo> AvailablePaths()
        {
            ClearNullPaths();
            List<PathInfo> availablePaths = new List<PathInfo>();

            if (paths != null)
            {
                foreach (PathInfo path in paths)
                {
                    if (!path.isUsed)
                    {
                        availablePaths.Add(path);
                    }
                }
            }

            return availablePaths;
        }
        public List<PathInfo> AvailablePaths(List<PathInfo> incompatablePaths)
        {
            List<PathInfo> availablePaths = new List<PathInfo>();

            foreach (PathInfo path in AvailablePaths())
            {
                if (!incompatablePaths.Contains(path))
                {
                    availablePaths.Add(path);
                }
            }

            return availablePaths;
        }
        public List<PathInfo> PreviousPaths(int currentIndex)
        {
            ClearNullRooms();
            if (currentIndex <= 0) { return AvailablePaths(); }
            if (currentIndex >= roomsInUse.Count) { return AvailablePaths(); }

            var availablePaths = AvailablePaths(roomsInUse[currentIndex - 1].GetComponent<RoomInfo>());
            if (availablePaths.Count > 0)
            {
                return availablePaths;
            }
            else
            {
                return PreviousPaths(currentIndex - 1);
            }


        }
        public List<PathInfo> AvailablePaths(RoomInfo room)
        {
            ClearNullPaths();
            var availablePaths = new List<PathInfo>();

            foreach (PathInfo path in room.paths)
            {
                if (!path.isUsed)
                {
                    availablePaths.Add(path);
                }
            }

            return availablePaths;
        }
        public List<PathInfo> RoomPaths(RoomInfo room)
        {
            List<PathInfo> pathList = new List<PathInfo>();

            foreach (PathInfo path in paths)
            {
                if (path.room == room)
                {
                    pathList.Add(path);
                }
            }

            return pathList;
        }
        public bool CanGenerate()
        {
            return Application.isPlaying;
        }
        #endregion

        #region Actions
        public void DestroyRooms()
        {
            if (roomsInUse == null) return;
            for(int i = roomsInUse.Count - 1; i >= 0; i--)
            {
                if (!Application.isPlaying) break;
                Destroy(roomsInUse[i].gameObject);
            }

            foreach(Transform child in roomList)
            {
                Destroy(child.gameObject);
            }

            roomsInUse = new();
        }
        public void AddRoom(RoomInfo room)
        {
            if (paths == null)
            {
                paths = new();
            }

            if (roomsInUse == null)
            {
                roomsInUse = new();
            }

            if (roomsInUse.Contains(room)) return;
            roomsInUse.Add(room);

            roomsGenerated = roomsInUse.Count;

            foreach (var path in room.paths)
            {
                paths.Add(path);
            }
        }
        public void ClearIncompatablePaths(RoomInfo roomInfo, List<PathInfo> availablePaths)
        {
            for (int i = 0; i < availablePaths.Count; i++)
            {
                if (roomInfo.incompatablePaths.Contains(availablePaths[i]))
                {
                    availablePaths.Remove(availablePaths[i]);
                    i--;
                }
            }
        }
        void ClearNullPaths()
        {
            for (int i = 0; i < paths.Count; i++)
            {
                if (paths[i] == null)
                {
                    paths.RemoveAt(i);

                    i--;
                }
            }
        }
        public void ClearNullRooms()
        {
            for (int i = 0; i < roomsInUse.Count; i++)
            {
                if (roomsInUse[i] == null)
                {
                    roomsInUse.RemoveAt(i);
                    i--;
                }
            }
        }

        public void DestroyRoom<T>(T room) where T: MonoBehaviour
        {
            if (Application.isPlaying)
            {
                Destroy(room.gameObject);
            }
            else
            {
                DestroyImmediate(room.gameObject);
            }
        }
        #endregion



    }
}

