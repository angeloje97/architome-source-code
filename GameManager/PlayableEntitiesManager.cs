using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
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

        void Awake()
        {
            active = this;
            HandleLoadEntities();
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

            var destroyOnLoad = new GameObject("DestroyOnLoad");
            party.transform.SetParent(destroyOnLoad.transform);
        }

        // Update is called once per frame
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
                

                Destroy(party.gameObject);

                party = partyObject.GetComponent<PartyInfo>();


                ArchAction.Delay(() => {
                    foreach (var member in party.GetComponentsInChildren<EntityInfo>())
                    {
                        member.transform.localPosition = new();
                        //member.sceneEvents.OnTransferScene?.Invoke(sceneManager.CurrentScene());
                        GMHelper.GameManager().AddPlayableParty(party);
                    }

                    party.HandleTransferScene(sceneManager.CurrentScene());
                }, .250f);
                

                return;
            }

            partyObject = party.gameObject;
            DontDestroyOnLoad(party);
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

        void GetDependencies()
        {

            saveSystem = SaveSystem.active;
            sceneManager = ArchSceneManager.active;

            if (saveSystem)
            {
                saveSystem.BeforeSave += BeforeSave;
            }

            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += BeforeLoadScene;
            }
        }
        

        void BeforeSave(SaveGame save)
        {
            SaveEntities();
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveEntities();
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
