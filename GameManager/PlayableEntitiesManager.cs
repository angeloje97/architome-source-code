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


        public bool loadFromSave { get; set; }

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
                saveSystem.AddListener(SaveEvent.BeforeSave, BeforeSave, this);
            }

            if (sceneManager)
            {


                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
                sceneManager.AddListener(SceneEvent.OnLoadScene, OnLoadScene, this);

            }
        }
        void Start()
        {
            GetDependencies();
            HandleLastLevel();

            OnSceneStart();
            //HandleMoveOutOfPortal();
        }
        void Update()
        {

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
        public bool LastLevel()
        {
            Debugger.Environment(5532, $"Dungeon {Core.currentDungeon}");
            if (Core.currentDungeon == null) return true;
            Debugger.Environment(5532, $"Dungeon Index : {Core.dungeonIndex}, Dungeon Count {Core.currentDungeon.Count}");

            if (Core.dungeonIndex >= Core.currentDungeon.Count -1) return true;


            return false;
        }
        async void HandleLoadEntities()
        {
            if (party == null) return;
            if (!loadFromSave) return;

            

            var currentSave = SaveSystem.current;

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
                if (this == null) return;
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

            var nextLevel = "Map Template Continue";
            var postDungeon = "PostDungeonResults";
            var promptHandler = PromptHandler.active;

            mapRoomGenerator.OnRoomsGenerated += (generator) => {
                var portals = PortalInfo.portals;
                foreach (var portal in portals)
                {

                    if(portal.info.portalType == PortalType.NextLevel)
                    {
                        portal.events.OnAllPartyMembersInPortal += HandleNextLevelTransfer;

                    }

                    if(portal.info.portalType == PortalType.Entrance)
                    {
                        portal.events.OnAllPartyMembersInPortal += HandleEntrancePortalTransfer;
                    }
                }
            };
            

            async void HandleNextLevelTransfer(PortalInfo info, List<EntityInfo> entities)
            {

                var options = new List<OptionData>();

                if (!LastLevel())
                {
                    options.Add(new("Next Floor", (OptionData data) => {
                        Core.dungeonIndex++;
                        sceneManager.LoadScene(nextLevel, true);
                    }));
                }

                options.Add(new("Leave Dungeon", (OptionData data) => {
                    sceneManager.LoadScene(postDungeon);
                }));

                options.Add(new("Cancel", (OptionData data) =>
                {
                    info.MoveAllEntitiesOutOfPortal(2f);
                })
                { isEscape = true });

                Debugger.UI(5533, $"Options {options.Count}");

                await promptHandler.GeneralPrompt(new()
                {
                    title = "Exit Portal",
                    question = "All party members have entered the portal. Select a destination to travel to.",
                    options = options,
                    blocksScreen = true,
                });

            }

            async void HandleEntrancePortalTransfer(PortalInfo info, List<EntityInfo> entities)
            {

                await promptHandler.GeneralPrompt(new()
                {
                    title = "Entrance Portal",
                    question = "Are you sure you want to leave the dungeon?",
                    options = new()
                    {
                        new("Leave Dungeon", (OptionData data)=> { sceneManager.LoadScene(postDungeon); }),
                        new("Cancel", (OptionData data) => { info.MoveAllEntitiesOutOfPortal(2f); }) { isEscape = true}
                    },
                    blocksScreen = true,
                });
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
            if (this == null) return;
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
            }

            HandleMoveOutOfPortal(entryPortal);

        }
        void BeforeSave(SaveSystem system, SaveGame save)
        {
            SaveEntities();
        }
        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveEntities();
        }
        void SaveEntities()
        {
            var currentSave = SaveSystem.current;
            if (currentSave == null) return;
            if (party == null) return;
            if (!loadFromSave) return;


            foreach(var member in party.members)
            {
                currentSave.SaveEntity(member);
            }


            //foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
            //{
            //    Core.currentSave.SaveEntity(entity);
            //}
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
