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

            TransferUnitsToEntrancePortal();
            StopMovingEntities();
        }
        void OnLoadScene(ArchSceneManager sceneManager)
        {
            TransferUnitsToEntrancePortal();
            StopMovingEntities();
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
            var portals = PortalInfo.portals;

            ArchAction.Delay(() => {
                if (portals == null || portals.Count == 0) return;
                foreach (var portal in portals)
                {
                    if (portal == null) continue;
                    if (portal.entryPortal)
                    {
                        foreach (var entity in party.GetComponentsInChildren<EntityInfo>())
                        {
                            entity.transform.position = portal.portalSpot.position + new Vector3(0, .25f, 0);
                        }

                        break;
                    }
                }
            }, .125f);
            
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
        
    }
}
