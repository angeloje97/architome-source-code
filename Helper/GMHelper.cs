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

        if (GameObject.Find("GameManager") == null) { return null; }
        if (GameObject.Find("GameManager").GetComponent<GameManager>())
        {
            return GameObject.Find("GameManager").GetComponent<GameManager>();
        }

        return null;
    }
    public static ContainerTargetables TargetManager()
    {
        return ContainerTargetables.active;
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
        return Architome.KeyBindings.active;
    }
    public static DifficultyModifications Difficulty()
    {
        return DifficultyModifications.active;
    }
    public static World WorldSettings()
    {
        return World.active;
    }
    public static LayerMasksData LayerMasks()
    {
        return LayerMasksData.active;
    }
    public static AudioMixerGroups Mixer()
    {
        return AudioMixerGroups.active;

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
