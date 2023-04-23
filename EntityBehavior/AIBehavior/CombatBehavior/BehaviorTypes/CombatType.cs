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

        protected EntityInfo target;

        public float currentTime;
        protected bool inRoutine { get; set; }
        protected bool autoAttacking { get; set; }
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

            if (combat && combat.specialAbilities != null)
            {

                foreach(var specialAbility in combat.specialAbilities)
                {
                    specialAbility.ability.coolDown.OnRecharge += HandleRechargeAbility;
                }
            }

        }

        public virtual void HandleRechargeAbility(AbilityInfo ability, AbilityCoolDown coolDown)
        {

        }

        public virtual void DestroySelf()
        {
            Destroy(gameObject);
        }

        public virtual void CombatRoutine()
        {

        }

        protected virtual async Task HandleAttack()
        {
            var attackAbility = abilityManager.attackAbility;
            autoAttacking = true;
            abilityManager.target = target;
            abilityManager.Attack();
            while (abilityManager.target && abilityManager.target.isAlive && attackAbility.isAutoAttacking)
            {
                if (!autoAttacking) break;
                await Task.Delay(250);
            }
            autoAttacking = false;
            abilityManager.target = null;
        }

    }

}
