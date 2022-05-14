using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
namespace Architome
{
    public class GameManager : MonoBehaviour
    {
        //Static variables
        public static GameManager active;

        public DataMap data;


        // Start is called before the first frame update
        public List<EntityInfo> playableEntities;
        public List<PartyInfo> playableParties;
        public RaidInfo playableRaid;

        public GameObject pauseMenu;
        public IGGUIInfo InGameUI;

        [Header("Managers")]
        public ContainerTargetables targetManager;

        [Header("Meta Data")]
        public KeyBindings keyBinds;
        public DifficultyModifications difficultyModifications;
        public World worldSettings;
        public LayerMasksData layerMasks;


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

            if (Core.currentSave == null || Core.currentSave.Length == 0) return;


            SaveGame.current.LoadSave(Core.currentSave);
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
    }

}
