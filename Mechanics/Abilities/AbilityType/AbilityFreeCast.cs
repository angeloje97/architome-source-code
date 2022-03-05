using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class AbilityFreeCast : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public GameObject catalyst;

    public EntityInfo entityInfo;
    public CatalystInfo catalystInfo;
    public AbilityInfo abilityInfo;
    public void GetDependencies()
    {
        if (gameObject.GetComponent<AbilityInfo>())
        {
            if (abilityInfo == null)
            {
                abilityInfo = gameObject.GetComponent<AbilityInfo>();
            }
            if (entityInfo == null)
            {
                entityInfo = abilityInfo.entityInfo;
                entityObject = entityInfo.gameObject;
            }
            if (catalyst == null)
            {
                if (abilityInfo.catalyst)
                {
                    catalyst = abilityInfo.catalyst;
                    catalystInfo = abilityInfo.catalyst.GetComponent<CatalystInfo>();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
            GetDependencies();
    }

    public void Cast()
    {
        catalystInfo.abilityInfo = abilityInfo;
        Instantiate(catalyst, entityObject.transform.position, entityObject.transform.rotation);
    }
}
