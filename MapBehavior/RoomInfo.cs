using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(RoomInfoTool))]
    public class RoomInfo : MonoBehaviour
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
        [Header("Room Properties")]
        public bool isEntranceRoom;
        public int frequency = 1;
        public bool badSpawn;
        public Transform roomCenter;
        public PathInfo originPath;
        public MapInfo mapInfo;
        public Transform prefab;

        [Header("Path Properties")]
        public List<PathInfo> paths;
        public List<PathInfo> incompatablePaths;
        //public Transform bin;

        [Serializable]
        public class IncompatablePath
        {
            public PathInfo roomPath;
            public List<PathInfo> otherPaths = new();
        }



        public List<IncompatablePath> incompatables;

        [Header("Spawn Positions")]
        public Transform tier1EnemyPos;
        public Transform tier2EnemyPos;
        public Transform tier3EnemyPos;
        public Transform tier1SpawnerPos;
        public Transform tier2SpawnerPos;
        public Transform chestPos;

        [Header("Patrol Properties")]
        public Transform patrolPoints;
        public Transform patrolGroups;

        [Header("RoomBehavior")]
        public Transform[] allObjects;
        public Renderer[] allRenderers;
        public bool hideOnStart;
        public bool ignoreCheckRoomCollison;
        public bool isRevealed = true;

        //events

        public struct Events
        {
            public Action<RoomInfo, bool> OnShowRoom;
        }

        public Events events;

        [Serializable]
        public struct Entities
        {
            public RoomInfo room;
            public List<EntityInfo> inRoom;
            public List<EntityInfo> playerInRoom;
            public bool playerDiscovered;

            public Action<EntityInfo> OnEntityEnter;
            public Action<EntityInfo> OnEntityExit;
            public Action<EntityInfo> OnPlayerEnter;
            public Action<EntityInfo> OnPlayerExit;
            public Action<EntityInfo> OnPlayerDiscover;

            public List<EntityInfo> HostilesInRoom 
            { get { return inRoom.Where(entity => entity.npcType == Enums.NPCType.Hostile && entity.isAlive).ToList(); } }



            public void ClearNullEntities()
            {
                inRoom = inRoom.Where(entity => entity != null).ToList();
            }

            public void HandleEntityEnter(EntityInfo entity)
            {
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

        }
        public Entities entities;

        //Private properties
        private float percentReveal;

        protected virtual void GetDependencies()
        {
            mapInfo = MapInfo.active;
            if (mapInfo)
            {
                mapInfo.RoomGenerator().roomsInUse.Add(gameObject);
                mapInfo.EntityGenerator().OnEntitiesGenerated += OnEntitiesGenerated;
            }

            entities.room = this;


            GetAllObjects();
            isRevealed = true;
        }
        public void GetAllObjects()
        {
            allObjects = this.transform.GetComponentsInChildren<Transform>();
        }

        public void GetAllRenderers()
        {
            allRenderers = GetComponentsInChildren<Renderer>();
        }
        public void ShowRoom(bool val, Vector3 point = new Vector3(), bool forceShow = false)
        {
            if (!forceShow)
            {
                if (val != entities.playerInRoom.Count > 0) return;
            }

            ShowRoomAsyncPoint(val, point, percentReveal);
        }

        public void SetPaths(bool close)
        {
            foreach (var path in GetComponentsInChildren<PathInfo>())
            {
                if (path.otherRoom == null) continue;
                path.SetPath(close);
            }
        }

        public virtual void OnEntitiesGenerated(MapEntityGenerator generator)
        {

        }

        public async void ShowRoomAsyncPoint(bool val, Vector3 pointPosition, float percent = .025f)
        {

            var orderedRenders = new List<Renderer>();

            GetAllRenderers();
            isRevealed = val;
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
            try
            {
                while (count < orderedRenders.Count)
                {
                    if (isRevealed != val) { break; }
                    if (HandleActivator(orderedRenders[count].gameObject))
                    {
                        count++;
                        continue;
                    }

                    if (orderedRenders[count].enabled == val)
                    {
                        count++;
                        continue;
                    }

                    orderedRenders[count].enabled = val;
                    count++;
                    if (count % increments == 0) { await Task.Yield(); }
                }
            }
            catch
            {
                Debugger.InConsole(5439, $"{orderedRenders[count]} bugged out");
            }

            if (!val)
            {
                SetLights(val);
            }


            void SetLights(bool val)
            {
                foreach (var light in GetComponentsInChildren<Light>())
                {
                    if (isRevealed != val) { break; }
                    light.enabled = val;
                }
            }
        }
        bool HandleActivator(GameObject activator)
        {
            if (!activator.GetComponent<WalkThroughActivate>()) { return false; }


            return true;
        }

        private void Awake()
        {
            GetDependencies();
        }
        void Start()
        {
            CheckBadSpawn();
            entities.room = this;
        }

        protected virtual void CheckBadSpawn()
        {
            if (ignoreCheckRoomCollison)
            {
                percentReveal = MapInfo.active.RoomGenerator().roomRevealPercent;
                return;
            }

            badSpawn = false;
            ArchAction.Delay(() => {
                if (!CheckRoomCollision())
                {
                    badSpawn = true;
                }
                else if (!CheckRoomAbove())
                {
                    badSpawn = true;
                }

                if (badSpawn)
                {
                    var incompatable = incompatables.Find(incompatable => incompatable.roomPath == Entrance);

                    if (incompatable == null)
                    {
                        incompatable = new() { roomPath = Entrance };
                        incompatables.Add(incompatable);
                    }

                    incompatable.otherPaths.Add(originPath);

                    incompatablePaths.Add(originPath);
                    mapInfo.RoomGenerator().fixTimer = mapInfo.RoomGenerator().fixTimeFrame;
                    if (mapInfo) { mapInfo.RoomGenerator().badSpawnRooms.Add(gameObject); }
                }

                foreach (PathInfo path in paths)
                {
                    if (!path.isEntrance)
                    {
                        path.isUsed = false;
                    }
                }

                if (!badSpawn)
                {

                    percentReveal = MapInfo.active.RoomGenerator().roomRevealPercent;
                }

            }, .0625f);

            
        }

        // Update is called once per frame
        void Update()
        {

        }
        bool CheckRoomCollision()
        {
            if (roomCenter == null) { return true; }
            if (isEntranceRoom) { return true; }
            foreach (Transform probe in roomCenter)
            {
                foreach (Transform child in allObjects)
                {
                    var direction = V3Helper.Direction(child.position, probe.position);
                    var distance = V3Helper.Distance(child.position, probe.position);

                    Ray ray = new Ray(probe.position, direction);

                    if (Physics.Raycast(ray, out RaycastHit hit, distance))
                    {
                        if (!allObjects.Contains(hit.transform))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
        bool CheckRoomAbove()
        {
            if (roomCenter == null) { return true; }
            if (isEntranceRoom) { return true; }

            foreach (Transform probe in roomCenter)
            {
                var distance = 35f;
                var direction = Vector3.up;
                Ray ray = new Ray(probe.position, direction);
                if (Physics.Raycast(ray, out RaycastHit hit, distance))
                {
                    if (!allObjects.Contains(hit.transform))
                    {
                        return false;
                    }
                }
            }

            return true;

        }

        

        public PathInfo Entrance
        {
            get { return paths.Find(path => path.isEntrance == true); } 
        }
        public void CheckRoomBelow()
        {
            if (isEntranceRoom) { return; }


        }
    }
}

