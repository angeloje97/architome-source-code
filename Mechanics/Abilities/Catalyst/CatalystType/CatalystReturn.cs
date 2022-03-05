using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class CatalystReturn : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;

    public CatalystInfo catalystInfo;
    public CatalystHit catalystHit;

    public bool isReturning;
    public bool hasReturned;

    void GetDependencies()
    {
        if(entityObject == null || entityInfo == null || catalystInfo == null)
        {
            if (GetComponent<CatalystInfo>())
            {
                catalystInfo = GetComponent<CatalystInfo>();
            }

            if(GetComponent<CatalystHit>())
            {
                catalystHit = GetComponent<CatalystHit>();
            }

            if(catalystInfo.entityInfo)
            {
                entityInfo = catalystInfo.entityInfo;
                entityObject = entityInfo.gameObject;
            }
        }
    }
    void Start()
    {
        GetDependencies();
    }

    // Update is called once per frame
    void Update()
    {
        HandleReturn();
        HandleHasReturned();
    }

    public void HandleReturn()
    {
        if(catalystInfo == null || entityObject==null) { return; }
        if(hasReturned) { return; }
        bool cantFindTarget = GetComponent<CatalystBounce>() && GetComponent<CatalystBounce>().cantFindEntity;

        if(!isReturning && (catalystInfo.ticks <= 0 || cantFindTarget))
        {
            isReturning = true;
            catalystInfo.target = entityObject;
        }
    }

    public void HandleHasReturned()
    {
        if(catalystInfo == null || catalystHit == null || entityInfo == null || catalystInfo.abilityInfo == null) { return; }
        if (isReturning && V3Helper.Distance(entityObject.transform.position, transform.position) < 1f)
        {
            hasReturned = true;
            isReturning = false;
            ApplyReturnAction();
        }
    }

    public void ApplyReturnAction()
    {

        if(catalystInfo.abilityInfo.returnAppliesBuffs)
        {
            catalystHit.isAssisting = true;
            catalystHit.canSelfCast = true;
        }

        if(catalystInfo.abilityInfo.returnAppliesHeals)
        {
            catalystHit.canSelfCast = true;
            catalystHit.isHealing = true;
        }

        if(catalystHit.isHealing || catalystHit.isAssisting)
        {
            catalystInfo.ticks++;
        }

        catalystHit.HandleTargetHit(entityInfo);
    }


}
