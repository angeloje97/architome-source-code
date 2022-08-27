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
            OnCombatRoutine();
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
            if (entity.workerState != WorkerState.Idle) return;
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
                //var ability = abilityManager.Ability(specialAbility.abilityIndex);
                var ability = specialAbility.ability;

                if (ability == null) continue;
                if (ability.WantsToCast() || ability.isCasting) return true;
                if (!ability.isHarming) continue;
                if (!ability.IsReady()) continue;

                //Special Ability Targeting
                if (TargetsCurrent(specialAbility, ability)) return true;
                if (TargetsRandom(specialAbility, ability)) return true;
                if (Use(specialAbility, ability)) return true;
                if (RandomLocation(specialAbility, ability)) return true;
            }
            return false;

            bool TargetsRandom(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.TargetsRandom) return false;

                var randomTarget = threatManager.RandomTargetBlackList(special.randomTargetBlackList);

                if (randomTarget == null) return false;

                abilityManager.target = randomTarget;
                abilityManager.location = randomTarget.transform.position;
                abilityManager.Cast(ability);

                return true;
            }

            bool TargetsCurrent(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.TargetsCurrent) return false;

                if (target == null) return false;

                abilityManager.location = this.target.transform.position;
                abilityManager.target = this.target;
                abilityManager.Cast(ability);

                return true;
            }

            bool Use(SpecialAbility special, AbilityInfo ability)
            {
                if (ability.abilityType != AbilityType.Use) return false;
                if (special.targeting != SpecialTargeting.Use) return false;

                abilityManager.Cast(ability);

                return true;
            }

            bool RandomLocation(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.RandomLocation) return false;

                var maxDistance = ability.catalystInfo.range;
                var randomX = Random.Range(-maxDistance, maxDistance);
                var randomZ = Random.Range(-maxDistance, maxDistance);

                abilityManager.location = abilityManager.transform.position + new Vector3(randomX, 0, randomZ);
                abilityManager.Cast(ability);


                return true;
            }



            
        }

        bool AbilityAssist()
        {
            if (entity.role != Role.Healer) return false;

            foreach (var specialAbility in combat.healSettings.specialHealingAbilities)
            {
                //var ability = abilityManager.Ability(specialAbility.abilityIndex);
                var ability = specialAbility.ability;
                if (ability == null) continue;
                if (ability.WantsToCast() || ability.isCasting) return true;
                if (!ability.isHealing || ability.isAssisting) continue;
                if (!ability.IsReady()) continue;
            }


            return false;
        }
    }

}
