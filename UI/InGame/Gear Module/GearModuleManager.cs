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
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
        
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        if(entityButtons.Count <= index)
        {
            return;
        }

        playableEntities.Add(newEntity);
        entityButtons[index].transform.parent.gameObject.SetActive(true);

        if(newEntity.entityPortrait)
        {
            
            entityButtons[index].sprite = newEntity.entityPortrait;
        }


        if(index == 0)
        {
            SetEntity(0);
        }

        for (int i = index + 1; i < entityButtons.Count; i++)
        {
            entityButtons[i].transform.parent.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
    }
    // Update is called once per frame

    public void SetEntity(int index)
    {
        if(index >= playableEntities.Count) { return; }

        stats.entityInfo = playableEntities[index];
        gearSlotManager.entityInfo = playableEntities[index];
    }
}
