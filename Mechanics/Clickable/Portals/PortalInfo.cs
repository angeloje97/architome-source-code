using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace Architome
{
    public class PortalInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public static List<PortalInfo> portals;
        public ArchSceneManager sceneManager;
        public bool entryPortal;

        public List<PortalInfo> portalList;
        public List<GameObject> entitiesInPortal = new();
        public Transform portalSpot;
        public Transform exitSpot;
        public Clickable clickable;
        public int partyCount;
        public int portalNum;
        public string setScene;

        [Serializable]
        public struct Info
        {
            public string sceneName;
            public RoomInfo room;
            public int portalID;
            public AudioClip portalEnterSound;
            public AudioClip portalExitSound;
        }

        [Serializable]
        public struct Restrictions
        {
            public bool requiresNoHostileEntities;
        }

        [Serializable]
        public struct Connection
        {
            public PortalInfo portal;
            public bool isConnected;
        }

        public List<Connection> connections;

        public Info info;
        public Restrictions restrictions;

        public PortalEvents events;


        void GetDependencies()
        {
            if (GetComponent<Clickable>())
            {
                clickable = GetComponent<Clickable>();
            }

            PortalManager.active.HandleNewPortal(this);

            GMHelper.GameManager().OnNewPlayableEntity += OnNewPlayableEntity;

            sceneManager = ArchSceneManager.active;

            info.room = GetComponentInParent<RoomInfo>();
        }
        

        void Start()
        {
            GetDependencies();
            portalList = portals;
            portalNum = portals.IndexOf(this);
            HandlePortalList();
        }
        private void OnDestroy()
        {
            if (portalList == null) return;

           if (portalList.Contains(this))
            {
                portalList.Remove(this);
            }
            
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

        void HandlePortalList()
        {
            if (portalList == null) portalList = new();

            for (int i = 0; i < portalList.Count; i++)
            {
                if (portalList[i] == null)
                {
                    portalList.RemoveAt(i);
                    i--;
                }
            }

            portalList.Add(this);
        }


        void OnNewPlayableEntity(EntityInfo info, int index)
        {
            partyCount++;
        }

        public void IncreaseDungeonIndex()
        {
            if (Core.currentDungeon == null) return;
            if (Core.currentDungeon.Count == 0) return;

            Core.dungeonIndex++;

            if (Core.dungeonIndex < 0 || Core.dungeonIndex >= Core.currentDungeon.Count)
            {
                setScene = "PostDungeonResults";
                return;
            }

            setScene = "Map Template Continue";
        }

        public void TeleportToScene(string sceneName)
        {
            if (setScene == null || setScene.Length == 0) return;
            sceneManager.LoadScene(sceneName, true);
        }

        public void TeleportToSetScene()
        {
            if (setScene == null || setScene.Length == 0) return;
            sceneManager.LoadScene(setScene, true);
        }


        public void HandleMoveTargets()
        {
            if (clickable == null) { return; }
            if (restrictions.requiresNoHostileEntities)
            {
                foreach (var entity in Entity.EntitiesFromRoom(info.room))
                {
                    if (entity.npcType != Enums.NPCType.Hostile) continue;

                    return;
                }
            }


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

    

    [Serializable]
    public struct PortalEvents
    {
        public Action<PortalInfo, GameObject> OnPortalEnter;
        public Action<PortalInfo, GameObject> OnPortalExit;
        public Action<PortalInfo, List<GameObject>> OnAllPartyMembersInPortal;

        public Action<PortalInfo, GameObject> OnHostilesStillInRoom;

        public UnityEvent OnAllMembersInPortal;
    }
}
