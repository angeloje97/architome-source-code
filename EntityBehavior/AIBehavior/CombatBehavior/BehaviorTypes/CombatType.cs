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

        protected Action<CombatType> OnDestroySelf { get; set; }
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

                if (threatManager)
                {
                    threatManager.OnNewThreat += HandleNewThreat;

                    OnDestroySelf += (CombatType type) => {
                        threatManager.OnNewThreat -= HandleNewThreat;
                    };
                }

            }

            if (combat && combat.specialAbilities != null)
            {

                foreach(var specialAbility in combat.specialAbilities)
                {
                    specialAbility.ability.coolDown.OnRecharge += HandleRechargeAbility;
                    OnDestroySelf += (CombatType type) => {
                        specialAbility.ability.coolDown.OnRecharge -= HandleRechargeAbility;
                    };
                }

                combat.OnAddedSpecialAbility += HandleNewSpecialAbility;

                OnDestroySelf += (CombatType type) => {
                    combat.OnAddedSpecialAbility -= HandleNewSpecialAbility;
                };
            }

        }

        public virtual void HandleRechargeAbility(AbilityInfo ability, AbilityCoolDown coolDown)
        {
            if (ability.isAttack) return;
            autoAttacking = false;
        }

        public virtual void HandleNewSpecialAbility(SpecialAbility special)
        {
            autoAttacking = false;
        }

        public virtual void DestroySelf()
        {
            OnDestroySelf?.Invoke(this);
            Destroy(gameObject);
        }

        public virtual void CombatRoutine()
        {

        }

        protected virtual void HandleNewThreat(ThreatManager.ThreatInfo threatInfo)
        {

        }



        protected virtual async Task HandleAttack()
        {
            var attackAbility = abilityManager.attackAbility;
            autoAttacking = true;
            abilityManager.target = target;
            abilityManager.Attack();
            float timer = 0f;
            while (abilityManager.target && abilityManager.target.isAlive && attackAbility.isAutoAttacking)
            {
                if (!autoAttacking) break;
                await Task.Delay(250);
                timer += .250f;

                if (timer > 2f) break;
            }
            autoAttacking = false;
            abilityManager.target = null;
        }

    }

}
