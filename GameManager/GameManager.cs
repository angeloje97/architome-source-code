using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Architome.Enums;
using UnityEngine.Rendering;

namespace Architome
{
    public class GameManager : MonoBehaviour
    {
        static bool applicationStart = false;
        public static GameManager active;

        public DataMap data;

        [SerializeField] SaveGame currentSave;

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

        public bool isPaused;
        public bool reloadCurrentScene;

        public Action<EntityInfo, int> OnNewPlayableEntity;
        public Action<PartyInfo, int> OnNewPlayableParty;
        public Action<EntityInfo> OnRemoveEntitiy;
        public Action<PartyInfo> OnRemoveParty;


        void Awake()
        {
            HandleApplicationStart();
            HandlePlayableEntities();
            
            if (gameState == GameState.Play)
            {
                if (active != null && active.gameState == GameState.Play)
                {
                    Destroy(gameObject);
                    return;
                }
            }



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

        void HandleApplicationStart()
        {
            if (applicationStart) return;

            Debug.developerConsoleVisible = false;
            DebugManager.instance.enableRuntimeUI = false;
        }


        private void Start()
        {
            sceneManager = ArchSceneManager.active;
            HandleDungeoneer();
            HandleCurrentSave();
        }

        void HandleCurrentSave()
        {
            LoadSave();

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.OnLoadScene, LoadSave, this);
            }

            void LoadSave()
            {
                currentSave = SaveSystem.current;
            }
        }

        void HandleDungeoneer()
        {
            if (destroyOnLoad) return;

            bool setDestroy = false;
            if (gameState == GameState.Play)
            {
                ArchGeneric.DontDestroyOnLoad(gameObject);

                var archSceneManager = ArchSceneManager.active;

                archSceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }

            void BeforeLoadScene(ArchSceneManager manager)
            {
                if (setDestroy) return;
                var validScenes = new HashSet<string>() {
                        "Map Template Continue",
                        "PostDungeonResults",
                    };


                if (!validScenes.Contains(manager.sceneToLoad))
                {
                    ArchGeneric.DestroyOnLoad(gameObject);
                    setDestroy = true;                    }
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
