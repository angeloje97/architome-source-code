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
                catalystInfo.OnCantFindEntity += OnCantFindEntity;
                catalystInfo.OnTickChange += OnTickChange;
                catalystInfo.OnCloseToTarget += OnCloseToTarget;
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
    }


    public void OnCantFindEntity(CatalystInfo catalyst)
    {
        Return();
    }

    public void OnTickChange(CatalystInfo catalyst, int ticks)
    {
        ArchAction.Yield(() => {
            if (ticks == 0 && !hasReturned)
            {
                Return();
            }
        });
        
    }

    public void Return()
    {
        isReturning = true;
        catalystInfo.target = entityObject;
    }


    public void OnTriggerEnter(Collider other)
    {
        OnPhysicsEnter(other.gameObject);  
    }

    public void OnCollisionEnter(Collision collision)
    {
        OnPhysicsEnter(collision.gameObject);
    }

    public void OnPhysicsEnter(GameObject other)
    {
        if (!isReturning) { return; }

        if(other == entityObject)
        {
            ApplyReturnAction();
        }
    }

    public void ApplyReturnAction()
    {

        hasReturned = true;
        isReturning = false;
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
            catalystInfo.IncreaseTicks();
        }

        catalystHit.HandleTargetHit(entityInfo);
        catalystInfo.OnReturn?.Invoke(catalystInfo);

    }

    public void OnCloseToTarget(CatalystInfo catalystInfo, GameObject target)
    {
        if (!isReturning) return;
        
        if(target == entityObject)
        {
            ApplyReturnAction();
        }
    }


}
