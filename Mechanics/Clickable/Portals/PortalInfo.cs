using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

namespace Architome
{
    public class PortalInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public static List<PortalInfo> portals;

        public List<PortalInfo> portalList;
        public List<GameObject> entitiesInPortal;
        public Transform portalSpot;
        public Transform exitSpot;
        public Clickable clickable;
        public int portalNum;

        [Serializable]
        public struct Info
        {
            public Scene scene;
            public RoomInfo room;
            public int portalID;
            public AudioClip portalEnterSound;
            public AudioClip portalExitSound;

            
        }

        public struct Connection
        {
            public PortalInfo portal;
            public bool isConnected;
        }

        public List<Connection> connections;

        public Info info;

        public PortalEvents events;

        void GetDependencies()
        {
            if (GetComponent<Clickable>())
            {
                clickable = GetComponent<Clickable>();
            }

            PortalManager.active.HandleNewPortal(this);
        }

        void Start()
        {
            GetDependencies();
            if (portals == null) { portals = new List<PortalInfo>(); }
            portals.Add(this);
            portalList = portals;
            portalNum = portals.IndexOf(this);
        }

        private void OnValidate()
        {
            info.room = GetComponentInParent<RoomInfo>();
        }

        private void Update()
        {

        }


        public void HandleMoveTargets()
        {
            if (clickable == null) { return; }
            if (clickable.clickedEntities.Count > 0)
            {
                var entities = clickable.clickedEntities;

                foreach(var entity in entities)
                {
                    entity.Movement().MoveTo(portalSpot);
                }
            }
        }

    }

    public struct PortalEvents
    {
        public Action<PortalInfo, GameObject> OnPortalEnter;
        public Action<PortalInfo, GameObject> OnPortalExit;
    }
}
