using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class CatalingActions : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;

    public CatalystInfo catalystInfo;
    public AbilityInfo abilityInfo;

    public GameObject cataling;
    public CatalystInfo catalingInfo;

    void GetDependencies()
    {
        if(GetComponent<CatalystInfo>())
        {
            catalystInfo = GetComponent<CatalystInfo>();
            abilityInfo = catalystInfo.abilityInfo;

            if (abilityInfo.cataling)
            {
                cataling = abilityInfo.cataling;
                catalingInfo = cataling.GetComponent<CatalystInfo>();
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

    }

    public void CastAt(EntityInfo target)
    {
        catalingInfo.abilityInfo = abilityInfo;
        catalingInfo.target = target.gameObject;
        catalingInfo.isCataling = true;
        catalingInfo.requiresLockOnTarget = true;
        catalingInfo.catalyingAbilityType = AbilityType.LockOn;

        var cataling = Instantiate(this.cataling, transform.position, transform.localRotation);

        catalystInfo.OnCatalingRelease?.Invoke(catalystInfo, cataling.GetComponent<CatalystInfo>());
    }

    public void CastTowards(GameObject location)
    {

    }
}
