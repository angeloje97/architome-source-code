using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    public class PortalInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public static List<PortalInfo> portals;
        public ArchSceneManager sceneManager;
        public bool entryPortal;
        public List<PortalInfo> portalList;
        public List<EntityInfo> entitiesInPortal = new();
        
        public Transform portalSpot;
        public Transform exitSpot;
        public Clickable clickable;
        public int partyCount;
        public int portalNum;
        public ArchScene setScene;


        public static PortalInfo EntryPortal { get; set; }

        [Serializable]
        public struct Info
        {
            public string sceneName;
            public PortalType portalType;
            public RoomInfo room;
            public int portalID;
            public AudioClip portalEnterSound;
            public AudioClip portalExitSound;
        }

        [Serializable]
        public struct Restrictions
        {
            public bool requiresNoHostileEntities;
            public bool requiresNoCombat;
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
            portals ??= new();
            if (entryPortal)
            {
                EntryPortal = this;
            }
            

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
            portalList ??= new();

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
            var setScene = ArchScene.PostDungeon;

            HandleDungeonLevels();

            this.setScene = setScene;


            void HandleDungeonLevels()
            {
                if (Core.currentDungeon == null) return;
                if (Core.currentDungeon.Count == 0) return;

                Core.dungeonIndex++;
                if (Core.dungeonIndex >= Core.currentDungeon.Count) return;
                setScene = ArchScene.Dungeon;
            }
        }
        public void TeleportToScene(ArchScene scene)
        {
            sceneManager.LoadScene(scene);
        }
        public void TeleportToSetScene()
        {
            sceneManager.LoadScene(setScene);
        }
        public void HandleMoveTargets()
        {
            if (clickable == null) { return; }
            if (restrictions.requiresNoHostileEntities)
            {
                foreach (var entity in Entity.EntitiesFromRoom(info.room))
                {
                    if (entity.npcType != NPCType.Hostile) continue;
                    
                    return;
                }
            }


            if (clickable.clickedEntities.Count > 0)
            {
                var entities = clickable.clickedEntities;

                foreach(var entity in entities)
                {
                    if (restrictions.requiresNoCombat && entity.isInCombat)
                    {
                        events.OnCantEnterPortal?.Invoke(this, entity, $"{entity} can't enter the portal because they are in combat.");
                        continue;
                    }
                    var movement = entity.Movement();
                   _= movement.MoveToAsync(portalSpot);
                    HandleEntityCombat(entity, movement); 
                }
            }
        }

        async void HandleEntityCombat(EntityInfo entity, Movement movement)
        {
            if (!restrictions.requiresNoCombat) return;

            while (movement.Target() == portalSpot)
            {

                if (entity.isInCombat)
                {
                    movement.StopMoving();
                }

                if (portalSpot == null) return;

                await Task.Yield();
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
        public void MoveAllEntitiesOutOfPortal(float radius)
        {
            if (entitiesInPortal == null) return;
            if (entitiesInPortal.Count == 0) return;
            if (exitSpot == null) return;
            var layerMasks = LayerMasksData.active;
            var obstructionLayer = layerMasks.structureLayerMask;

            var positions = V3Helper.PointsAroundPosition(exitSpot.transform.position, entitiesInPortal.Count, radius, obstructionLayer);

            for (int i = 0; i < entitiesInPortal.Count; i++)
            {
                var movement = entitiesInPortal[i].Movement();
                if (movement == null) continue;

                _= movement.MoveToAsync(positions[i]);
            }
        }
    }

    [Serializable]
    public struct PortalEvents
    {
        public Action<PortalInfo, GameObject> OnPortalEnter;
        public Action<PortalInfo, GameObject> OnPortalExit;
        public Action<PortalInfo, EntityInfo> OnPlayerEnter;
        public Action<PortalInfo, EntityInfo> OnPlayerExit;
        public Action<PortalInfo, EntityInfo, string> OnCantEnterPortal;

        public Action<PortalInfo, List<EntityInfo>> OnAllPartyMembersInPortal { get; set; }
        public Action<PortalInfo, GameObject> OnHostilesStillInRoom;
        public UnityEvent OnAllMembersInPortal;
    }
}
