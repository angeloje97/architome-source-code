using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class CatalystLockOn : MonoBehaviour
{
    // Start is called before the first frame update
    public CatalystInfo catalystInfo;
    public AbilityInfo abilityInfo;

    public GameObject target;

    public float speed;

    public bool closeToTarget;

    public void GetDependenciesStart()
    {
        if (GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            abilityInfo = catalystInfo.abilityInfo;

            catalystInfo.OnNewTarget += OnNewTarget;
        }
    }
    public void GetDependencies()
    {
        

        if(target == null)
        {
            if(catalystInfo && catalystInfo.target)
            {
                target = catalystInfo.target;
            }
        }

        if(abilityInfo && speed == 0)
        {
            speed = abilityInfo.speed;
        }
    }
    void Start()
    {
        GetDependenciesStart();
    }

    

    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleCloseTarget();
        TravelToTarget();
    }

    void TravelToTarget()
    {
        if (!target) { return; }
        transform.LookAt(target.transform);

        if(speed == -1)
        {
            transform.position = target.transform.position;
            if(GetComponent<CatalystHit>())
            {
                GetComponent<CatalystHit>().HandleTargetHit(target.GetComponent<EntityInfo>());
            }
        }
        else
        {
            transform.Translate(speed * Time.deltaTime * Vector3.forward);
        }
    }

    public void HandleCloseTarget()
    {
        if(target == null) { return; }
        if(V3Helper.Distance(target.transform.position, catalystInfo.transform.position) > .5f) { return; }
        if (catalystInfo.isDestroyed) { return; }
        if (closeToTarget) { return; }

        closeToTarget = true;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(.125f);

            closeToTarget = false;

            if (!catalystInfo.isDestroyed)
            {
                if(V3Helper.Distance(target.transform.position, catalystInfo.transform.position) < 1f)
                {
                    catalystInfo.OnCloseToTarget?.Invoke(catalystInfo, target);
                }
            }


        }
    }

    public void OnNewTarget(GameObject previous, GameObject next)
    {
        closeToTarget = false;
        target = next;
        
    }
    
}
