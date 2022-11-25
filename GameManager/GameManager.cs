using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Architome.Enums;

namespace Architome
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager active;

        public DataMap data;

        [SerializeField] GameState gameState;
        [SerializeField] bool destroyOnLoad;
        public GameState GameState { get { return gameState; } }
        // Start is called before the first frame update
        public List<EntityInfo> playableEntities;
        public List<PartyInfo> playableParties;
        public RaidInfo playableRaid;
        ArchSceneManager sceneManager;
        public GameObject pauseMenu;
        public IGGUIInfo InGameUI;

        //[Header("Managers")]
        //public ContainerTargetables targetManager;

        //[Header("Meta Data")]
        //public KeyBindings keyBinds;
        //public DifficultyModifications difficultyModifications;
        //public World worldSettings;

        public bool isPaused;
        public bool reloadCurrentScene;

        public Action<EntityInfo, int> OnNewPlayableEntity;
        public Action<PartyInfo, int> OnNewPlayableParty;
        public Action<EntityInfo> OnRemoveEntitiy;
        public Action<PartyInfo> OnRemoveParty;


        void Awake()
        {
            HandlePlayableEntities();
            
            if (gameState == GameState.Play)
            {
                if (active != null && active.gameState == GameState.Play)
                {
                    Destroy(gameObject);
                    return;
                }
            }
            //if (gameState == GameState.Play)
            //{
            //    if (active != null&& active.gameState == GameState.Play)
            //    {
            //        Destroy(gameObject);
            //        return;
            //    }

            //    DontDestroyOnLoad(gameObject);

            //}



            data.SetData();

            active = this;
            playableEntities = new List<EntityInfo>();
            isPaused = false;

            Core.Reset();

            if(gameState == GameState.Menu)
            {
                Core.ResetAll();
            }
        }

        private void Start()
        {
            HandleDungeoneer();
            sceneManager = ArchSceneManager.active;
        }

        void HandleDungeoneer()
        {
            if (destroyOnLoad) return;

            if (gameState == GameState.Play)
            {
                ArchGeneric.DontDestroyOnLoad(gameObject);

                var archSceneManager = ArchSceneManager.active;

                archSceneManager.BeforeLoadScene += BeforeLoadScene;
            }

            void BeforeLoadScene(ArchSceneManager manager)
            {
                var validScenes = new HashSet<string>() {
                        "Map Template Continue",
                        "PostDungeonResults",
                    };


                if (!validScenes.Contains(manager.sceneToLoad))
                {
                    ArchGeneric.DestroyOnLoad(gameObject);
                    manager.BeforeLoadScene -= BeforeLoadScene;                    }
            }
        }

        public void LoadScene(string sceneName)
        {
            sceneManager.LoadScene(sceneName, true);
        }

        void HandlePlayableEntities()
        {
            OnNewPlayableEntity += (EntityInfo entity, int index) => {
                entity.infoEvents.OnIsPlayerCheck += HandleIsPlayerCheck;
            };

            OnRemoveEntitiy += (EntityInfo entity) => {
                entity.infoEvents.OnIsPlayerCheck -= HandleIsPlayerCheck;
            };

            void HandleIsPlayerCheck(EntityInfo entity, List<bool> checks)
            {
                checks.Add(true);
            }
        }

        // Update is called once per frame
        void Update()
        {
            ReloadScene();
        }


        public void AddPlayableCharacter(EntityInfo playableChar)
        {
            playableEntities ??= new();
            if (playableEntities.Contains(playableChar)) return;

            playableEntities.Add(playableChar);
            OnNewPlayableEntity?.Invoke(playableChar, playableEntities.IndexOf(playableChar));
        }

        public void AddPlayableParty(PartyInfo playableParty)
        {
            playableParties ??= new();

            playableParties.Add(playableParty);
            OnNewPlayableParty?.Invoke(playableParty, playableParties.IndexOf(playableParty));
        }

        public void RemovePLayableCharacter(EntityInfo playableChar)
        {
            playableEntities ??= new();

            if (!playableEntities.Contains(playableChar)) return;
            playableEntities.Remove(playableChar);
            OnRemoveEntitiy?.Invoke(playableChar);
        }

        public void RemovePlayableParty(PartyInfo party)
        {
            playableParties ??= new();

            if (!playableParties.Contains(party)) return;
            playableParties.Remove(party);
            OnRemoveParty?.Invoke(party);
        }


        public void ReloadScene()
        {
            if (!reloadCurrentScene) return;
            reloadCurrentScene = false;
            
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
            
        }


        private void OnValidate()
        {
        }
    }

}
