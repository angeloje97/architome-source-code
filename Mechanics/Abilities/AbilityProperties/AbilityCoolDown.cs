using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class AbilityCoolDown
    {
        AbilityInfo ability;
        public float timer;
        public float timePerCharge = 1;
        public float progress = 1;
        public bool isActive;
        public bool globalCoolDownActive;
        public bool usesGlobal;
        public bool maxChargesOnStart = true;
        public bool interruptConsumesCharge;
        public int charges;
        public int maxCharges = 1;

        EntityInfo entity;
        float currentHaste;
        public void Initiate(AbilityInfo ability)
        {
            this.ability = ability;
            if (interruptConsumesCharge)
            {
                ability.OnInterrupt += OnInterrupt;
            }

            entity = ability.entityInfo;

            entity.OnChangeStats += OnChangeStats;

            ability.OnRemoveAbility += delegate (AbilityInfo ability)
            {
                entity.OnChangeStats -= OnChangeStats;
            };

            currentHaste = entity.stats.haste;

            ability.OnReadyCheck += OnReadyCheck;
            ability.OnSuccessfulCast += OnSuccesfulCast;

            if (maxChargesOnStart)
            {
                charges = maxCharges;
            }

            HandleCooldownTimer();
            HandleGlobalCoolDown();
        }
        void OnChangeStats(EntityInfo entity)
        {
            var difference = entity.stats.haste - currentHaste;
            currentHaste = entity.stats.haste;
            if (!isActive) return;

            timer -= (timePerCharge * difference);

            if (timer < 0)
            {
                timer = 0;
            }
        }
        public void HandleGlobalCoolDown()
        {
            if (!usesGlobal) return;

            var abilityManager = ability.abilityManager;

            if (abilityManager)
            {
                abilityManager.OnGlobalCoolDown += HandleGlobalCoolDown;

                ability.OnSuccessfulCast += (AbilityInfo ability) => {
                    abilityManager.OnGlobalCoolDown?.Invoke(this.ability);
                };
            }

            async void HandleGlobalCoolDown(AbilityInfo other)
            {
                if (globalCoolDownActive) return;
                globalCoolDownActive = true;
                float timer = 1 - Haste();

                while(timer > 0)
                {
                    await Task.Yield();

                    if(charges > 0)
                    {
                        progress = 1 - timer;
                    }
                    timer -= Time.deltaTime;
                }

                if(charges > 0)
                {
                    progress = 1;
                }

                globalCoolDownActive = false;
            }
        }
        void OnInterrupt(AbilityInfo ability)
        {
            if (charges > 0)
            {
                charges -= 1;
            }

            HandleCooldownTimer();
        }
        void OnReadyCheck(AbilityInfo ability, List<(string, bool)> checks)
        {
            if(charges <= 0)
            {
                checks.Add(("No Charges", false));
            }

            if (globalCoolDownActive)
            {
                checks.Add(("Global Cooldown is Active", false));
            }
            //andChecks.Add(("Sufficient charges", charges > 0));
        }
        void OnSuccesfulCast(AbilityInfo ability)
        {
            charges--;

            HandleCooldownTimer();
        }
        float Haste()
        {
            return currentHaste;

        }
        async void HandleCooldownTimer()
        {
            if (isActive) return;
            if (charges >= maxCharges) return;

            isActive = true;

            timer = timePerCharge - timePerCharge * Haste();

            while (charges < maxCharges)
            {
                await Task.Yield();
                timer -= Time.deltaTime;

                if (!globalCoolDownActive || charges < 1)
                {
                    progress = 1 - (timer / timePerCharge);

                }


                if (timer < 0)
                {
                    timer = timePerCharge;
                    charges++;
                }
            }

            progress = 1;

            isActive = false;
        }
        public void SetCharges(int newCharges)
        {
            this.charges = newCharges;
        }
    }
}
