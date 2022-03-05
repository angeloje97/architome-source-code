using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
public class CatalystDeathCondition : MonoBehaviour
{
    // Start is called before the first frame update

    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;

    public AbilityInfo.DestroyConditions conditions;


    public float destroyDelay = 0f;

    public void GetDependencies()
    {
        if(catalystInfo == null)
        {
            if(gameObject.GetComponent<CatalystInfo>())
            {
                catalystInfo = gameObject.GetComponent<CatalystInfo>();
            }
        }
        if (abilityInfo == null)
        {
            if(catalystInfo.abilityInfo)
            {
                conditions = catalystInfo.abilityInfo.destroyConditions;
                abilityInfo = catalystInfo.abilityInfo;
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


        HandleTicks();
        HandleLiveTime();
        HandleRange();
        HandleReturn();
        HandleDeadEntity();
    }
    public void OnTriggerEnter(Collider other)
    {
        if(conditions.destroyOnStructure)
        {
            if(other.CompareTag("Structure"))
            {
                DestroySelf();
            }
        }

        
    }

    public void HandleTicks()
    {
        if (conditions.destroyOnNoTickDamage)
        {
            if (catalystInfo.ticks <= 0)
            {
                DestroySelf();
            }
        }
    }

    public void HandleDeadEntity()
    {
        if (abilityInfo.targetsDead) { return; }
        if(catalystInfo.target && !catalystInfo.target.GetComponent<EntityInfo>().isAlive)
        {
            if(conditions.destroyOnDeadTarget)
            {
                DestroySelf();
            }
        }
    }



    public void HandleReturn()
    {
        if(conditions.destroyOnReturn)
        {
            if(GetComponent<CatalystReturn>() && GetComponent<CatalystReturn>().hasReturned)
            {
                DestroySelf();
            }
        }
    }
    public void HandleRange()
    {
        if(catalystInfo.range == -1) { return; }
        if (conditions.destroyOnOutOfRange)
        {
            if (catalystInfo.currentRange > catalystInfo.range)
            {
                DestroySelf();
            }

            if (gameObject.GetComponent<CatalystFreeScan>())
            {
                if (V3Helper.Abs(gameObject.transform.localScale) > catalystInfo.range*2)
                {
                    DestroySelf();
                }
            }
        }
    }
    public void HandleLiveTime()
    {
        if(catalystInfo && abilityInfo)
        {
            if(catalystInfo.liveTime > abilityInfo.liveTime && conditions.destroyOnLiveTime)
            {
                DestroySelf();
            }
        }
    }


    public void DestroySelf()
    {
        StartCoroutine(DestroyDelay());

        IEnumerator DestroyDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            catalystInfo.OnCatalystDestroy?.Invoke(this);
            Destroy(gameObject);
        }
    }
    
}
