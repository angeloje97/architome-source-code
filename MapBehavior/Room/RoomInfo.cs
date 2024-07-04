
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;
using Architome.Enums;
using PixelCrushers.DialogueSystem.UnityGUI;
using UnityEditor.Build.Reporting;
using PixelCrushers.DialogueSystem;
using Language.Lua;

namespace Architome
{
    [RequireComponent(typeof(RoomInfoTool))]
    public class RoomInfo : MonoActor
    {
        [SerializeField] int id;
        [SerializeField] bool idSet;
        public int _id
        {
            get
            {
                return idSet ? id : 999999999;
            }
            private set
            {
                id = value;
            }
        }

        public void SetId(int value, bool forceId = false)
        {
            if (idSet && !forceId) return;
            idSet = true;
            _id = value;

        }

        public RoomPool pool;
        public RoomType type;
        public string seed;

        [Header("Room Properties")]
        public bool isEntranceRoom;
        public int frequency = 1;
        public bool badSpawn;
        public Transform roomCenter;
        public Transform misc;
        public PathInfo originPath;
        public MapInfo mapInfo;
        public Transform prefab;
        public VectorCluster<Transform> vectorCLuster;



        [Header("Path Properties")]
        public List<PathInfo> paths;
        public List<PathInfo> incompatablePaths;

        //Fields that only the PathInfo class should be using
        //public Transform bin;

        [Serializable]
        public class IncompatablePath
        {
            public PathInfo roomPath;
            public List<PathInfo> otherPaths = new();
        }



        public List<IncompatablePath> incompatables;

        #region RoomSpawnPosition
        
        [SerializeField] protected List<RoomSpawnPositions> roomSpawnPositions;

        protected Dictionary<EntityTier, RoomSpawnPositions> spawnPositionMap;

        protected void UpdateSpawnPosititionMap()
        {
            if (spawnPositionMap != null) return;
            spawnPositionMap = new();

            foreach(var spawnPosition in roomSpawnPositions)
            {
                if (spawnPositionMap.ContainsKey(spawnPosition.entityTier)) continue;
                spawnPositionMap.Add(spawnPosition.entityTier, spawnPosition);
            }
        }

        public virtual RoomSpawnPositions SpawnPositionFromTier(EntityTier tier)
        {
            UpdateSpawnPosititionMap();

            if (tier == EntityTier.Boss) return null;
            if (!spawnPositionMap.ContainsKey(tier)) return null;
            if (spawnPositionMap[tier].parent == null) return null;

            return spawnPositionMap[tier];
        }

        public List<RoomSpawnPositions> EntitySpawnPositions()
        {
            var list = new List<RoomSpawnPositions>();

            var tier1 = SpawnPositionFromTier(EntityTier.Tier1);
            var tier2 = SpawnPositionFromTier(EntityTier.Tier2);
            var tier3 = SpawnPositionFromTier(EntityTier.Tier3);
            var boss = SpawnPositionFromTier(EntityTier.Boss);

            if (tier1 != null) list.Add(tier1);
            if (tier2 != null) list.Add(tier2);
            if (tier3 != null) list.Add(tier3);
            if (boss != null) list.Add(boss);

            return list;
        }

        private void OnValidate()
        {
            UpdateSpawnPositionNames();
            void UpdateSpawnPositionNames()
            {
                if (roomSpawnPositions == null) return;
                foreach(var pos in roomSpawnPositions)
                {
                    pos.name = pos.entityTier.ToString();
                }
            }
        }

        #endregion



        [Header("Spawn Positions")]
        public Transform chestPos;

        [Header("Patrol Properties")]
        public Transform patrolPoints;
        public Transform patrolGroups;

        [Header("RoomBehavior")]
        public List<Transform> allObjects;
        public List<Renderer> allRenderers;
        public bool ignoreHideOnStart;
        public bool ignoreCheckRoomCollison;
        public bool isRevealed = true;
        public bool spawnedByGenerator = false;
        //events

        bool changingVisibility;

        public struct Events
        {
            public Action<RoomInfo, bool> OnShowRoom;
            public Action<RoomInfo, List<Renderer>> OnGetAllRenderers;

            //Entity Events
            public Action<RoomInfo> OnEnterRoom;

        }

        public Events events;

        [Serializable]
        public struct Entities
        {
            public RoomInfo room;
            [SerializeField] List<EntityInfo> inRoom;
            [SerializeField] List<EntityInfo> playerInRoom;
            public bool playerDiscovered;

