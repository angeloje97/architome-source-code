using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentType : MonoBehaviour
    {
        public Augment augment;
        protected AbilityInfo ability;

        public float value;
        public float valueContribution = 1f;

        public CatalystInfo activeCatalyst;
        protected CatalystHit activeHit;


        async void Start()
        {
            await GetDependencies();
            Debugger.Environment(329, $"Test");
        }
        protected async Task GetDependencies()
        {
            augment = GetComponent<Augment>();


            while (!augment.dependenciesAcquired)
            {
                await Task.Yield();
            }
            
            if (augment)
            {
                value = valueContribution * augment.info.value;
                ability = augment.ability;
            }
        }

        public virtual async Task<bool> Ability()
        {
            return true;
        }
        protected void EnableCatalyst()
        {
            augment.OnNewCatalyst += HandleNewCatlyst;

        }
        protected void EnableAugmentAbility()
        {
            if (ability.augmentAbilities == null)
            {
                ability.augmentAbilities = new();
            }

            ability.augmentAbilities.Add(this);

            augment.OnRemove += (Augment augment) =>
            {
                ability.augmentAbilities.Remove(this);
            };
        }
        protected void EnableCasting()
        {
            augment.ability.WhileCasting += WhileCasting;
            augment.OnRemove += (Augment augment) => {
                ability.WhileCasting -= WhileCasting;
            };
        }
        protected void EnableSuccesfulCast()
        {
            ability.OnSuccessfulCast += HandleSuccessfulCast;

            augment.OnRemove += (Augment augment) =>
            {
                ability.OnSuccessfulCast -= HandleSuccessfulCast;
            };
        }
        protected void EnableAbilityStartEnd()
        {
            ability.OnAbilityStartEnd += HandleAbility;

            augment.OnRemove += (Augment augment) => { ability.OnAbilityStartEnd -= HandleAbility; };
        }

        protected void EnableTasks()
        {
            augment.entity.taskEvents.OnTaskComplete += HandleTaskComplete;
            augment.OnRemove += (Augment augment) => {
                augment.entity.taskEvents.OnTaskComplete -= HandleTaskComplete;
            };

            augment.entity.taskEvents.OnStartTask += HandleStartTask;
        }
        public virtual void SetCatalyst(CatalystInfo catalyst, bool active)
        {
            if (active)
            {
                activeCatalyst = catalyst;
                activeHit = catalyst.GetComponent<CatalystHit>();

            }
            else
            {
                if (activeCatalyst == catalyst)
                {
                    activeCatalyst = null;
                    activeHit = null;
                }
            }
        }
        public virtual void HandleNewCatlyst(CatalystInfo catalyst)
        {

        }
        public virtual void WhileCasting(AbilityInfo ability)
        {

        }
        public virtual void HandleSuccessfulCast(AbilityInfo ability)
        {

        }
        public virtual void HandleAbility(AbilityInfo ability, bool start)
        {

        }

        public virtual void HandleTaskComplete(TaskEventData eventData)
        {

        }

        public virtual void HandleStartTask(TaskEventData eventData)
        {

        }
        public virtual string Description()
        {
            var result = "";
            return result;
        }

        public void TriggerAugment()
        {

        }
    }
}
