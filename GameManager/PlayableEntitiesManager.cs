using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;


namespace Architome
{
    public class PlayableEntitiesManager : MonoBehaviour
    {
        public static PlayableEntitiesManager active;


        public bool loadFromSave;

        public static GameObject partyObject;

        public PartyInfo party;
        public SaveSystem saveSystem;
        public ArchSceneManager sceneManager;

        public List<string> scenesToSaveEntities;

        void Awake()
        {
            active = this;
            HandleLoadEntities();
        }
        void GetDependencies()
        {

            saveSystem = SaveSystem.active;
            sceneManager = ArchSceneManager.active;

            var questManager = QuestManager.active;

            if (questManager)
            {
                questManager.OnQuestEnd += OnQuestEnd;
            }

            if (saveSystem)
            {
                saveSystem.BeforeSave += BeforeSave;
            }

            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += BeforeLoadScene;
                sceneManager.OnLoadScene += OnLoadScene;
            }


        }
        void Start()
        {
            GetDependencies();
            HandleLastLevel();

            OnSceneStart();
            //HandleMoveOutOfPortal();
        }

        void HandleMoveOutOfPortal(PortalInfo portal)
        {
            var entityGenerator = MapEntityGenerator.active;

            if (entityGenerator == null) return;
            if (portal.exitSpot == null) return;

            entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => {
                //ArchAction.Delay(() => party.MovePartyTo(portal.exitSpot.position), 1.5f);
                portal.MoveAllEntitiesOutOfPortal(2);
            };
        }

        void HandleLastLevel()
        {
            if (party == null) return;
            if (!LastLevel()) return;
        }
        void Update()
        {

        }
        public bool LastLevel()
        {
            if (Core.currentDungeon == null) return false;
            if (Core.dungeonIndex >= Core.currentDungeon.Count -1) return false;


            return true;
        }
        async void HandleLoadEntities()
        {
            if (party == null) return;
            if (!loadFromSave) return;

            

            var currentSave = Core.currentSave;

            if (currentSave == null) return;

            if (partyObject != null) return;

            partyObject = party.gameObject;
            //DontDestroyOnLoad(party);
            var savedEntities = currentSave.savedEntities;
            var entityIndex = currentSave.selectedEntitiesIndex;



            var members = party.GetComponentsInChildren<EntityInfo>().ToList();
            Debugger.InConsole(3204, $"{members.Count}");

            for (int i = 0; i < members.Count; i++)
            {
                Destroy(members[i].gameObject);

                members.RemoveAt(i);
                i--;
            }

            var tasks = new List<Task>();

            foreach (var index in entityIndex)
            {
                tasks.Add(EntityDataLoader.SpawnEntity(savedEntities[index], party.transform));
                
            }

            await Task.WhenAll(tasks);

            
        }


        void OnSceneStart()
        {
            ArchAction.Delay(() => {
                TransferUnitsToEntrancePortal();
                StopMovingEntities();
                HandlePortals();
            }, .50f);
        }
        void OnLoadScene(ArchSceneManager sceneManager)
        {
            OnSceneStart();
        }

        void HandlePortals()
        {
            if (Core.currentDungeon == null) return;
            if (Core.currentDungeon.Count == 0) return;
            var mapRoomGenerator = MapRoomGenerator.active;

            mapRoomGenerator.OnRoomsGenerated += (generator) => {
                var portals = PortalInfo.portals;

                foreach (var portal in portals)
                {
                    if (portal.info.portalType != PortalType.NextLevel) continue;
                    portal.events.OnAllPartyMembersInPortal += HandleSceneTransfer;
                }
            };

            void HandleSceneTransfer(PortalInfo info, List<EntityInfo> entities)
            {
                var nextLevel = "Map Template Continue";
                var postDungeon = "PostDungeonResults";
                Core.dungeonIndex++;

                if (Core.dungeonIndex >= Core.currentDungeon.Count)
                {
                    sceneManager.LoadScene(postDungeon, true);
                    return;
                }
                sceneManager.LoadScene(nextLevel, true);
            }
        }
        void OnQuestEnd(Quest quest)
        {
            if (quest.info.state != QuestState.Completed) return;
            
            foreach (var entity in party.members)
            {
                entity.CompleteQuest(quest);
            }
        }
        void StopMovingEntities()
        {
            foreach (var movement in party.GetComponentsInChildren<Movement>())
            {
                movement.StopMoving(true);
            }
        }
        async void TransferUnitsToEntrancePortal()
        {
            var entryPortal = PortalInfo.EntryPortal;
            
            while (entryPortal == null)
            {
                if (PortalInfo.portals == null)
                {
                    await Task.Yield();
                    continue;
                }
                foreach (var portal in PortalInfo.portals)
                {
                    if (portal == null) continue;
                    if (portal.info.portalType == PortalType.Entrance)
                    {
                        entryPortal = portal;
                    }
                }

                await Task.Yield();
            }

            foreach (var entity in party.members)
            {
                entity.Move(entryPortal.portalSpot.position + new Vector3(0, .25f, 0));
                //entity.transform.position = entryPortal.portalSpot.position + new Vector3(0, .25f, 0);
                //entity.infoEvents.OnSignificantMovementChange?.Invoke(entity.transform.position);
                //entity.
            }

            HandleMoveOutOfPortal(entryPortal);

        }
        void BeforeSave(SaveGame save)
        {
            SaveEntities();
        }
        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
        }
        void SaveEntities()
        {
            if (Core.currentSave == null) return;
            if (party == null) return;
            if (!loadFromSave) return;

            if (!LastLevel()) return;


            foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
            {
                Core.currentSave.SaveEntity(entity);
            }
        }

        public List<InventorySlot> AvailableInventorySlots()
        {
            var slots = new List<InventorySlot>();

            var manager = GameManager.active;

            foreach (var entity in manager.playableEntities)
            {
                var inventory = entity.GetComponentInChildren<Inventory>();

                if (inventory == null) continue;
                if (inventory.entityInventoryUI == null) continue;

                foreach (var slot in inventory.entityInventoryUI.inventorySlots)
                {
                    if (slot.currentItemInfo != null) continue;
                    slots.Add(slot);
                }

            }
            return slots;
        }
        
    }
}