            public Action<EntityInfo> OnEntityEnter;
            public Action<EntityInfo> OnEntityExit;
            public Action<EntityInfo> OnPlayerEnter;
            public Action<EntityInfo> OnPlayerExit;
            public Action<EntityInfo> OnPlayerDiscover;

            public List<EntityInfo> HostilesInRoom
            { get { return inRoom.Where(entity => entity.npcType == Enums.NPCType.Hostile && entity.isAlive).ToList(); } }

            public List<EntityInfo> EntitiesInRoom
            {
                get
                {
                    return inRoom.ClearNulls();
                }
            }

            public void ClearNullEntities()
            {
                inRoom = inRoom.ClearNulls();
                playerInRoom = playerInRoom.ClearNulls();
            }

            public void HandleEntityEnter(EntityInfo entity)
            {
                if (!room) return;
                if (inRoom.Contains(entity)) return;
                inRoom.Add(entity);
                OnEntityEnter?.Invoke(entity);

                if (entity.rarity == EntityRarity.Player)
                {
                    playerInRoom.Add(entity);
                    OnPlayerEnter?.Invoke(entity);

                    if (!playerDiscovered)
                    {
                        OnPlayerDiscover?.Invoke(entity);
                        playerDiscovered = true;
                    }

                    room.ShowRoom(true, entity.transform.position);
                }
            }
            public void HandleEntityExit(EntityInfo entity)
            {
                if (!inRoom.Contains(entity)) return;
                if (!room) return;
                inRoom.Remove(entity);
                OnEntityExit?.Invoke(entity);

                if (entity.rarity == EntityRarity.Player)
                {
                    playerInRoom.Remove(entity);
                    OnPlayerExit?.Invoke(entity);

                    if (playerInRoom.Count == 0)
                    {
                        room.ShowRoom(false, entity.transform.position);
                    }
                }

                
            }
            public bool PlayerIsInRoom()
            {

                for(int i = 0; i < inRoom.Count; i++)
                {
                    if (inRoom[i] == null)
                    {
                        inRoom.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (!Entity.IsPlayer(inRoom[i])) continue;
                    return true;
                }

                return playerInRoom.Count > 0;
            }
            public void Initiate(MapEntityGenerator generator)
            {
                for (int i = 0; i < inRoom.Count; i++)
                {
                    var entity = inRoom[i];

                    if(entity.currentRoom != room)
                    {
                        inRoom.RemoveAt(i);
                        i--;

                        if (playerInRoom.Contains(entity))
                        {
                            playerInRoom.Remove(entity);
                        }
                    }

                }
            }
        }
        public Entities entities;

        //Private properties
        [SerializeField] float percentReveal;

        void Start()
        {
            //var badSpawn = await CheckBadSpawn();
            GetDependencies();
            

            
        }
        void Update()
        {

        }

        public void SuccesfulStart()
        {
            HandleOnGenerateEntities();
            HandleRoomGeneration();

            void HandleRoomGeneration()
            {
                var roomGenerator = MapRoomGenerator.active;

                roomGenerator.OnRoomsGenerated += OnRoomsGenerated;
            }
        }

        protected virtual void GetDependencies()
        {
            mapInfo = MapInfo.active;

            entities.room = this;

            if (mapInfo && !spawnedByGenerator)
            {
                mapInfo.RoomGenerator().roomsInUse.Add(this);
            }

            mapInfo.EntityGenerator().OnEntitiesGenerated += OnEntitiesGenerated;


            GetAllObjects();
            isRevealed = true;


            if (!spawnedByGenerator)
            {
                SuccesfulStart();
            }
        }
        public void GetAllObjects()
        {

            allObjects = this.transform.GetComponentsInChildren<Transform>().ToList();

            vectorCLuster = new VectorCluster<Transform>(allObjects, this);
        }

        public void GetAllRenderers()
        {
            allRenderers = GetComponentsInChildren<Renderer>().ToList();

        }
        public async void ShowRoom(bool val, Vector3 point = new Vector3(), bool forceShow = false)
        {
            if (!forceShow)
            {
                //if (val != entities.playerInRoom.Count > 0) return;
                if (entities.PlayerIsInRoom() != val) return;
            }

            await ShowRoomAsyncPoint(val, point, percentReveal);
        }

