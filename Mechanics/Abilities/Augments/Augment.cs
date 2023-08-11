using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class Augment : MonoBehaviour
    {
        int id;

        public new string name;
        [Multiline]
        public string description;
        public Sprite icon;
        public CatalystTarget catalystTarget;

        [Serializable]
        public class Info
        {
            public float value;
            public float valueContributionToAugment = 1f;
            public float generalRadius;   
        }
        public bool spawnedByItem;
        public Info info;

        public AbilityInfo ability;
        public AbilityManager abilityManager;
        public AugmentCataling augmentCataling;
        public EntityInfo entity;

        public AugmentProp.DestroyConditions additiveDestroyConditions, subtractiveDestroyConditions;
        public AugmentProp.Restrictions additiveRestrictions, subtractiveRestrictions, originalRestrictions;


        public Action<CatalystInfo> OnNewCatalyst;
        public Action<Augment> OnRemove;
        public Action<AugmentEventData> OnAugmentTrigger;
        public Action<AugmentEventData> OnAugmentActivate;

        public bool dependenciesAcquired;

        async void Start()
        {
            
            await GetDependencies();
            HandleRestrictions();
        }

        private void Awake()
        {
            
        }
        async Task GetDependencies()
        {
            ability = GetComponentInParent<AbilityInfo>();
            
            augmentCataling = transform.parent.GetComponent<AugmentCataling>();
            entity = GetComponentInParent<EntityInfo>();

            while (ability.abilityManager == null)
            {
                await Task.Yield();
            }

            abilityManager = ability.abilityManager;

            
            if (augmentCataling)
            {
                Action<CatalystInfo, CatalystInfo> action = (CatalystInfo original, CatalystInfo cataling) => {
                    OnNewCatalyst?.Invoke(cataling);

                    UpdateDeathConditions(cataling);
                };

                augmentCataling.OnCatalystRelease += action;
                OnRemove += (Augment augment) => { augmentCataling.OnCatalystRelease -= action; };
                catalystTarget = CatalystTarget.Cataling;
                dependenciesAcquired = true;

                info.value = augmentCataling.value * info.valueContributionToAugment;
                return;
            }

            if (ability)
            {
                info.value = ability.value * info.valueContributionToAugment;

                Action<CatalystInfo> action = (CatalystInfo catalyst) => {
                    OnNewCatalyst?.Invoke(catalyst);
                    UpdateDeathConditions(catalyst);
                };

                ability.OnCatalystRelease += action;

                OnRemove += (Augment augment) => {
                    ability.OnCatalystRelease -= action;
                };
                catalystTarget = CatalystTarget.Catalyst;
            }

            dependenciesAcquired = true;
        }
        public void HandleRestrictions()
        {
            var firstAugment = FirstAugment;
            if (firstAugment == this)
            {
                originalRestrictions = ability.restrictions.Copy();
            }
            else
            {
                originalRestrictions = firstAugment.originalRestrictions.Copy();
            }

            ability.OnUpdateRestrictions += OnUpdateRestrictions;

            OnRemove += (Augment augment) => {
                ability.OnUpdateRestrictions -= OnUpdateRestrictions;
                ability.OnUpdateRestrictions?.Invoke(ability);
            };

            ability.OnUpdateRestrictions?.Invoke(ability);

            async void OnUpdateRestrictions(AbilityInfo ability)
            {
                if (FirstAugment == this)
                {
                    ability.restrictions = originalRestrictions.Copy();
                }

                await Task.Yield();

                ability.restrictions.Add(additiveRestrictions);

                await Task.Yield();

                ability.restrictions.Subtract(subtractiveRestrictions);
            }
        }
        Augment FirstAugment
        {
            get
            {
                var augments = ability.GetComponentsInChildren<Augment>();

                return augments[0];
            }
        }
        async void UpdateDeathConditions(CatalystInfo catalyst)
        {
            var deathCondition = catalyst.GetComponent<CatalystDeathCondition>();
            await Task.Yield();

            deathCondition.conditions.Add(additiveDestroyConditions);

            await Task.Yield();

            deathCondition.conditions.Subtract(subtractiveDestroyConditions);
        }
        public string Description()
        {
            var result = "";
            

            return result;
        }
        public string TypeDescription()
        {
            var result = "";

            foreach (var type in GetComponents<AugmentType>())
            {
                var description = type.ToString();
                if (description.Length > 0 && result.Length > 0)
                {
                    result += $"\n";
                }

                result += $"{description}";
            }
            return result;
        }
        public async void RemoveAugment()
        {
            OnRemove?.Invoke(this);


            await Task.Yield();


            Destroy(gameObject);
            
        }
        public void TriggerAugment(AugmentEventData eventData)
        {
            eventData.eventTrigger = AugmentEvent.OnAugmentTrigger;
            OnAugmentTrigger?.Invoke(eventData);
        }

        public void ActivateAugment(AugmentEventData eventData)
        {
            eventData.eventTrigger = AugmentEvent.OnAugmentActive;
            OnAugmentActivate?.Invoke(eventData);
        }

        public class AugmentEventData
        {
            public AugmentType augmentType;
            public Augment augment;
            public CatalystInfo activeCatalyst;
            public bool active = true;
            public AugmentEvent eventTrigger;

            public async Task EndActivation()
            {
                while (active) await Task.Yield();
            }

            public AugmentEventData(AugmentType source)
            {
                augmentType = source;
                augment = augmentType.augment;
                activeCatalyst = source.activeCatalyst;
            }
        }
    }
}
