using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystProp : MonoBehaviour
    {
        // Start is called before the first frame update
        public AbilityInfo ability;
        public CatalystInfo catalyst;
        public EntityInfo entity;

        public void GetDependencies()
        {
            catalyst = GetComponent<CatalystInfo>();
            
            if (catalyst)
            {
                ability = catalyst.abilityInfo;
                entity = catalyst.entityInfo;
            }
        }
    }

}