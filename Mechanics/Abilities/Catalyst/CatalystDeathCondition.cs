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

    public string destroyReason;
    public float destroyDelay = 0f;

    public void GetDependencies()
    {
        if(catalystInfo == null)
        {
            if(gameObject.GetComponent<CatalystInfo>())
            {
                catalystInfo = gameObject.GetComponent<CatalystInfo>();
                catalystInfo.OnWrongTargetHit += OnWrongTargetHit;
                catalystInfo.OnTickChange += OnTickChange;
                catalystInfo.OnReturn += OnReturn;
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
        HandleLiveTime();
        HandleRange();
        HandleDeadEntity();
    }
    public void OnTriggerEnter(Collider other)
    {
        if(conditions.destroyOnStructure)
        {
            if(other.CompareTag("Structure"))
            {
                DestroySelf("Collided With Wall");
            }
        }

        
    }

    public void OnTickChange(CatalystInfo catalyst, int ticks)
    {
        if (conditions.destroyOnNoTickDamage)
        {
            if (ticks <= 0)
            {
                DestroySelf("No Ticks");
            }

        }
    }

    public void OnWrongTargetHit(CatalystInfo catalyst, GameObject target)
    {
        DestroySelf("Wrong Target Hit");
    }

    public void HandleDeadEntity()
    {
        if (abilityInfo.targetsDead) { return; }
        if(catalystInfo.target && !catalystInfo.target.GetComponent<EntityInfo>().isAlive)
        {
            if(conditions.destroyOnDeadTarget)
            {
                DestroySelf("Dead Target");
            }
        }
    }



    public void OnReturn(CatalystInfo catalyst)
    {
        if(gameObject == null) { return; }
        if(conditions.destroyOnReturn)
        {

            if(GetComponent<CatalystReturn>() && GetComponent<CatalystReturn>().hasReturned)
            {
                DestroySelf("Returned");
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
                DestroySelf("Out of Range");
            }

            if (gameObject.GetComponent<CatalystFreeScan>())
            {
                if (V3Helper.Abs(gameObject.transform.localScale) > catalystInfo.range*2)
                {
                    DestroySelf("Out of Range");
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
                DestroySelf("Expired");
            }
        }
    }


    public void DestroySelf(string reason)
    {
        StartCoroutine(DestroyDelay());
        catalystInfo.OnCatalystDestroy?.Invoke(this);
        destroyReason = reason;
        Debugger.InConsole(1294, $"Catalyst Destroyed for {reason}");
        IEnumerator DestroyDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            if(gameObject!= null)
            {
                catalystInfo.isDestroyed = true;
                Destroy(gameObject);

            }
        }
    }
    
}
