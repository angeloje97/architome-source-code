using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using System.Linq;

namespace Architome
{
    public class CombatHalfControl : CombatType
    {
        // Start is called before the first frame update
        new void GetDependencies()
        {
            base.GetDependencies();

        }
        void Start()
        {
            GetDependencies();
            OnCombatRoutine();
        }

        public void Update()
        {
            HandleTimer();
        }

        void HandleTimer()
        {
            if (!entity.isAlive) return;
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else if (currentTime <= 0)
            {
                currentTime = .250f;
                OnCombatRoutine();
            }

        }

        // Update is called once per frame
        void OnCombatRoutine()
        {
            if (combat.tryMoveTimer > 0) { return; }
            if (behavior.behaviorState != BehaviorState.Idle) return;
            if (behavior.combatType == CombatBehaviorType.Passive && combat.GetFocus() == null) return;

            if (UsingAbility()) return;

            var target = combat.GetFocus() != null ? combat.GetFocus() : combat.target;
            HandleHarm(target);
            HandleAssist();

            abilityManager.target = null;

        }

        bool UsingAbility()
        {
            var currentAbility = abilityManager.currentlyCasting;

            if (currentAbility != null && !currentAbility.isAttack) return false;

            foreach (var special in combat.specialAbilities)
            {

                var ability = abilityManager.Ability(special.abilityIndex);

                if (ability == null) continue;
                if (ability.IsBusy()) return true;
                if (!ability.IsReady()) continue;

                var target = combat.GetFocus() != null ? combat.GetFocus() : combat.target;

                if (special.targeting == SpecialTargeting.TargetsFocus)
                {
                    if(combat.GetFocus() == null && (int) behavior.combatType < 2) { continue; }

                    abilityManager.target = target;
                    abilityManager.Cast(special.abilityIndex);
                    abilityManager.target = null;

                    return true;
                }
                else
                {
                    if (ability.requiresLockOnTarget)
                    {
                        abilityManager.target = target;
                    }

                    abilityManager.Cast(special.abilityIndex);
                    abilityManager.target = null;

                    return true;
                }
            }

            return false;
        }
        void HandleHarm(GameObject target)
        {
            if (abilityManager.attackAbility == null) return;
            if (!entity.CanAttack(target)) return;
            if (!abilityManager.attackAbility.isHarming) return;

            var focusTarget = combat.GetFocus();

            
            if (focusTarget != null)
            {
                abilityManager.target = target;
                abilityManager.Attack();
                abilityManager.target = null;

                return;
            }


            if (!AttackReactive(target))
            {
                AttackReactive2();
            }

            AttackProactive(target);
        }
        bool AttackReactive(GameObject target)
        {
            if (behavior.combatType != CombatBehaviorType.Reactive) return false;
            if (!los.HasLineOfSight(target)) return false;
            if (!abilityManager.attackAbility.AbilityIsInRange(target)) { return false; }

            abilityManager.target = target;
            abilityManager.Attack();

            return true;
        }
        void AttackReactive2()
        {
            if(behavior.combatType != CombatBehaviorType.Reactive) { return; }

            var newThreat = threatManager.NearestHighestThreat(abilityManager.attackAbility.range);

            if (!los.HasLineOfSight(newThreat) || !abilityManager.attackAbility.AbilityIsInRange(newThreat)) return;

            abilityManager.target = newThreat;
            abilityManager.Attack();
            

        }
        void AttackProactive(GameObject target)
        {
            if ((int) behavior.combatType < 2 ) return;

            abilityManager.target = target;
            abilityManager.Attack();
        }
        void HandleAssist()
        {
            if ((int) behavior.combatType < 2) return;
            if (entity.role != Role.Healer) return;
            if (entity.mana / entity.maxMana < .60) return;

            var allies = los.DetectedEntities(entity.npcType);
            allies.Add(entity);

            allies = allies.Where(ally => ally.health / ally.maxHealth <= .80 && ally.isAlive).ToList();

            if (allies.Count == 0) return;

            allies = allies.OrderBy(ally => ally.health/ally.maxHealth).ToList();

            abilityManager.target = allies[0].gameObject;

            abilityManager.Attack();
            
        }

        
    }

}
