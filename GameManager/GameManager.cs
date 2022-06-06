using System.Collections;
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
            data.SetData();
            active = this;
            playableEntities = new List<EntityInfo>();
            isPaused = false;

            Core.Reset();

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
        }
    }

}
