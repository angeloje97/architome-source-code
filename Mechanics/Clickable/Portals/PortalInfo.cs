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
        public List<GameObject> entitiesInPortal = new();
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
            portalList = portals;
            portalNum = portals.IndexOf(this);
        }
        private void Awake()
        {
            if (portals == null) portals = new();
            portals.Add(this);
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
        public void SpawnEntity(GameObject entity)
        {
            if (entity.GetComponent<EntityInfo>() == null) return;
        }

        public void SpawnParty(GameObject party)
        {
            if (party.GetComponent<PartyInfo>() == null) return;

            var spawnedParty = Instantiate(party, portalSpot.transform.position, new Quaternion());

            foreach (var entity in spawnedParty.GetComponentsInChildren<EntityInfo>())
            {
                entity.transform.position = portalSpot.transform.position;
            }
        }
    }

    

    public struct PortalEvents
    {
        public Action<PortalInfo, GameObject> OnPortalEnter;
        public Action<PortalInfo, GameObject> OnPortalExit;
    }
}
