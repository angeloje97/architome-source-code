using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Linq;

public class CatalystStop : MonoBehaviour
{
    // Start is called before the first frame update
    CatalystInfo catalystInfo;
    AbilityInfo abilityInfo;



    public float startSpeed;
    public float deceleration;
    public float distance;

    public bool stopped;

    


    void GetDependencies()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            if (catalystInfo.abilityInfo)
            {
                abilityInfo = catalystInfo.abilityInfo;
            }

        }
    }

    void DetermineDeceleration()
    {
        if(catalystInfo.target)
        {
            distance = V3Helper.Distance(catalystInfo.target.transform.position, transform.position);
        }
        else
        {
            distance = V3Helper.Distance(catalystInfo.location, transform.position);
        }

        if(distance > catalystInfo.range)
        {
            distance = catalystInfo.range;
        }

        deceleration = -((abilityInfo.speed * abilityInfo.speed) / (2*(distance)));
    }
    void Start()
    {
        GetDependencies();
        DetermineDeceleration();
        
    }
    // Update is called once per frame
    void Update()
    {
        if (stopped) return;
        UpdateSpeed();
    }
    void UpdateSpeed()
    {
        if(catalystInfo.speed > 0)
        {
            catalystInfo.speed += deceleration*Time.deltaTime;
        }
        if(catalystInfo.speed <= 0)
        {
            catalystInfo.speed = 0;
            catalystInfo.OnCatalystStop?.Invoke(this);
            stopped = true;
        }
        
    }

}
