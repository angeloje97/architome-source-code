using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class GMHelper : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static GameManager GameManager()
    {
        if(GameObject.Find("GameManager") == null) { return null; }
        if(GameObject.Find("GameManager").GetComponent<GameManager>())
        {
            return GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        return null;
    }
    public static ContainerTargetables TargetManager()
    {
        if (GameManager() == null) { return null; }
        if (GameManager().targetManager)
        {
            return GameManager().targetManager;
        }
        return null;
    }
    
    //IGUI Properties
    public static IGGUIInfo IGUI()
    {
        if (GameManager() == null) { return null; }
        if (GameManager() && GameManager().InGameUI) { return GameManager().InGameUI; }
        return null;
    }
    public static ActionBarsInfo ActionBarsInfo()
    {
        if (GameManager() == null) { return null; }
        if (IGUI() && IGUI().ActionBarInfo()) { return IGUI().ActionBarInfo(); }

        return null;
    }
    public static KeyBindings KeyBindings()
    {
        if (GameManager() == null) { return null; }
        if (GameManager().keyBinds)
        {
            return GameManager().keyBinds;
        }
        return null;
    }
    public static DifficultyModifications Difficulty()
    {
        if (GameManager() == null) { return null; }
        if (GameManager() && GameManager().difficultyModifications)
        {
            return GameManager().difficultyModifications;
        }

        return null;
    }
    public static WorldSettings WorldSettings()
    {
        if(GameManager() && GameManager().worldSettings)
        {
            return GameManager().worldSettings;
        }
        return null;
    }
    public static LayerMasksData LayerMasks()
    {
        if(GameManager() && GameManager().layerMasks)
        {
            return GameManager().layerMasks;
        }
        return null;
    }
    public static AudioMixerGroups Mixer()
    {
        if(GameManager())
        {
            foreach(Transform child in GameManager().transform)
            {
                if(child.GetComponent<AudioMixerGroups>())
                {
                    return child.GetComponent<AudioMixerGroups>();
                }
            }
        }

        return null;
    }
    public static EntityDeathHandler EntityDeathHandler()
    {
        if(GameManager() && GameManager().GetComponentInChildren<EntityDeathHandler>())
        {
            return GameManager().GetComponentInChildren<EntityDeathHandler>();
        }

        return null;
    }
    public static WorldInfo WorldInfo()
    {

        if(GameManager() && GameManager().GetComponentInChildren<WorldInfo>())
        {
            return GameManager().GetComponentInChildren<WorldInfo>();
        }
        return null;
    }

    public static WorldActions WorldActions()
    {
        if(GameManager() && GameManager().GetComponentInChildren<WorldActions>())
        {
            return GameManager().GetComponentInChildren<WorldActions>();
        }

        return null;
    }

}
