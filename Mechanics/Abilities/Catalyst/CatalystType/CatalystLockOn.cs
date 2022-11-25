using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class CatalystLockOn : MonoBehaviour
{
    // Start is called before the first frame update
    public CatalystInfo catalystInfo;
    public AbilityInfo abilityInfo;

    public CatalystHit catalystHit;

    public GameObject target;

    public float speed;
    public float smoothening;

    public bool closeToTarget;

    public bool isAlive = true;


    [Header("Settings")]
    public bool disableLockOn;

    public void GetDependencies()
    {
        catalystInfo = GetComponent<CatalystInfo>();
        catalystHit = GetComponent<CatalystHit>();
        if (catalystInfo)
        {
            abilityInfo = catalystInfo.abilityInfo;
            catalystInfo.OnNewTarget += OnNewTarget;
            target = catalystInfo.target;
            speed = catalystInfo.speed;

            catalystInfo.OnCloseToTarget += OnCloseToTarget;
            catalystInfo.OnHit += OnHit;
        }

        isAlive = true;
    }
    void Start()
    {
        GetDependencies();
        StartCoroutine(HandleCloseTarget());
        HandleInertia();

        //StartCoroutine(HandleDeadTarget());
    }

    void HandleInertia()
    {
        if (!target) return;
        

        transform.LookAt(target.transform);

        GetStartDirection();

        smoothening =  catalystInfo.metrics.inertia;

    }

    void GetStartDirection()
    {
        if (catalystInfo.metrics.startDirectionRange == new Vector3()) return;
        if (catalystInfo.target == catalystInfo.entityObject) return;

        var role = Random.Range(0, 100);
        int factor = 1;
        if (role > 50)
        {
            factor = -1;
        }
        var angle = factor * catalystInfo.metrics.startDirectionRange;

        transform.eulerAngles += angle;

    }

    public void OnHit(CatalystInfo catalyst, EntityInfo target)
    {
        if (catalystInfo.metrics.inertia <= 0) return;
        smoothening = 300f;

        ArchAction.Delay(() => { smoothening = catalystInfo.metrics.inertia; }, .25f);
    }

    // Update is called once per frame
    void Update()
    {
        if (target == null) return;
        if (disableLockOn) return;
        TravelToTarget();
        HandleDeadTarget();
        TravelToTargetInertia();
    }

    void TravelToTargetInertia()
    {
        if (target == catalystInfo.entityObject) return;

        if (catalystInfo.metrics.inertia <= 0)
        {
            return;
        }

        transform.rotation = V3Helper.LerpLookAt(transform, target.transform, 1 / smoothening);

        if (smoothening > 1)
        {
            smoothening -= Time.deltaTime * catalystInfo.metrics.inertiaFallOff;
        }
        else
        {
            if (smoothening != 1)
            {
                smoothening = 1;
            }
        }

    }

    void TravelToTarget()
    {
        if (catalystInfo.metrics.inertia != 0f) return;
        if (!target) { return; }
        transform.LookAt(target.transform);

        if(speed == -1)
        {
            transform.position = target.transform.position;
            if(catalystHit)
            {
                catalystHit.HandleTargetHit(target.GetComponent<EntityInfo>());
            }
        }
    }

    void HandleDeadTarget()
    {
        if (!Entity.IsEntity(target)) {

            
            return; 
        }
        
        if (isAlive != target.GetComponent<EntityInfo>().isAlive)
        {
            isAlive = target.GetComponent<EntityInfo>().isAlive;

            if (!isAlive)
            {
                Debugger.InConsole(8562, $"{catalystInfo}'s target has died");
                catalystInfo.OnDeadTarget?.Invoke(target);
            }

            ArchAction.Delay(() => isAlive = !isAlive, 1f);
        }
    }

    public void OnCloseToTarget(CatalystInfo catalyst, GameObject target)
    {
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
