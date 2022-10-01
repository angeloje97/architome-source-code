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

        }

        private void Start()
        {
            HandleDungeoneer();
        }

        void HandleDungeoneer()
        {
            if (destroyOnLoad) return;

            if (gameState == GameState.Play)
            {
                DontDestroyOnLoad(gameObject);

                var archSceneManager = ArchSceneManager.active;

                archSceneManager.BeforeLoadScene += (ArchSceneManager manager) => {
                    var validScenes = new List<string>() {
                        "Map Template Continue",
                        "PostDungeonResults",
                    };


                    if (!validScenes.Contains(manager.sceneToLoad))
                    {
                        ArchGeneric.DestroyOnLoad(gameObject);
                    }
                };
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
