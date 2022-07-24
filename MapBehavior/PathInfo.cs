using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class PathInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public bool isEntrance;

        public RoomInfo room;
        public PathInfo otherPath;

        public GameObject pathBlock;
        public WalkThroughActivate pathActivator;
        public GameObject otherRoom;
        public GameObject screen;

        public Transform roomAnchor;

        public MapInfo mapInfo;

        public bool isUsed = false;
        public bool hasAnotherPath = false;
        protected bool setPath;


        void GetDependencies()
        {
            if (MapHelper.MapInfo())
            {
                mapInfo = MapHelper.MapInfo();
                mapInfo.RoomGenerator().paths.Add(this);
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
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetPath(bool close)
        {
            if (pathBlock == null) return;
            pathBlock.SetActive(close);
        }

        //public void Close()
        //{
        //    if (otherRoom == null) return;
        //    if (pathBlock == null) return;

        //    pathBlock.SetActive(true);
        //}

        //public void Open()
        //{
        //    if (otherRoom == null) return;
        //    if (pathBlock == null) return;

        //    pathBlock.SetActive(false);
        //}


        async public Task<RoomInfo> SpawnRoom(GameObject room, Transform parent, bool existingRoom = false)
        {
            
            if (!room.GetComponent<RoomInfo>()) { return null; }
            



            var otherRoom = Instantiate(room, roomAnchor.transform.position, roomAnchor.transform.rotation);

            if (existingRoom)
            {
                Destroy(room);
            }

            var info = otherRoom.GetComponent<RoomInfo>();
            info.originPath = this;
            bool badRoom;

            var entrancePathIndex = 0;
            var badPathIndex = new List<int>();

            do
            {
                var paths = otherRoom.GetComponentsInChildren<PathInfo>().ToList();
                var availablePaths = new List<PathInfo>();

                for(int i = 0; i < paths.Count; i++)
                {
                    if (badPathIndex.Contains(i)) continue;
                    availablePaths.Add(paths[i]);
                }

                if (availablePaths.Count == 0) break;
                var randomPath = ArchGeneric.RandomItem(paths);
                entrancePathIndex = paths.IndexOf(randomPath);
                randomPath.setPath = true;
                

                var anchor = new GameObject("RoomAnchor");
                anchor.transform.position = randomPath.transform.position;
                anchor.transform.rotation = randomPath.transform.rotation;

                otherRoom.transform.SetParent(anchor.transform);

                anchor.transform.position = roomAnchor.transform.position;
                anchor.transform.rotation = roomAnchor.transform.rotation;

                otherRoom.transform.SetParent(parent);

                Destroy(anchor);

                badRoom = await info.CheckBadSpawn();


                if (badRoom)
                {
                    var destroyThis = otherRoom;

                    badPathIndex.Add(paths.IndexOf(randomPath));

                    otherRoom = Instantiate(destroyThis, destroyThis.transform.position, destroyThis.transform.rotation);
                    info = otherRoom.GetComponent<RoomInfo>();

                    Destroy(destroyThis);

                }


            } while (badRoom);


            var position = otherRoom.transform.position;
            var rotation = otherRoom.transform.rotation;

            var tempRoom = otherRoom;

            otherRoom = Instantiate(tempRoom, position, rotation);
            Destroy(tempRoom);

            info = otherRoom.GetComponent<RoomInfo>();

            var newRoomPaths = info.GetComponentsInChildren<PathInfo>();

            for (int i = 0; i < newRoomPaths.Length; i++)
            {
                var entrancePath = i == entrancePathIndex;
                newRoomPaths[i].isUsed = entrancePath;


                if (!entrancePath) continue;

                //newRoomPaths[i].isUsed = i == entrancePathIndex;


            }


            info.spawnedByGenerator = true;

            otherRoom.transform.SetParent(parent);
            //otherRoom.transform.parent = parent;
            info.originPath = this;

            isUsed = true;
            return info;

        }

        public void CheckPath()
        {
            Collider[] paths = Physics.OverlapSphere(transform.position, 1f);

            List<Collider> pathList = new List<Collider>(paths);

            foreach (Collider path in pathList)
            {
                var otherInfo = path.GetComponentInParent<PathInfo>();

                if (otherInfo == null || otherInfo == this) continue;
                //if (!path.GetComponentInParent<PathInfo>()) { continue; }

                if (!V3Helper.EqualVector3(roomAnchor.transform.position, otherInfo.transform.position, .125f)) continue;
                //if (!V3Helper.EqualVector3(transform.position, otherInfo.transform.position, .25f)) continue;

                otherPath = otherInfo;
                hasAnotherPath = true;
                otherInfo.otherPath = this;
                otherInfo.hasAnotherPath = true;
                SetOtherPath();
                otherPath.SetOtherPath();
                return;
            }
        }

        void SetOtherPath()
        {

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

                if (pathActivator)
                {
                    pathActivator.gameObject.SetActive(true);
                    pathActivator.activatedObjects.Add(otherPath.room.gameObject);
                }

                if (pathBlock)
                {
                    pathBlock.SetActive(false);
                }
            }
        }

    }

}
