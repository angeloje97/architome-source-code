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


    public void GetDependenciesStart()
    {
        if (GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            abilityInfo = catalystInfo.abilityInfo;
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
        StartCoroutine(HandleCloseTarget());
    }

    

    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleTargettingChange();
        TravelToTarget();
    }

    void TravelToTarget()
    {
        if (target)
        {
            transform.LookAt(target.transform);
        }

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
    public void HandleTargettingChange()
    {
        if(target == null && catalystInfo.target)
        {
            target = catalystInfo.target;
        }
        else if(target != catalystInfo.target)
        {
            target = catalystInfo.target;
        }
    }

    IEnumerator HandleCloseTarget()
    {
        while (true)
        {
            yield return new WaitForSeconds(.25f);
            if (catalystInfo.target != null)
            {
                Collider[] targets = Physics.OverlapSphere(transform.position, 1f, catalystInfo.abilityInfo.targetLayer);

                foreach(Collider check in targets)
                {
                    if(check.gameObject == catalystInfo.target)
                    {
                        if(check.GetComponent<EntityInfo>())
                        {
                            GetComponent<CatalystHit>().HandleTargetHit(check.GetComponent<EntityInfo>());
                        }
                        catalystInfo.HandleDeadTarget();
                    }
                    
                }
            }

            
        }
    }
}
