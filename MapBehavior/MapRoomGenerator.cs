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
        // Start is called before the first frame update
        public static MapRoomGenerator active { get; private set; }

        public bool useCoreInfo;

        public MapInfo mapInfo;
        public SeedGenerator seedGenerator;
        public MapEntityGenerator entityGenerator;
        public Transform roomList;
        public int roomsDestroyed;
        public bool generatedRooms;

        [Header("Rooms and Paths")]
        public GameObject startingRoom;
        public List<GameObject> skeletonRooms;
        public List<GameObject> availableRooms;
        public List<GameObject> badSpawnRooms;
        public List<GameObject> roomsInUse;
        public List<PathInfo> paths;

        [Header("Room Generator Properties")]

        [Range(0, 1)]
        public float roomRevealPercent;
        public bool roomsHidden;
        public bool hideRooms;
        public bool generatingSkeleton;
        public bool generatingAvailable;

        public float spawnDelay = 1f;
        public float hideDelay;
        public float fixTimer;
        public float fixTimeFrame;

        //Events
        public Action<MapRoomGenerator> OnRoomsGenerated;


        void GetDependencies()
        {
            if (GetComponentInParent<MapInfo>())
            {
                mapInfo = GetComponentInParent<MapInfo>();
                if (mapInfo.GetComponentInChildren<MapEntityGenerator>())
                {
                    entityGenerator = mapInfo.GetComponentInChildren<MapEntityGenerator>();

                    entityGenerator.OnEntitiesGenerated += OnEntitiesGenerated;
                }
            }

            if (GetComponentInParent<SeedGenerator>())
            {
                seedGenerator = GetComponentInParent<SeedGenerator>();
            }


        }

        async void GenerateRooms()
        {
            if (mapInfo.generateRooms)
            {
                UpdateStartingRoom();
                await Task.Delay(1000);
                StartCoroutine(ClearNullsRoutine());
                await UpdateskeletonRooms();
                await UpdateAvailableRooms();
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

        void HandleCoreDungeons()
        {
            if (!useCoreInfo) return;
            
            var levels = Core.currentDungeon;

            if (levels == null) return;

            var dungeonInfo = levels[Core.dungeonIndex];

            if (dungeonInfo.skeleton == null || dungeonInfo.entrance == null) return;

            skeletonRooms = new();
            availableRooms = new();

            foreach (var room in dungeonInfo.skeleton)
            {
                skeletonRooms.Add(room.gameObject);
            }

            if (dungeonInfo.entrance)
            {
                startingRoom = dungeonInfo.entrance.gameObject;
            }

            if (dungeonInfo.boss)
            {
                skeletonRooms.Add(dungeonInfo.boss.gameObject);
            }

            foreach (var room in dungeonInfo.random)
            {
                availableRooms.Add(room.gameObject);
            }
        }
        void Start()
        {
            GetDependencies();
            HandleCoreDungeons();
            GenerateRooms();
        }


        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {
            HandleTimers();
            if (badSpawnRooms.Count > 0 && mapInfo.generateRooms)
            {
                fixTimer = fixTimeFrame;
                HandleBadSpawnRooms();
            }
        }

        void UpdateStartingRoom()
        {
            if (startingRoom == null) return;
            Instantiate(startingRoom, roomList);
        }
        async Task UpdateAvailableRooms()
        {
            generatingAvailable = true;
            do
            {
                if (!mapInfo.generateRooms) { break; }
                HandleAvailableRooms();

                await Task.Delay((int)(spawnDelay * 1000));
            } while (availableRooms.Count > 0 || fixTimer > 0);

            generatingAvailable = false;



            //HandleEndGeneration();

            void HandleAvailableRooms()
            {
                if (fixTimer > 0) { return; }
                if (availableRooms.Count > 0 && AvailablePaths().Count > 0)
                {
                    var availablePaths = AvailablePaths();
                    //var randomPathIndex = Random.Range((int)0, (int)AvailablePaths().Count);
                    //var seedPathIndex = mapInfo.seedGenerator.factors[AvailablePaths().Count];
                    //var seedRoomIndex = mapInfo.seedGenerator.factors[availableRooms.Count];
                    //var randomRoomIndex = Random.Range((int)0, (int)availableRooms.Count);
                    var seedPathIndex = UnityEngine.Random.Range(0, availablePaths.Count);
                    var seedRoomIndex = UnityEngine.Random.Range(0, availableRooms.Count);


                    availablePaths[seedPathIndex].SpawnRoom(availableRooms[seedRoomIndex], roomList);
                    availableRooms.Remove(availableRooms[seedRoomIndex]);
                    return;

                }
            }
        }
        async Task UpdateskeletonRooms()
        {
            generatingSkeleton = true;

            do
            {
                if (!mapInfo.generateRooms) { break; }

                HandleSekeletonRooms();
                generatingSkeleton = true;
                await Task.Delay((int) (spawnDelay * 1000));

            } while (skeletonRooms.Count > 0 || fixTimer > 0);

            await Task.Delay((int)(spawnDelay * 1000));
            generatingSkeleton = false;
            


            void HandleSekeletonRooms()
            {
                if (fixTimer > 0 || roomsInUse.Count == 0 || seedGenerator == null || skeletonRooms.Count == 0) { return; }

                var lastRoom = roomsInUse[roomsInUse.Count - 1].GetComponent<RoomInfo>();
                var availablePaths = AvailablePaths(lastRoom);

                if (availablePaths.Count == 0)
                {
                    availablePaths = PreviousPaths(roomsInUse.Count - 1);
                    if (availablePaths.Count == 0)
                    {
                        skeletonRooms.Clear();
                    }
                }

                var pathSeed = UnityEngine.Random.Range(0, availablePaths.Count);
                //var pathSeed = seedGenerator.Factor2(skeletonRooms.Count, availablePaths.Count);
                Debugger.InConsole(9532, $"{pathSeed}");


                availablePaths[pathSeed].SpawnRoom(skeletonRooms[0], roomList);
                skeletonRooms.RemoveAt(0);
            }
        }
        async Task HandleBackgroundAdjustment()
        {
            if (GetComponentInParent<MapAdjustments>() == null) { return; }

            var mapAdjustment = GetComponentInParent<MapAdjustments>();

            var midPoint = MapMidPoint();
            var size = MapSize(1);

            await mapAdjustment.AdjustBackground(midPoint, size);


            Vector3 MapMidPoint()
            {
                var roomObjects = new List<Transform>();


                foreach (var roomInfo in roomList.GetComponentsInChildren<RoomInfo>())
                {
                    foreach (var roomObject in roomInfo.allObjects)
                    {
                        roomObjects.Add(roomObject);
                    }

                }

                return V3Helper.MidPoint(roomObjects);
            }
            Vector3 MapSize(int height)
            {
                var rooms = new List<Transform>();


                foreach (var room in roomList.GetComponentsInChildren<RoomInfo>())
                {
                    foreach (var roomObject in room.allObjects)
                    {
                        rooms.Add(roomObject);
                    }
                }

                Debugger.InConsole(54892, $"The dimensions are {V3Helper.Dimensions(rooms)}");

                return V3Helper.Dimensions(rooms);
            }
        }
        async Task HandleEndGeneration()
        {
            HandleCheckPaths();
            await HandleBackgroundAdjustment();
            AssignRooms();
            AssignPatrolPoints();

            


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
        void HandleBadSpawnRooms()
        {
            if (badSpawnRooms.Count > 0)
            {
                ClearNullRooms();
                ClearNullPaths();

                for (int i = 0; i < badSpawnRooms.Count; i++)
                {
                    var room = badSpawnRooms[i];


                    foreach (PathInfo path in RoomPaths(room.GetComponent<RoomInfo>()))
                    {
                        path.isUsed = true;
                    }

                    if (!HandleBadSpawnSkeleton(room)) { Destroy(room); roomsDestroyed++; }
                    else if (!HandleBadSpawnAvailable(room)) { Destroy(room); roomsDestroyed++; }

                    badSpawnRooms.RemoveAt(i);
                    i--;
                }
            }

            bool HandleBadSpawnSkeleton(GameObject room)
            {
                if (!generatingSkeleton) { return true; }
                var availablePaths = AvailablePaths(roomsInUse[roomsInUse.Count - 2].GetComponent<RoomInfo>());
                ClearIncompatablePaths(room.GetComponent<RoomInfo>(), availablePaths);

                if (availablePaths.Count == 0)
                {
                    availablePaths = PreviousPaths(roomsInUse.Count - 2);

                    if (availablePaths.Count == 0)
                    {
                        return false;
                    }
                }

                var pathIndexSeed = seedGenerator.Factor2(roomsInUse.Count, availablePaths.Count);


                var newRoom = availablePaths[pathIndexSeed].SpawnRoom(room, roomList);

                if (room && room.GetComponent<RoomInfo>() && room.GetComponent<RoomInfo>().originPath)
                {
                    room.GetComponent<RoomInfo>().originPath.isUsed = false;
                }

                foreach (PathInfo path in newRoom.paths)
                {
                    if (!path.isEntrance)
                    {
                        path.isUsed = false;
                    }
                }

                Destroy(room);

                return true;

            }

            bool HandleBadSpawnAvailable(GameObject room)
            {
                if (!generatingAvailable) { return true; }

                var availablePaths = AvailablePaths(room.GetComponent<RoomInfo>().incompatablePaths);

                if (availablePaths.Count == 0) { return false; }

                //int randomPathIndex = seedGenerator.factors[availablePaths.Count];
                int randomPathIndex = UnityEngine.Random.Range(0, availablePaths.Count);


                var roomInfo = availablePaths[randomPathIndex].SpawnRoom(room, roomList);

                if (room && room.GetComponent<RoomInfo>() && room.GetComponent<RoomInfo>().originPath)
                {
                    room.GetComponent<RoomInfo>().originPath.isUsed = false;
                }

                foreach (PathInfo path in roomInfo.paths)
                {
                    if (!path.isEntrance)
                    {
                        path.isUsed = false;
                    }
                }

                Destroy(room);

                return true;
            }
        }
        IEnumerator ClearNullsRoutine()
        {
            do
            {
                yield return new WaitForSeconds(spawnDelay / 2);
                ClearNullPaths();
                ClearNullRooms();

            } while (generatingSkeleton || generatingAvailable);
        }
        public void HandleTimers()
        {
            if (fixTimer > 0 && badSpawnRooms.Count == 0)
            {
                fixTimer -= Time.deltaTime;
            }
            if (fixTimer < 0)
            {
                fixTimer = 0;
            }

        }
        void HandleHideRooms()
        {
            if (!hideRooms) { return; }
            if (!roomsHidden)
            {
                if (availableRooms.Count == 0 && badSpawnRooms.Count == 0)
                {
                    StartCoroutine(HideRooms());
                    roomsHidden = true;
                }
            }

            IEnumerator HideRooms()
            {
                yield return new WaitForSeconds(hideDelay);
                ClearNullRooms();
                foreach (GameObject room in roomsInUse)
                {
                    if (room.GetComponent<RoomInfo>() && room.GetComponent<RoomInfo>().hideOnStart)
                    {
                        room.GetComponent<RoomInfo>().ShowRoom(false);
                    }
                }
            }
        }
        void HandleCheckPaths()
        {
            ClearNullPaths();
            foreach (PathInfo path in paths)
            {
                path.CheckPath();
            }


        }
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

            if (currentIndex == 0) { return AvailablePaths(); }
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
        public List<PathInfo> AvailablePaths(RoomInfo room)
        {
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

        //Event Handlers
        public void OnEntitiesGenerated(MapEntityGenerator entityGenerator)
        {
            HandleHideRooms();
        }

    }
}

