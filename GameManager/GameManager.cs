using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Architome;
using System;
public class GameManager : MonoBehaviour
{
    //Static variables
    public static GameManager active;

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
    public WorldSettings worldSettings;
    public LayerMasksData layerMasks;


    public bool isPaused;
    public bool reloadCurrentScene;

    public Action<EntityInfo> OnNewPlayableEntity;
    public Action<PartyInfo> OnNewPlayableParty;

    void Awake()
    {
        active = this;
        playableEntities = new List<EntityInfo>();
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleUserInputs();
        ReloadScene();
    }


    public void HandleUserInputs()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(IGGUIOn()){ return; }
        }

        bool IGGUIOn()
        {
            if(InGameUI)
            {
                if(InGameUI.ModulesActive())
                {
                    InGameUI.SetModules(false);
                    return true;
                }
            }

            return false;
        }
    }

    public void AddPlayableCharacter(EntityInfo playableChar)
    {
        if(playableEntities == null) { playableEntities = new List<EntityInfo>(); }

        OnNewPlayableEntity?.Invoke(playableChar);
        playableEntities.Add(playableChar);
    }

    public void AddPlayableParty(PartyInfo playableParty)
    {
        if(playableParties == null) { playableParties = new List<PartyInfo>(); }
        OnNewPlayableParty?.Invoke(playableParty);

        playableParties.Add(playableParty);
    }

    public void ReloadScene()
    {
        if(reloadCurrentScene)
        {
            reloadCurrentScene = false;

            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }
    }
}
