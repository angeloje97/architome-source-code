using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Architome
{
    public class CombatHalfControl : CombatType
    {
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

        async void HandleTimerAsync()
        {
            while (this)
            {
                await Task.Yield();
                HandleTimer();

                if (abilityManager.currentlyCasting && abilityManager.currentlyCasting.abilityType2 != AbilityType2.AutoAttack)
                {
                    await abilityManager.currentlyCasting.EndActivation();
                }
            }
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
            if (entity.workerState != WorkerState.Idle) return;
            if (behavior.combatType == CombatBehaviorType.Passive && combat.GetFocus() == null) return;

            if (UsingAbility()) return;
            if (UsingHealingAbility()) return;

            var focusTarget = combat.GetFocus();
            var target = focusTarget != null ? focusTarget : combat.target;
            HandleHarm(target);
            HandleAutoHeal();

            abilityManager.target = null;

        }

        bool UsingAbility()
        {
            if (!abilityManager.IsOpen()) return false;

            foreach (var special in combat.specialAbilities)
            {

                //var ability = abilityManager.Ability(special.abilityIndex);
                var ability = special.ability;

                if (ability == null) continue;
                if (ability.IsBusy()) return true;
                if (!ability.IsReady()) continue;

                var target = combat.GetFocus() != null ? combat.GetFocus() : combat.target;

                if (special.targeting == SpecialTargeting.TargetsFocus)
                {
                    if(combat.GetFocus() == null && (int) behavior.combatType < 2) { continue; }
                    if (!ability.IsCorrectTarget(target)) continue;

                    abilityManager.target = target;
                    //abilityManager.Cast(special.abilityIndex);
                    abilityManager.Cast(special.ability);
                    abilityManager.target = null;

                    return true;
                }
                else if (special.targeting == SpecialTargeting.Use)
                {
                    if (ability.abilityType != AbilityType.Use) continue;

                    abilityManager.Cast(special.ability);
                    abilityManager.target = null;

                    return true;
                }
            }

            return false;
        }

        bool UsingHealingAbility()
        {
            if (entity.role != Role.Healer) return false;

            foreach (var specialAbility in combat.healSettings.specialHealingAbilities)
            {
                //var ability = abilityManager.Ability(specialAbility.abilityIndex);
                var ability = specialAbility.ability;

                if (ability == null) continue;
                if (ability.WantsToCast() || ability.isCasting) return true;
                if (!ability.isHealing && !ability.isAssisting) continue;
                if (!ability.IsReady()) continue;

                if (HealLowest(specialAbility, ability)) return true;
            }




            return false;

            bool HealLowest(SpecialHealing healing, AbilityInfo ability)
            {
                var allies = los.DetectedAllies().OrderBy(entity => entity.health / entity.maxHealth).ToList();


                if (allies.Count == 0) return false;

                foreach (var ally in allies)
                {
                    if (!ally.isAlive) continue;
                    if ((ally.health / ally.maxHealth) <= healing.minimumHealth)
                    {

                        abilityManager.target = ally;
                        abilityManager.location = ally.transform.position;
                        abilityManager.Cast(healing.ability);
                        abilityManager.target = null;

                        return true;
                    }
                }


                return false;
            }
        }
        void HandleHarm(EntityInfo target)
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

            if (behavior.combatType == CombatBehaviorType.Reactive)
            {
                if (!AttackReactive(target))
                {
                    AttackReactive2();
                }

            }

            AttackProactive(target);
        }
        bool AttackReactive(EntityInfo target)
        {
            if (behavior.combatType != CombatBehaviorType.Reactive) return false;
            if (!los.HasLineOfSight(target.gameObject)) return false;
            if (!abilityManager.attackAbility.AbilityIsInRange(target.gameObject)) { return false; }

            abilityManager.target = target;
            abilityManager.Attack();

            return true;
        }
        void AttackReactive2()
        {
            if(behavior.combatType != CombatBehaviorType.Reactive) { return; }

            var newThreat = threatManager.NearestHighestThreat(abilityManager.attackAbility.range);
            if (newThreat == null) return;

            if (!los.HasLineOfSight(newThreat.gameObject) || !abilityManager.attackAbility.AbilityIsInRange(newThreat.gameObject)) return;

            abilityManager.target = newThreat;
            abilityManager.Attack();
            

        }
        void AttackProactive(EntityInfo target)
        {
            if ((int) behavior.combatType < 2 ) return;

            abilityManager.target = target;
            abilityManager.Attack();
        }
        void HandleAutoHeal()
        {
            if ((int) behavior.combatType < 2) return;
            if (entity.role != Role.Healer) return;
            if (combat.GetFocus() != null && abilityManager.attackAbility.isAutoAttacking) return;
            if (entity.mana / entity.maxMana < combat.healSettings.minMana && combat.healSettings.minMana != 0f) return;

            Debugger.InConsole(93254, $"{entity} checking assist");

            var allies = los.DetectedAllies();
            allies.Add(entity);

            allies = allies.Where(ally => ally.health / ally.maxHealth <= combat.healSettings.targetHealth && ally.isAlive).ToList();

            if (allies.Count == 0) return;

            allies = allies.OrderBy(ally => ally.health/ally.maxHealth).ToList();

            abilityManager.target = allies[0];

            abilityManager.Attack();
            
        }

        
    }

}
