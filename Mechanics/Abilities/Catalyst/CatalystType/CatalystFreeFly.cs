using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class CatalystFreeFly : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public Vector3 direction;

    public EntityInfo entityInfo;
    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;
    public void GetDependencies()
    {
        catalystInfo = GetComponent<CatalystInfo>();

        if (catalystInfo)
        {
            abilityInfo = catalystInfo.abilityInfo;
        }

        if(abilityInfo)
        {
            direction = abilityInfo.directionLocked;
            entityInfo = abilityInfo.entityInfo;
        }
    }
    void Start()
    {
        GetDependencies();
    }

    public void LookAtLocation(Vector3 location)
    {
        transform.LookAt(location);
    }

    public void ChangeDirection(Vector3 direction)
    {
        transform.eulerAngles = direction;
    }

    // Update is called once per frame
    void Update()
    {
        FlyForward();
    }

    void FlyForward()
    {
        transform.Translate(Vector3.forward * catalystInfo.speed * Time.deltaTime);
    }
}
