using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System.Threading.Tasks;
using System;

namespace Architome
{
    public class CombatNoControl : CombatType
    {
        // Start is called before the first frame update
        new void GetDependencies()
        {
            base.GetDependencies();

            if (combat)
            {
                combat.OnSetFocus += OnSetFocus;
                OnDestroySelf += (CombatType type) => {
                    combat.OnSetFocus -= OnSetFocus;
                };
            }
        }
        void Start()
        {
            GetDependencies();
            OnCombatRoutine();
            TestTask();
        }

        async void TestTask()
        {
            var tasks = new List<Func<Task>>() {
                async () => {
                    await TestingTask(1);
                },
                async () => {
                    await TestingTask(2);
                },
                async () => {
                    await TestingTask(3);
                }
            };
            

            foreach(var task in tasks)
            {
                await task();
            }

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

            await HandleAttack();

            inRoutine = false;
        }

        

        void OnSetFocus(EntityInfo target)
        {
            autoAttacking = false;
            OnCombatRoutine();
        }

        protected override void HandleNewThreat(ThreatManager.ThreatInfo threatInfo)
        {
            autoAttacking = false;
        }

        public override void HandleRechargeAbility(AbilityInfo ability, AbilityCoolDown coolDown)
        {
            if (ability.isAttack) return;
            autoAttacking = false;
        }


        async Task UsingAbility()
        {
            await AbilityHarm();

            await AbilityAssist();

        }

        async Task TestingTask(int num)
        {
            await Task.Delay(1000);
            Debugger.InConsole(4832, $"Testing Task {num}");
        }

        async Task AbilityHarm()
        {
            foreach (var specialAbility in combat.specialAbilities)
            {
                var ability = specialAbility.ability;

                if (ability == null) continue;
                if (!ability.isHarming) continue;

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

                Debugger.Combat(6914, $"Random Target: {randomTarget}");

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
                var randomX = UnityEngine.Random.Range(-maxDistance, maxDistance);
                var randomZ = UnityEngine.Random.Range(-maxDistance, maxDistance);

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
