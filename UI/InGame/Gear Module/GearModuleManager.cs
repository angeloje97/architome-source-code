using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
public class GearModuleManager : MonoBehaviour
{
    // Start is called before the first frame update
    public List<EntityInfo> playableEntities;
    public List<Image> entityButtons;

    public GearStatsUI stats;
    public GearSlotManager gearSlotManager;
    
    
    
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
        GetDependencies();
        UpdatePortraits();
        SetEntity(0);
    }

    private void Update()
    {
        HandleNewPlayableEntities();
    }

    void HandleNewPlayableEntities()
    {
        if(GMHelper.GameManager() == null) { return; }
        if(playableEntities != GMHelper.GameManager().playableEntities)
        {
            playableEntities = GMHelper.GameManager().playableEntities;
            UpdatePortraits();
            if(playableEntities.Count > 0)
            {
                SetEntity(0);
            }
        }
    }

    void UpdatePortraits()
    {
        for(int i = 0; i < playableEntities.Count; i++)
        {
            var entity = playableEntities[i];
            if(i < entityButtons.Count && entity.entityPortrait != null)
            {
                entityButtons[i].sprite = entity.entityPortrait;
            }
        }
    }

    // Update is called once per frame

    public void SetEntity(int index)
    {
        if(index >= playableEntities.Count) { return; }

        stats.entityInfo = playableEntities[index];
        gearSlotManager.entityInfo = playableEntities[index];
    }
}
