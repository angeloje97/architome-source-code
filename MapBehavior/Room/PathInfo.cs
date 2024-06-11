using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class PathInfo : MonoBehaviour
    {
        public static PathInfo activePath;
        public bool isEntrance;

        bool isChecked;

        public RoomInfo room;
        public PathInfo otherPath;

        public Transform enableOnActive;
        public Transform enableOnClose;

        public WalkThroughActivate pathActivator;
        public GameObject otherRoom { get; set; }
        public GameObject screen;

        public Transform roomAnchor;
        public Transform activeAnchor;
        

        public MapInfo mapInfo;
        public MapAdjustments mapAdjustments;

        public bool isUsed = false;
        public bool hasAnotherPath = false;
        [SerializeField] protected bool neverEntrance;
        protected bool setPath;

        [SerializeField] bool isOpen;
        [SerializeField] bool test;


        void GetDependencies()
        {
            if (MapHelper.MapInfo())
            {
                mapInfo = MapHelper.MapInfo();
                //mapInfo.RoomGenerator().paths.Add(this);
            }

            if (pathActivator && !isUsed)
            {
                pathActivator.deactivatedObjects.Add(screen);
                pathActivator.gameObject.SetActive(false);
            }

            if (transform.parent && GetComponentInParent<RoomInfo>())
            {
                room = GetComponentInParent<RoomInfo>();
            }

            mapAdjustments = MapAdjustments.active;
        }
        void Start()
        {
            GetDependencies();
        }

        void Update()
        {

        }

        private void OnValidate()
        {
            if (!test) return;
            test = false;
            UpdatePath(isOpen);
        }




        async public Task<RoomInfo> SpawnRoom(GameObject room, Transform parent, bool existingRoom = false)
        {
            if (!room.GetComponent<RoomInfo>()) { return null; }

            while (activePath != null)
            {
                await Task.Yield();
            }

            activePath = this;

            var mapRoomGenerator = MapRoomGenerator.active;
            var anchor = mapRoomGenerator.RoomAnchor();
            activeAnchor = anchor;
            mapRoomGenerator.lastActivePath = transform;


            otherRoom = Instantiate(room, roomAnchor.transform.position, roomAnchor.transform.rotation);

            if (existingRoom)
            {
                if (Application.isPlaying)
                {
                    Destroy(room);
                }
            }

            var info = otherRoom.GetComponent<RoomInfo>();
            info.spawnedByGenerator = true;
            info.originPath = this;
            bool badRoom;

            var entrancePathIndex = 0;
            var badPathIndex = new List<int>();

            do
            {
                var paths = otherRoom.GetComponentsInChildren<PathInfo>().ToList();
                var availablePaths = new List<PathInfo>();

                for (int i = 0; i < paths.Count; i++)
                {
                    var badPath = badPathIndex.Contains(i);
                    var neverEntrance = paths[i].neverEntrance;

                    if (badPath || neverEntrance) continue;

                    availablePaths.Add(paths[i]);
                }

                if (availablePaths.Count == 0) break;
                var randomPath = ArchGeneric.RandomItem(availablePaths);
                entrancePathIndex = paths.IndexOf(randomPath);
                randomPath.setPath = true;


                //var anchor = new GameObject($"{otherRoom.name} Anchor");


                anchor.transform.SetPositionAndRotation(randomPath.transform.position, randomPath.transform.rotation);

                otherRoom.transform.SetParent(anchor.transform);

                anchor.transform.SetPositionAndRotation(roomAnchor.transform.position, roomAnchor.transform.rotation);

                otherRoom.transform.SetParent(parent);

                //Destroy(anchor);


                badRoom = await info.CheckBadSpawn();


                if (badRoom)
                {
                    var destroyThis = otherRoom;

                    badPathIndex.Add(paths.IndexOf(randomPath));

                    otherRoom = Instantiate(destroyThis, destroyThis.transform.position, destroyThis.transform.rotation);
                    info = otherRoom.GetComponent<RoomInfo>();
                    info.spawnedByGenerator = true;

                    if (!Application.isPlaying) break;
                    Destroy(destroyThis);

                }
                await Task.Delay((int)(1000 * mapRoomGenerator.fixDelay));


            } while (badRoom);


            var position = otherRoom.transform.position;
            var rotation = otherRoom.transform.rotation;

            var tempRoom = otherRoom;

            otherRoom = Instantiate(tempRoom, position, rotation);
            if (Application.isPlaying)
            {
                Destroy(tempRoom);

            }

            info = otherRoom.GetComponent<RoomInfo>();

            var newRoomPaths = info.GetComponentsInChildren<PathInfo>();

            for (int i = 0; i < newRoomPaths.Length; i++)
            {
                var entrancePath = i == entrancePathIndex;
                newRoomPaths[i].isUsed = entrancePath;


                if (!entrancePath) continue;
            }


            info.spawnedByGenerator = true;

            otherRoom.transform.SetParent(parent);
            info.SuccesfulStart();
            info.originPath = this;
            activePath = null;
            isUsed = true;
            return info;

        }

        public List<PathInfo> OtherRoomPath()
        {
            var paths = new List<PathInfo>();
            if (otherRoom == null) return paths;

            var otherRoomInfo = otherRoom.GetComponent<RoomInfo>();

            foreach(var path in otherRoomInfo.paths)
            {
                if (path == this) continue;
                paths.Add(path);
            }

            return paths;
        }

        public void CheckPath()
        {
            if (isChecked) return;
            Collider[] paths = Physics.OverlapSphere(transform.position, 1f);

            List<Collider> pathList = new List<Collider>(paths);

            foreach (Collider path in pathList)
            {
                var otherInfo = path.GetComponentInParent<PathInfo>();

                if (otherInfo == null || otherInfo == this) continue;
                //if (!path.GetComponentInParent<PathInfo>()) { continue; }

                if (!V3Helper.EqualVector3(roomAnchor.transform.position, otherInfo.transform.position, .25f)) continue;
                //if (!V3Helper.EqualVector3(transform.position, otherInfo.transform.position, .25f)) continue;

                otherPath = otherInfo;
                hasAnotherPath = true;
                otherInfo.otherPath = this;
                otherInfo.hasAnotherPath = true;
                SetOtherPath();
                otherPath.SetOtherPath();
                
                return;
            }

            UpdatePath(false);

        }

        public void UpdatePath(bool isOpen)
        {
            if (enableOnActive)
            {
                foreach (Transform child in enableOnActive)
                {
                    child.gameObject.SetActive(isOpen);
                }
            }

            if (enableOnClose)
            {
                foreach (Transform child in enableOnClose)
                {
                    child.gameObject.SetActive(!isOpen);
                }
            }

            mapAdjustments.AdjustAroundCollider(GetComponentsInChildren<Collider>());
        }

        void SetOtherPath()
        {
            isChecked = true;
            if (!otherPath)
            {
                Debugger.InConsole(8942, $"There is no other path for {this}");
                return;
            }
            //HandleOtherRoom();
            HandleThisPath();

            void HandleThisPath()
            {
                if (otherPath.room)
                {
                    otherRoom = otherPath.room.gameObject;
                }

                if (pathActivator && otherPath.room)
                {
                    pathActivator.gameObject.SetActive(true);
                    
                    pathActivator.activatedObjects.Add(otherPath.room.gameObject);
                }

                UpdatePath(true);
            }
        }

    }

}
