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
        public CatalystTarget catalystTarget;

        [Serializable]
        public class Info
        {
            public float value;
            public float valueContributionToAugment = 1f;
            public float generalRadius;


            public int ticks;
        }

        public Info info;

        public AbilityInfo ability;
        public AugmentCataling augmentCataling;
        public EntityInfo entity;

        public AugmentProp.DestroyConditions additiveDestroyConditions, subtractiveDestroyConditions;
        public AugmentProp.Restrictions additiveRestrictions, subtractiveRestrictions;


        public Action<CatalystInfo> OnNewCatalyst;
        public Action<Augment> OnRemove;

        public bool dependenciesAcquired;

        async void Start()
        {
            
            await GetDependencies();
            AdjustAbility(true);
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

            
            if (augmentCataling)
            {
                Action<CatalystInfo, CatalystInfo> action = (CatalystInfo original, CatalystInfo cataling) => {
                    OnNewCatalyst?.Invoke(cataling);
                };

                augmentCataling.OnCatalystRelease += action;
                OnRemove += (Augment augment) => { augmentCataling.OnCatalystRelease -= action; };
                catalystTarget = CatalystTarget.Cataling;
                dependenciesAcquired = true;
                return;
            }

            if (ability)
            {
                info.value = ability.value * info.valueContributionToAugment;

                Action<CatalystInfo> action = (CatalystInfo catalyst) => {
                    OnNewCatalyst?.Invoke(catalyst);
                };

                ability.OnCatalystRelease += action;

                OnRemove += (Augment augment) => {
                    ability.OnCatalystRelease -= action;
                };
                catalystTarget = CatalystTarget.Cataling;
            }

            dependenciesAcquired = true;
        }

        public void AdjustAbility(bool applying)
        {
            if (applying)
            {
                ability.ticksOfDamage += info.ticks;
            }
            else
            {
                ability.ticksOfDamage -= info.ticks;
            }
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
                var description = type.Description();
                if (description.Length > 0 && result.Length > 0)
                {
                    result += $"\n";
                }

                result += $"{description}";
            }
            return result;
        }

        public void RemoveAugment()
        {
            OnRemove?.Invoke(this);

            AdjustAbility(false);

            ArchAction.Yield(() => {
                Destroy(gameObject);
            });
        }
    }
}
