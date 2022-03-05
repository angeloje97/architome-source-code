using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using Architome;
public class CastBar : MonoBehaviour
{
    // Start is called before the first frame update
    public Image slider;
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;

    public List<GameObject> children;

    public void GetDependencies()
    {   if(abilityManager == null)
        {
            if (GetComponentInParent<GraphicsInfo>())
            {
                if (GetComponentInParent<GraphicsInfo>().entityInfo)
                {
                    entityInfo = GetComponentInParent<GraphicsInfo>().entityInfo;
                    
                    if(entityInfo.AbilityManager())
                    {
                        abilityManager = entityInfo.AbilityManager();
                    }
                }
            }
        }

        if(children.Count == 0)
        {
            foreach(Transform child in transform)
            {
                children.Add(child.gameObject);
            }
        }
        
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        UpdateCastBar();
    }

    void UpdateCastBar()
    {
        if(!abilityManager)
        {
            return;
        }
        if(abilityManager.currentlyCasting && abilityManager.currentlyCasting.isAttack)
        {
            return;
        }
        if(abilityManager.currentlyCasting && abilityManager.currentlyCasting.castTime == 0
            && !abilityManager.currentlyCasting.abilityFunctions.Contains(AbilityFunction.Channel))
        {
            return;
        }

        bool isCasting = abilityManager.currentlyCasting != null;
       

        SetActive(isCasting);


        if(isCasting)
        {
            var abilityInfo = abilityManager.currentlyCasting;

            bool isChanneling = abilityInfo.isChanneling;
            float castTime;
            float castTimer;

            if (isChanneling)
            {
                castTime = abilityInfo.channelTime;
                castTimer = abilityInfo.castTimer;
            }
            else
            {
                castTime = abilityInfo.castTime;
                castTimer = abilityInfo.castTimer;
            }
            

            slider.fillAmount = castTimer / castTime;
        }

    }

    void SetActive(bool status)
    {
        foreach(GameObject child in children)
        {
            if(child.name != "Text")
            {
                child.SetActive(status);
            }
        }
    }
}
