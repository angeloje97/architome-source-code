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
            HandleDungeons();
        }

        void HandleDungeons()
        {
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
            if (playableEntities == null) { playableEntities = new List<EntityInfo>(); }

            playableEntities.Add(playableChar);
            OnNewPlayableEntity?.Invoke(playableChar, playableEntities.IndexOf(playableChar));
        }

        public void AddPlayableParty(PartyInfo playableParty)
        {
            if (playableParties == null) { playableParties = new List<PartyInfo>(); }

            playableParties.Add(playableParty);
            OnNewPlayableParty?.Invoke(playableParty, playableParties.IndexOf(playableParty));
        }

        public void ReloadScene()
        {
            if (reloadCurrentScene)
            {
                reloadCurrentScene = false;

                Scene scene = SceneManager.GetActiveScene();
                SceneManager.LoadScene(scene.name);
            }
        }


        private void OnValidate()
        {
            //Stats stats = new()
            //{
            //    Wisdom = 5,
            //    Strength = 3,
            //    attackSpeed = .25f,
            //    attackDamage = 10f,
            //};

            //var percentageFields = Stats.PercentageFields;
            //foreach (var attribute in stats.Attributes())
            //{
            //    if (percentageFields.Contains(attribute.Name))
            //    {
            //        Debugger.InConsole(4392, $"{attribute.Name} : {((float)attribute.Data) * 100}%");
            //    }
            //    else
            //    {
            //        Debugger.InConsole(4328, $"{attribute.Name}: {attribute.Value}");
            //    }
            //}
        }
    }

}
