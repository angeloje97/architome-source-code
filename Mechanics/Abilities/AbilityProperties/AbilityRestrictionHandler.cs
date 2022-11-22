using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    [Serializable]
    public class AbilityRestrictionHandler
    {
        AbilityInfo ability;
        EntityInfo entity;
        public AugmentProp.Restrictions restrictions;

        public void Initiate(AbilityInfo ability)
        {
            this.ability = ability;
            entity = ability.entityInfo;
            HandleMain();
            HandleDeadTarget();
            HandleCatalystHit();
            HandleUIVisibility();
        }

        public string Description()
        {
            return restrictions.Description();
        }

        void HandleUIVisibility()
        {
            ability.OnCanShowCheck += HandleCanShow;

            void HandleCanShow(AbilityInfo ability, List<bool> checks)
            {
                if (restrictions.hideUI) checks.Add(false);
            }
        }

        void HandleCatalystHit()
        {
            ability.OnCatalystRelease += (CatalystInfo newCatalyst) => {
                if (newCatalyst.isCataling)
                {
                    Debugger.Combat(1964, $"New Catalyst Created {newCatalyst}");
                }
                newCatalyst.OnCanAssistCheck += HandleAssist;
                newCatalyst.OnCanHarmCheck += HandleHarm;
                newCatalyst.OnCanHealCheck += HandleHeal;
                newCatalyst.OnCanHitCheck += HandleHit;
                newCatalyst.OnCorrectLockOnCheck += HandleCorrectLockOn;
            };


            void HandleCorrectLockOn(CatalystHit hit, EntityInfo target, List<bool> checks)
            {
                if(ability.abilityType != AbilityType.LockOn)
                {
                    checks.Add(true); return;
                }


            }
            void HandleHit(CatalystHit hit, EntityInfo target, List<bool> checks)
            {
                if(target == entity && !restrictions.canCastSelf)
                {
                    checks.Add(false); return;
                }

                if(target.isAlive && restrictions.onlyTargetsDead)
                {
                    checks.Add(false); return;
                }

                if(!target.isAlive && !restrictions.targetsDead)
                {
                    checks.Add(false);
                }
            }

            void HandleAssist(CatalystHit catalyst, EntityInfo target, List<bool> checks)
            {
                if (!restrictions.isAssisting) return;
                if (!entity.CanHelp(target)) return;
                if (catalyst.AssistContains(target) && !restrictions.canHitSameTarget) return;


                checks.Add(true);


            }

            void HandleHarm(CatalystHit catalyst, EntityInfo target, List<bool> checks)
            {
                if (!restrictions.isHarming)
                {
                    checks.Add(false); return;
                }

                if (!entity.CanAttack(target))
                {
                    checks.Add(false); return;
                }

                if (catalyst.EnemiesHitContains(target) && !restrictions.canHitSameTarget)
                {
                    checks.Add(false); return;
                }
            }

            void HandleHeal(CatalystHit catalyst, EntityInfo target, List<bool> checks)
            {
                if (!restrictions.isHealing) return;
                if (!entity.CanHelp(target)) return;
                if (catalyst.HealedContains(target) && !restrictions.canAssistSameTarget) return;

                checks.Add(true);

            }
        }

        void HandleDeadTarget()
        {
            ability.OnCorrectTargetCheck += HandleCorrectTarget;

            void HandleCorrectTarget(AbilityInfo ability, EntityInfo target, List<bool> orChecks, List<bool> andChecks)
            {
                if (!target.isAlive && !restrictions.targetsDead)
                {
                    andChecks.Add(false);
                    ability.abilityManager.OnDeadTarget?.Invoke(ability);
                }

                if(restrictions.onlyTargetsDead && target.isAlive)
                {
                    andChecks.Add(false);
                }

            }
        }

        void HandleMain()
        {
            ability.OnCorrectTargetCheck += HandleCorrectTargetCheck;
            ability.OnHandlingTargetLocked += OnHandlingTargetLocked;

            ability.OnCanHarmCheck += HandleHarmCheck;
            ability.OnCanHealCheck += HandleHealCheck;

            void HandleHealCheck(AbilityInfo ability, EntityInfo target, List<bool> checks)
            {

                if (restrictions.isAssisting || restrictions.isHealing)
                {
                    if (entity.CanHelp(target))
                    {
                        checks.Add(true);
                    }
                }
            }

            void HandleHarmCheck(AbilityInfo ability, EntityInfo target, List<bool> checks)
            {
                if (!restrictions.isHarming)
                {
                    checks.Add(false); return;
                }
                if (!entity.CanAttack(target))
                {
                    checks.Add(false); return;
                }
            }
            void HandleCorrectTargetCheck(AbilityInfo ability,EntityInfo target, List<bool> orChecks, List<bool> andChecks)
            {
                if(entity == target)
                {
                    if (!restrictions.canCastSelf)
                    {
                        andChecks.Add(false);
                    }
                }
                else
                {
                    if (restrictions.onlyCastSelf)
                    {
                        ability.target = entity;
                        ability.targetLocked = entity;
                    }
                }

            }

            void OnHandlingTargetLocked(AbilityInfo ability, EntityInfo target)
            {
                if(ability.abilityType == AbilityType.Use || restrictions.onlyCastSelf)
                {
                    ability.target = entity;
                    return;
                }

                if (!IsCorrectCombatTarget(target) && IsCorrectCombatTarget(entity))
                {
                    ability.target = entity;
                }
            }
        }

        public bool IsCorrectCombatTarget(EntityInfo target)
        {
            if (restrictions.isHarming && entity.CanAttack(target))
            {
                return true;
            }

            if ((restrictions.isAssisting || restrictions.isHealing) && entity.CanHelp(target))
            {
                return true;
            }

            return false;
        }
    }
}