        public async Task VisibilityChanges()
        {
            while (changingVisibility) await Task.Yield();
        }

        public void SetPaths(bool open)
        {
            foreach (var path in GetComponentsInChildren<PathInfo>())
            {
                if (path.otherRoom == null) continue;
                path.UpdatePath(open);
            }
        }

        public void UpdatePaths()
        {
            paths = GetComponentsInChildren<PathInfo>().ToList();
        }

        void HandleOnGenerateEntities()
        {
            var entityGenerator = MapEntityGenerator.active;
            if (entityGenerator == null) return;

            entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => {

            };
        }

        public virtual void OnEntitiesGenerated(MapEntityGenerator generator)
        {
            entities.Initiate(generator);
        }

        public virtual void OnRoomsGenerated(MapRoomGenerator generator)
        {
            Debugger.Environment(5621, $"Room reveal percent is {generator.roomRevealPercent}");
            percentReveal = generator.roomRevealPercent;
        }

        public async Task ShowRoomAsyncPoint(bool val, Vector3 pointPosition, float percent = .025f)
        {
            if (isRevealed == val) return;
            var orderedRenders = new List<Renderer>();

            UpdatePercentReveal();

            GetAllRenderers();
            isRevealed = val;
            changingVisibility = true;
            events.OnShowRoom?.Invoke(this, val);
            if (val)
            {
                orderedRenders = allRenderers.OrderBy(render => V3Helper.Distance(render.gameObject.transform.position, pointPosition)).ToList();
            }
            else
            {
                orderedRenders = allRenderers.OrderByDescending(render => V3Helper.Distance(render.transform.position, pointPosition)).ToList();
            }

            int increments = (int)Mathf.Ceil(orderedRenders.Count * percent);

            if (val)
            {
                SetLights(val);
            }

            var count = 0;
            while (this && count < orderedRenders.Count)
            {

                if (isRevealed != val) { break; }

                if (!orderedRenders[count])
                {
                    count++;
                    break;
                }

                if (orderedRenders[count].enabled == val)
                {
                    count++;
                    continue;
                }

                orderedRenders[count].enabled = val;
                count++;
                if (increments == 0) await Task.Yield();
                else if (count % increments == 0) { await Task.Yield(); }
            }

            if (this == null) return;

            if (!val)
            {
                SetLights(val);
            }

            changingVisibility = false;


            void SetLights(bool val)
            {
                var lights = GetComponentsInChildren<Light>();
                foreach (var light in lights)
                {
                    if (isRevealed != val) { break; }
                    var ignore = light.GetComponent<IgnoreProp>();
                    if (ignore && !ignore.CanSet(val)) continue;

                    light.enabled = val;
                }
            }
        }

        public void UpdatePercentReveal()
        {
            if (percentReveal > 0) return;

            percentReveal = MapRoomGenerator.active.roomRevealPercent;
        }
        public Transform Misc
        {
            get
            {
                if (misc == null)
                {
                    var newObject = new GameObject("Misc");
                    misc = newObject.transform;

                    misc.transform.SetParent(transform);
                }

                return misc;
            }
        }


        public virtual async Task<bool> CheckBadSpawn()
        {
            if (ignoreCheckRoomCollison)
            {
                return true;
            }

            badSpawn = false;

            await Task.Yield();

            if (!CheckAll())
            {
                badSpawn = true;
            }
            

            if (badSpawn)
            {
                if (!incompatablePaths.Contains(originPath))
                {
                    incompatablePaths.Add(originPath);
                }
                return true;
            }
            return false;

            
        }

        bool CheckAll()
        {
            if (roomCenter == null) return true;
            if (isEntranceRoom) return true;
            var groundLayer = LayerMasksData.active.walkableLayer;
            foreach (Transform probe in roomCenter)
            {
                foreach (Transform child in allObjects)
                {
                    var direction1 = V3Helper.Direction(child.position, probe.position);
                    var distance1 = V3Helper.Distance(child.position, probe.position);

                    Ray ray1 = new Ray(probe.position, direction1);

                    if (Physics.Raycast(ray1, out RaycastHit hit1, distance1))
                    {
                        if (allObjects.Contains(hit1.transform)) continue;
                        return false;
                    }
                }

                var direction2 = Vector3.up;
                var distance2 = 35f;

                Ray ray2 = new Ray(probe.position, direction2);

                if (Physics.Raycast(ray2, out RaycastHit hit2, distance2))
                {
                    if (!allObjects.Contains(hit2.transform)) return false;
                }

                var direction3 = Vector3.up;
                var groundPosition = V3Helper.GroundPosition(probe.position, groundLayer, 0, -1);
                var distance3 = 20f;

                var ray3 = new Ray(groundPosition, direction3);

                if (Physics.Raycast(ray3, out RaycastHit hit3, distance3))
                {
                    if (!allObjects.Contains(hit3.transform)) return false;
                }

            }

            return true;
        }

