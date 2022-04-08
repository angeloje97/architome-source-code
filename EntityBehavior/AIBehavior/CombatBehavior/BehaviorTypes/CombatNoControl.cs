using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class CombatNoControl : CombatType
    {
        // Start is called before the first frame update
        GameObject target;
        new void GetDependencies()
        {
            base.GetDependencies();

            if (combat)
            {
                combat.OnSetFocus += OnSetFocus;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            HandleTimers();
        }

        void HandleTimers()
        {
            if (currentTime > 0)
            {
                currentTime -= Time.deltaTime;
            }
            else
            {
                currentTime = .250f;
                OnCombatRoutine();
            }
        }

        void OnCombatRoutine()
        {
            target = combat.GetFocus() ? combat.GetFocus() : combat.target;

            if (UsingAbility())
            {
                return;
            }
            else
            {
                abilityManager.target = target;
                abilityManager.Attack();
                abilityManager.target = null;
            }

        }

        void OnSetFocus(GameObject target)
        {

            OnCombatRoutine();
        }


        bool UsingAbility()
        {
            if (AbilityHarm())
            {
                return true;
            }

            if (AbilityAssist())
            {
                return true;
            }

            return false;
        }

        bool AbilityHarm()
        {
            foreach (var specialAbility in combat.specialAbilities)
            {
                var ability = abilityManager.Ability(specialAbility.abilityIndex);

                if (ability == null) continue;
                if (ability.WantsToCast() || ability.isCasting) return true;
                if (!ability.isHarming) continue;
                if (!ability.IsReady()) continue;

                //Special Ability Targeting
                if (TargetsCurrent(specialAbility)) return true;
                if (TargetsRandom(specialAbility)) return true;
                if (Use(specialAbility)) return true;
            }
            return false;

            bool TargetsRandom(SpecialAbility special)
            {
                if (special.targeting != SpecialTargeting.TargetsRandom) return false;

                var randomTarget = threatManager.RandomTargetBlackList(special.randomTargetBlackList);

                if (randomTarget == null) return false;

                abilityManager.target = randomTarget;
                abilityManager.Cast(special.abilityIndex);

                return true;
            }

            bool TargetsCurrent(SpecialAbility special)
            {
                if (special.targeting != SpecialTargeting.TargetsCurrent) return false;

                if (target == null) return false;

                abilityManager.target = this.target;
                abilityManager.Cast(special.abilityIndex);

                return true;
            }

            bool Use(SpecialAbility special)
            {
                var ability = abilityManager.Ability(special.abilityIndex);

                if (ability.abilityType != AbilityType.Use) return false;

                abilityManager.Cast(special.abilityIndex);

                return true;
            }
        }

        bool AbilityAssist()
        {
            if (entity.role != Role.Healer) return false;

            return false;
        }
    }

}
