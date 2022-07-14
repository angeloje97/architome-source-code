using System.Collections;
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


        public RoomInfo SpawnRoom(GameObject room, Transform parent)
        {
            if (!room.GetComponent<RoomInfo>()) { return null; }


            otherRoom = Instantiate(room, roomAnchor.transform.position, roomAnchor.transform.rotation);
            var info = otherRoom.GetComponent<RoomInfo>();

            info.spawnedByGenerator = true;

            otherRoom.transform.parent = parent;
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
                if (!path.GetComponentInParent<PathInfo>()) { continue; }

                if (path.GetComponentInParent<PathInfo>() && this != path.GetComponentInParent<PathInfo>())
                {

                    var tempPath = path.GetComponentInParent<PathInfo>();
                    if (tempPath.transform.position != roomAnchor.transform.position) { continue; }

                    otherPath = tempPath;
                    hasAnotherPath = true;
                    path.GetComponentInParent<PathInfo>().otherPath = this;
                    path.GetComponentInParent<PathInfo>().hasAnotherPath = true;
                    SetOtherPath();
                    otherPath.SetOtherPath();
                    return;
                }
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
