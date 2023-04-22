using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    public class CombatNoControl : CombatType
    {
        // Start is called before the first frame update
        EntityInfo target;
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

        async void OnCombatRoutine()
        {
            if (entity.workerState != WorkerState.Idle) return;
            target = combat.GetFocus() ? combat.GetFocus() : combat.target;
            if (!abilityManager.IsOpen()) return;
            if (target == null) return;
            if (inRoutine) return;
            inRoutine = true;

            Debugger.Combat(6313, $"{entity} combat routine active");

            await UsingAbility();

            if (target)
            {
                abilityManager.target = target;
                abilityManager.Attack();
                abilityManager.target = null;
            }

            inRoutine = false;
        }

        void OnSetFocus(EntityInfo target)
        {

            OnCombatRoutine();
        }


        async Task UsingAbility()
        {
            await AbilityHarm();

            await AbilityAssist();

        }

        async Task AbilityHarm()
        {
            foreach (var specialAbility in combat.specialAbilities)
            {
                var ability = specialAbility.ability;

                if (ability == null) continue;
                if (!ability.isHarming) continue;
                if (!ability.IsReady()) continue;

                //Special Ability Targeting
                if (await TargetsCurrent(specialAbility, ability)) return;
                if (await TargetsRandom(specialAbility, ability)) return;
                if (await Use(specialAbility, ability)) return;
                if (await RandomLocation(specialAbility, ability)) return;
            }

            async Task<bool> TargetsRandom(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.TargetsRandom) return false;

                var randomTarget = threatManager.RandomTargetBlackList(special.randomTargetBlackList);

                if (randomTarget == null) return false;

                abilityManager.target = randomTarget;
                abilityManager.location = randomTarget.transform.position;
                await abilityManager.Cast(ability);

                return true;
            }

            async Task<bool> TargetsCurrent(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.TargetsCurrent) return false;

                if (target == null) return false;

                abilityManager.location = target.transform.position;
                abilityManager.target = target;
                await abilityManager.Cast(ability);

                return true;
            }

            async Task<bool> RandomLocation(SpecialAbility special, AbilityInfo ability)
            {
                if (special.targeting != SpecialTargeting.RandomLocation) return false;

                var maxDistance = ability.catalystInfo.range;
                var randomX = Random.Range(-maxDistance, maxDistance);
                var randomZ = Random.Range(-maxDistance, maxDistance);

                abilityManager.location = V3Helper.NearestNodePosition(abilityManager.transform.position + new Vector3(randomX, 0, randomZ));
                await abilityManager.Cast(ability);


                return true;
            }



            
        }

        async Task<bool> AbilityAssist()
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

                if (await Use(specialAbility, specialAbility.ability)) return true;


            }


            return false;
        }

        async Task<bool> Use(SpecialAbility special, AbilityInfo ability)
        {
            if (ability.abilityType != AbilityType.Use) return false;
            if (special.targeting != SpecialTargeting.Use) return false;

            await abilityManager.Cast(ability);

            return true;
        }
    }

}
