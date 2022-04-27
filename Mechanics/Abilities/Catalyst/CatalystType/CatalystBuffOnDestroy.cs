using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class CatalystBuffOnDestroy : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo sourceInfo;
    public CatalystInfo catalystInfo;
    public CatalystHit catalystHit;
    public AbilityInfo abilityInfo;

    void GetDependencies()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            abilityInfo = catalystInfo.abilityInfo;
            sourceInfo = catalystInfo.entityInfo;

            catalystInfo.OnCatalystDestroy += OnCatalystDestroy;

        }

        if(GetComponent<CatalystHit>())
        {
            catalystHit = GetComponent<CatalystHit>();
        }
    }

    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnCatalystDestroy(CatalystDeathCondition deathCondition)
    {
        HandleSelfBuff();
    }

    void HandleSelfBuff()
    {
        if(abilityInfo && abilityInfo.buffProperties.selfBuffOnDestroy)
        {
            catalystHit.isAssisting = true;
            catalystHit.canSelfCast = true;
            catalystInfo.IncreaseTicks(false);
            catalystHit.HandleTargetHit(sourceInfo, true);
            catalystHit.canSelfCast = false;
            catalystHit.isAssisting = false;
        }
    }
}
