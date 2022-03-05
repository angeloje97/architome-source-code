using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatalystLockScan : MonoBehaviour
{
    // Start is called before the first frame update
    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;

    public GameObject target;

    public float xGrowth = 1;
    public float zGrowth;
    public void GetDependencies()
    {
        if(target == null)
        {
            if (catalystInfo.target)
            {
                target = catalystInfo.target;
            }
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        GetDependencies();
        HandleLookingAt();
        HandleGrowth();
        
    }

    public void HandleLookingAt()
    {
        if(target == null)
        {
            return;
        }
        transform.LookAt(target.transform.position);
    }

    public void HandleGrowth()
    {
        if(catalystInfo)
        {
            zGrowth += catalystInfo.speed * Time.deltaTime;

            transform.localScale = new Vector3(xGrowth, transform.localScale.y, zGrowth);
        }
    }
}
