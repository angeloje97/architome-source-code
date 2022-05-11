using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System;
public class GearModuleManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<EntityInfo> playableEntities;
    public List<Image> entityButtons;

    public GearStatsUI stats;
    public GearSlotManager gearSlotManager;
    public Action<EntityInfo> OnSetEntity;

    
    
    void GetDependencies()
    {
        if(GMHelper.GameManager() &&
            GMHelper.GameManager().playableEntities.Count > 0)
        {
            playableEntities = GMHelper.GameManager().playableEntities;
        }
    }
    void Start()
    {
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
        
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        if (index == 0)
        {
            GetComponentInParent<ModuleInfo>().OnSelectEntity?.Invoke(newEntity);
        }


    }

    private void Update()
    {
    }
    // Update is called once per frame


}
