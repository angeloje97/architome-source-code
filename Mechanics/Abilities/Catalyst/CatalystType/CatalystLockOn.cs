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

    public void GetDependencies()
    {
        if (GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            abilityInfo = catalystInfo.abilityInfo;
            catalystInfo.OnNewTarget += OnNewTarget;
            target = catalystInfo.target;
            speed = catalystInfo.speed;

            catalystInfo.OnCloseToTarget += OnCloseToTarget;
        }


    }
    void Start()
    {
        GetDependencies();
        StartCoroutine(HandleCloseTarget());
        //StartCoroutine(HandleDeadTarget());
    }

    

    // Update is called once per frame
    void Update()
    {
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

    public void OnCloseToTarget(CatalystInfo catalyst, GameObject target)
    {
        if(!target.GetComponent<EntityInfo>()) { return; }

        if(!target.GetComponent<EntityInfo>().isAlive)
        {
            catalystInfo.OnDeadTarget?.Invoke(target);
        }
    }
    public IEnumerator HandleCloseTarget()
    {
        while(true)
        {
            yield return new WaitForSeconds(.125f);
            CheckIfCloseToTarget();
        }

        void CheckIfCloseToTarget()
        {
            if (target == null) { return; }
            if (catalystInfo.isDestroyed) { return; }


            closeToTarget = V3Helper.Distance(target.transform.position, catalystInfo.transform.position) < .25f;

            if(closeToTarget)
            {
                catalystInfo.OnCloseToTarget?.Invoke(catalystInfo, target);
            }

        }
    }

    public void OnNewTarget(GameObject previous, GameObject next)
    {
        target = next;
        
    }
    
}
