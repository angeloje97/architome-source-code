using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class CombatType : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entity;
        public CombatBehavior combat;
        public AIBehavior behavior;
        public AbilityManager abilityManager;
        public ThreatManager threatManager;
        public LineOfSight los;

        public float currentTime;

        public void GetDependencies()
        {
            combat = GetComponentInParent<CombatBehavior>();
            behavior = GetComponentInParent<AIBehavior>();
            entity = GetComponentInParent<EntityInfo>();
            

            if (entity)
            {
                abilityManager = entity.AbilityManager();
            }

            if (behavior)
            {
                los = behavior.LineOfSight();
                threatManager = behavior.ThreatManager();
            }

        }

    }

}