        public PathInfo Entrance
        {

            get 
            { 
                foreach (var path in paths)
                {
                    if (path.isEntrance)
                    {
                        return path;
                    }
                }

                return null; 
            } 
        }



        #region Adjacent Rooms

        public List<RoomInfo> AdjacentRooms()
        {
            var rooms = new List<RoomInfo>();

            foreach(var path in paths)
            {
                if (path.otherRoom)
                {
                    rooms.Add(path.otherRoom.GetComponent<RoomInfo>());
                }
            }
            return rooms;
        }

        public static List<RoomInfo> RoomsBetween2Rooms(RoomInfo roomA, RoomInfo roomB)
        {
            var rooms = new Queue<RoomInfo>();
            var visited = new HashSet<RoomInfo>();
            var predecessor = new Dictionary<RoomInfo, RoomInfo>();
            rooms.Enqueue(roomA);

            while(rooms.Count != 0)
            {
                var currentRoom = rooms.Dequeue();
                var otherRooms = currentRoom.AdjacentRooms();

                foreach(var room in otherRooms)
                {
                    if (visited.Contains(room)) continue;
                    visited.Add(room);
                    predecessor[room] = currentRoom;

                    rooms.Enqueue(room);

                    if (room.Equals(roomB))
                    {
                        var path = new List<RoomInfo>();
                        var step = roomB;

                        while(step != null)
                        {
                            path.Insert(0, step);
                            if (!predecessor.ContainsKey(step))
                            {
                                step = null;
                                continue;
                            }

                            step = predecessor[step];
                        }

                        return path;
                    }

                }


                if (otherRooms.Contains(roomB))
                {
                    rooms.Enqueue(currentRoom);
                    rooms.Enqueue(roomB);
                    break;
                }

                foreach(var room in otherRooms)
                {
                    if (rooms.Contains(room)) continue;
                    rooms.Enqueue(room);
                }
            }

            return new();
        }

        public async void BFSAdjacentRooms(Action<RoomInfo> action, float delay)
        {
            int ms = (int) delay * 1000;

            var queue = new Queue<RoomInfo>();
            var visited = new HashSet<RoomInfo>();

            queue.Enqueue(this);

            while(queue.Count > 0)
            {
                var current = queue.Dequeue();

                await Task.Delay(ms);

                visited.Add(current);
                var otherRooms = AdjacentRooms();

                foreach(var room in otherRooms)
                {
                    if (visited.Contains(room)) continue;
                    queue.Enqueue(room);

                    action(room);
                }

            }

        }

        public async Task RoomsInWaves(Action<List<RoomInfo>> action, float delay)
        {
            var currentWave = new List<RoomInfo> { this };
            var visited = new HashSet<RoomInfo>() { this };

            var ms = (int)delay * 1000;

            while (currentWave.Count > 0)
            {
                action?.Invoke(currentWave);

                await Task.Delay(ms);

                var newWave = new List<RoomInfo>();

                foreach (var room in currentWave)
                {
                    var adjacentRooms = room.AdjacentRooms();

                    foreach (var adjacentRoom in adjacentRooms)
                    {
                        if (visited.Contains(adjacentRoom)) continue;

                        visited.Add(adjacentRoom);
                        newWave.Add(adjacentRoom);
                    }
                }
                currentWave = newWave;
            }
        }

        public async Task SetRevealedInWaves(bool revealed, float delay)
        {
            var center = V3Helper.Average(allObjects);

            await RoomsInWaves((List<RoomInfo> rooms) => {
            foreach(var room in rooms)
            {
                    room.ShowRoom(revealed, center);
            }
            }, delay);
        }

        #endregion
        
    }

    [Serializable] public class RoomSpawnPositions
    {
        [HideInInspector] public string name;
        public EntityTier entityTier;
        public Transform parent;
    }
}

