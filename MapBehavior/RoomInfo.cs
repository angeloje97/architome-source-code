
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
        public bool ignoreHideOnStart;
        public bool ignoreCheckRoomCollison;
        public bool isRevealed = true;
        public bool spawnedByGenerator = false;
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

            public bool PlayerIsInRoom()
            {
                foreach (var entity in inRoom)
                {
                    if (!Entity.IsPlayer(entity.gameObject)) continue;
                    return true;
                }


                return playerInRoom.Count > 0;
            }

        }
        public Entities entities;

        //Private properties
        private float percentReveal;

        private void Awake()
        {
        }
        void Start()
        {
            //var badSpawn = await CheckBadSpawn();
            GetDependencies();
            entities.room = this;
        }
        void Update()
        {

        }
        public float PercentReveal { get { return percentReveal; } set { percentReveal = value; } }

        protected virtual void GetDependencies()
        {
            mapInfo = MapInfo.active;

            if (mapInfo && !spawnedByGenerator)
            {
                mapInfo.RoomGenerator().roomsInUse.Add(gameObject);
            }

            mapInfo.EntityGenerator().OnEntitiesGenerated += OnEntitiesGenerated;
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

        public void SetPaths(bool open)
        {
            foreach (var path in GetComponentsInChildren<PathInfo>())
            {
                if (path.otherRoom == null) continue;
                path.UpdatePath(open);
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
                if (increments == 0) await Task.Yield();
                else if (count % increments == 0) { await Task.Yield(); }
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
                percentReveal = MapInfo.active.RoomGenerator().roomRevealPercent;
                return true;
            }

            badSpawn = false;

            await Task.Yield();

            if (!CheckAll())
            {
                badSpawn = true;
            }

            //if (!CheckRoomCollision())
            //{
            //    badSpawn = true;
            //}
            //else if (!CheckRoomAbove())
            //{
            //    badSpawn = true;
            //}
            

            if (badSpawn)
            {
                if (!incompatablePaths.Contains(originPath))
                {
                    incompatablePaths.Add(originPath);
                }
                //incompatablePaths.Add(originPath);
                return true;
            }

            return false;

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
        // Update is called once per frame
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

