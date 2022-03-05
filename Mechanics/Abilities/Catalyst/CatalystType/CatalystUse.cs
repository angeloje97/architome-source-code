using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
public class CatalystUse : MonoBehaviour
{
    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;


    public AbilityInfo abilityCheck;

    public Action<CatalystUse> OnAcquireAbility;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        HandleEvents();
    }

    void HandleEvents()
    {
        if(abilityCheck != abilityInfo)
        {
            abilityCheck = abilityInfo;

            if(abilityCheck != null)
            {
                HandleUse();
                OnAcquireAbility?.Invoke(this);
            }
        }
    }

    void HandleUse()
    {
        var catalystHit = GetComponent<CatalystHit>();
        var deathCondition = GetComponent<CatalystDeathCondition>();
        catalystHit.HandleTargetHit(abilityInfo.entityInfo);
        var entities = catalystInfo.EntitiesWithinRadius();
        Debugger.InConsole(1849, $"{entities.Count}");
        foreach(var i in entities)
        {
            if (i != abilityInfo.entityInfo)
            {
                catalystHit.HandleTargetHit(i.GetComponent<EntityInfo>());
            }
        }

        deathCondition.DestroySelf();
    }

}
