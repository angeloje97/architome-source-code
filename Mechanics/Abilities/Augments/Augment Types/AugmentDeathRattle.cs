using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentDeathRattle : AugmentType
    {
        // Start is called before the first frame update
        async void Start()
        {
            await GetDependencies(() => {
                if (augment.ability.abilityType != AbilityType.Use) return;

                var entity = augment.entity;

                if (entity)
                {
                    entity.OnLifeChange += OnEntityLifeChange;
                }
            });
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void OnEntityLifeChange(bool isAlive)
        {
            if (isAlive) return;
            augment.ability.HandleAbilityType();
            augment.TriggerAugment(new(this));
        }

        protected override string Description()
        {

            var result = "This ability will be activated when the caster dies";

            return result;
        }
    }
}
