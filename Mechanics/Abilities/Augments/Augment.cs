using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome.Events;
using Language.Lua;

namespace Architome
{


    public class Augment : MonoActor
    {
        int id;

        #region Common Data

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
        public Action<Augment> OnRemove { get; set; }
        ArchEventHandler<AugmentEvent, AugmentEventData> events { get; set; }
        Augment FirstAugment
        {
            get
            {
                var augments = ability.GetComponentsInChildren<Augment>();

                return augments[0];
            }
        }

        #endregion

        #region Initiation
        bool dependenciesAcquired { get; set; }
        async void Start()
        {
            await GetDependencies();
            if (ability == null)
            {
                RemoveAugment();
                return;
            }
            HandleRestrictions();
        }

        protected override void Awake()
        {
            base.Awake();
            events = new(this);

        }
        async Task GetDependencies()
        {
            ability = GetComponentInParent<AbilityInfo>();

            try
            {
                var success = await UntilParentAugmentCompleted();

                if (!success)
                {
                    throw new Exception("Parent augment never completed initiation");
                }

                await ability.UntilInitiationComplete();

                if (this == null) return;

                augmentCataling = transform.parent.GetComponent<AugmentCataling>();
                entity = ability.entityInfo;

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
            catch (Exception e)
            {
                Defect.CreateIndicator(transform, "Augment initialization problem", e);
            }

        }

        async Task<bool> UntilParentAugmentCompleted()
        {

            var parent = transform.parent;
            var parentAugment = parent.GetComponent<Augment>();
            if (parentAugment == null) return true;

            var successful = await parentAugment.UntilDependenciesAcquired();

            return successful;
        }
        public async Task<bool> UntilDependenciesAcquired()
        {
            while (!dependenciesAcquired)
            {
                if (this == null) return false;
                await Task.Yield();
            }

            return true;
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
        async void UpdateDeathConditions(CatalystInfo catalyst)
        {
            var deathCondition = catalyst.GetComponent<CatalystDeathCondition>();
            await Task.Yield();

            deathCondition.conditions.Add(additiveDestroyConditions);

            await Task.Yield();

            deathCondition.conditions.Subtract(subtractiveDestroyConditions);
        }

        #endregion
        public Action AddListener(AugmentEvent eventType, Action<AugmentEventData> action, Component listener)
        {
            return events.AddListener(eventType, action, listener);
        }

        public Action AddiListenerCheck(AugmentEvent eventType, Action<AugmentEventData, List<bool>> action, Component listener) => events.AddListenerCheck(eventType, action, listener);

        public bool InvokeCheck(AugmentEvent eventType, AugmentEventData eventData) => events.InvokeCheck(eventType, eventData);
        public bool InvokeCheck(AugmentEvent eventType, AugmentEventData eventData, LogicType logicType) => events.InvokeCheck(eventType, eventData, logicType);

        #region Description
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
        #endregion

        #region Remove Handler
        bool removed { get; set; }

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
        #endregion

        #region Trigger Handler
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

        public bool CanAttachToAbility(AugmentEventData eventData)
        {
            if(!InvokeCheck(AugmentEvent.OnCanAttachToAbility, eventData, LogicType.NoFalse))
            {
                return false;
            }

            var childrenAugmentTypes = GetComponentsInChildren<AugmentType>();

            foreach(var childAugment in childrenAugmentTypes)
            {
                if (!childAugment.CanAttachToAbility(eventData)) return false;
            }

            return true;
        }
        #endregion

        public class AugmentEventData
        {
            public AugmentType augmentType;
            public Augment augment;
            public CatalystInfo activeCatalyst;
            public AbilityInfo ability;
            public bool active = true;
            public bool hasEnd { get; set; }

            public string errorMessage { get; private set; }

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

            public void SetErrorMessage(string errorMessage)
            {
                this.errorMessage = errorMessage;
            }
        }
    }
}
