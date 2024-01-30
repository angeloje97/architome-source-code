using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome.Events;

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
        ArchEventHandler<AugmentEvent, AugmentEventData> events { get; set; }

        bool dependenciesAcquired { get; set; }
        bool removed;


        async void Start()
        {
            await GetDependencies();
            if(ability == null)
            {
                RemoveAugment();
                return;
            }
            HandleRestrictions();
        }

        private void Awake()
        {
            events = new(this);

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

                augmentCataling.AddListener(AugmentCatalingEvent.OnReleaseCataling, ((AugmentEventData, CatalystInfo, CatalystInfo) tuple) => {
                    OnNewCatalyst?.Invoke(tuple.Item3);
                    UpdateDeathConditions(tuple.Item3);
                }, this);

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

            await Task.Delay(125);
            if (this == null) return;

            dependenciesAcquired = true;
            ArchAction.Delay(() => {
                events.Invoke(AugmentEvent.OnAttach, new(this));
            }, .125f);
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

        public Action AddListener(AugmentEvent eventType, Action<AugmentEventData> action, Component listener)
        {
            return events.AddListener(eventType, action, listener);
        }

        public async Task UntilDependenciesAcquired()
        {
            while (!dependenciesAcquired) await Task.Yield();
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
            events.Invoke(AugmentEvent.OnRemove, new(this));
            OnRemove?.Invoke(this);
            if (removed) return;
            removed = true;
            await Task.Yield();


            Destroy(gameObject);
            
        }
        private void OnDestroy()
        {
            removed = true;
        }

        public int amountsTriggered;
        public bool resetTriggerAtAmount;
        public int amountToReset;

        public void TriggerAugment(AugmentEventData eventData, bool effectsOnly = false)
        {
            eventData.eventTrigger = AugmentEvent.OnAugmentTrigger;

            if (!effectsOnly)
            {
                amountsTriggered++;
            }

            events.Invoke(AugmentEvent.OnAugmentTrigger, eventData);
            

            if (!effectsOnly)
            {
                abilityManager.HandleTriggerAugment(this);
            }

            if (resetTriggerAtAmount && amountsTriggered >= amountToReset)
            {
                amountsTriggered = 0;
                events.Invoke(AugmentEvent.OnResetTriggerAmount, eventData);
            }
        }

        public void ActivateAugment(AugmentEventData eventData)
        {
            eventData.eventTrigger = AugmentEvent.OnAugmentActive;
            events.Invoke(AugmentEvent.OnAugmentActive, eventData);
        }

        public class AugmentEventData
        {
            public AugmentType augmentType;
            public Augment augment;
            public CatalystInfo activeCatalyst;
            public bool active = true;
            public bool hasEnd { get; set; }
            public AugmentEvent eventTrigger;

            public async Task EndActivation()
            {
                if (!hasEnd)
                {
                    await Task.Delay(2000);
                    return;
                }

                while (active) await Task.Yield();

            }

            public AugmentEventData(AugmentType source)
            {
                augmentType = source;
                augment = augmentType.augment;
                activeCatalyst = source.activeCatalyst;
            }

            public AugmentEventData(Augment augment)
            {
                this.augment = augment;
            }
        }
    }
}
