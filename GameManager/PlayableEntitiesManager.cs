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

            ArchAction.Delay(() => {
                TransferUnitsToEntrancePortal();
                StopMovingEntities();
                
            }, .50f);
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

            //var destroyOnLoad = new GameObject("DestroyOnLoad");
            //party.transform.SetParent(destroyOnLoad.transform);
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

            if (partyObject != null)
            {
                

                //Destroy(party.gameObject);

                //party = partyObject.GetComponent<PartyInfo>();


                //ArchAction.Delay(() => {
                //    foreach (var member in party.GetComponentsInChildren<EntityInfo>())
                //    {
                //        member.transform.localPosition = new();
                //        member.sceneEvents.OnTransferScene?.Invoke(sceneManager.CurrentScene());
                //        GMHelper.GameManager().AddPlayableParty(party);
                //    }

                //    party.HandleTransferScene(sceneManager.CurrentScene());
                //}, .250f);
                

                return;
            }

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

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            ArchAction.Delay(() => {
                TransferUnitsToEntrancePortal();
                StopMovingEntities();
            }, .50f);
            

        }
        void OnQuestEnd(Quest quest)
        {
            if (quest.info.state != QuestState.Completed) return;
            
            foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
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
        void TransferUnitsToEntrancePortal()
        {
            var entryPortal = PortalInfo.EntryPortal;

            if (entryPortal == null) return;

            foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
            {
                entity.transform.position = entryPortal.portalSpot.position + new Vector3(0, .25f, 0);
            }

            HandleMoveOutOfPortal(entryPortal);


            //if (portals == null || portals.Count == 0) return;
            //foreach (var portal in portals)
            //{
            //    if (portal == null) continue;
            //    if (!portal.entryPortal) continue;
                
            //    foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
            //    {
            //        entity.transform.position = portal.portalSpot.position + new Vector3(0, .25f, 0);
            //    }

            //    HandleMoveOutOfPortal(portal);
            //    break;
                
            //}
            
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
