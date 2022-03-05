using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.RVO;
using Architome;
public class MovementRVO : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public GameObject entityObject;
    public RVOController controller;
    public Movement movement;

    public float distanceToTargetLocation;

    public bool isNearByEntity = false;

    void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();

            entityObject = entityInfo.gameObject;

            if(!entityObject.GetComponent<RVOController>())
            {
                entityObject.AddComponent<RVOController>();
                controller = entityObject.GetComponent<RVOController>();
                controller.lockWhenNotMoving = true;
            }
        }

        if(GetComponent<Movement>())
        {
            movement = GetComponent<Movement>();
        }
    }
    void Start()
    {
        GetDependencies();
        StartCoroutine(HandleNearbyEntity());
    }

    // Update is called once per frame
    void Update()
    {
        if (controller == null) { return; }
        HandleController();
        UpdateMetrics();
    }

    void UpdateMetrics()
    {
    }

    IEnumerator HandleNearbyEntity()
    {
        while(true)
        {
            yield return new WaitForSeconds(1f);
            Collider[] rangeChecks = Physics.OverlapSphere(entityObject.transform.position, 1f, entityInfo.LineOfSight().targetLayer);
            Debugger.InConsole(8123, $"{rangeChecks.Length}");

            if(rangeChecks.Length > 1)
            {
                if(!isNearByEntity)
                {
                    isNearByEntity = true;
                    HandleStopMoving();
                }
            }
            else
            {
                if(isNearByEntity)
                {
                    isNearByEntity = false;
                }
            }
        }


        void HandleStopMoving()
        {
            
        }
    }

    void HandleController()
    {
        
        var delta = controller.CalculateMovementDelta(entityObject.transform.position, Time.deltaTime);

        
        
    }
}
