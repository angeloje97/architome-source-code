using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(RoomInfoTool))]
    public class RoomInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        

        [Header("Room Properties")]
        public bool isEntranceRoom;
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
            public PathInfo roomPath = new();
            public List<PathInfo> otherPaths = new();
        }

        public List<IncompatablePath> incompatables;

        [Header("Spawn Positions")]
        public Transform tier1EnemyPos;
        public Transform tier2EnemyPos;
        public Transform tier3EnemyPos;
        public Transform bossPos;
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

        public Action<RoomInfo, bool> OnShowRoom;

        //Private properties
        private float percentReveal;

        void GetDependencies()
        {
            if (MapHelper.MapInfo())
            {
                mapInfo = MapHelper.MapInfo();
                mapInfo.RoomGenerator().roomsInUse.Add(gameObject);
            }


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
            if (forceShow)
            {
                if (Entity.PlayerIsInRoom(this) != val) return;
            }

            ShowRoomAsyncPoint(val, point, percentReveal);
        }

        public void ClosePaths()
        {
            foreach (var path in GetComponentsInChildren<PathInfo>())
            {
                path.Close();
            }
        }

        public void OpenPaths()
        {
            foreach (var path in GetComponentsInChildren<PathInfo>())
            {
                path.Open();
            }
        }

        public async void ShowRoomAsyncPoint(bool val, Vector3 pointPosition, float percent = .025f)
        {

            var orderedRenders = new List<Renderer>();

            GetAllRenderers();
            isRevealed = val;
            OnShowRoom?.Invoke(this, val);
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

        void Start()
        {
            GetDependencies();

            if (!ignoreCheckRoomCollison)
            {
                badSpawn = false;
                ArchAction.Delay(() => {
                    if (!CheckRoomCollision())
                    {
                        badSpawn = true;
                    }
                    else if(!CheckRoomAbove())
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
                        if (mapInfo) { mapInfo.RoomGenerator().badSpawnRooms.Add(gameObject); }
                    }

                    foreach (PathInfo path in paths)
                    {
                        if (!path.isEntrance)
                        {
                            path.isUsed = false;
                        }
                    }

                    //if (!CheckRoomBelow())
                    //{

                    //}
                }, .0625f);
            }
            

            if (!badSpawn)
            {
                percentReveal = MapInfo.active.RoomGenerator().roomRevealPercent;
            }

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

        public static RoomInfo GetRoom(Vector3 point)
        {
            List<Ray> rays = new();

            rays.Add(new Ray(point, Vector3.down));
            rays.Add(new Ray(point, Vector3.left));
            rays.Add(new Ray(point, Vector3.right));
            rays.Add(new Ray(point, Vector3.forward));
            rays.Add(new Ray(point, Vector3.back));


            foreach (var ray in rays)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GMHelper.LayerMasks().structureLayerMask))
                {
                    if (hit.transform.GetComponentInParent<RoomInfo>())
                    {
                        return hit.transform.GetComponentInParent<RoomInfo>();
                    }
                }
            }

            return null;

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

