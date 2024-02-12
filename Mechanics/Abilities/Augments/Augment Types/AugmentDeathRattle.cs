using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using TMPro.EditorUtilities;

namespace Architome
{
    public class AugmentDeathRattle : AugmentType
    {
        

        protected override void GetDependencies()
        {
            if (augment.ability.abilityType != AbilityType.Use) return;

            var entity = augment.entity;

            if (entity)
            {
                entity.OnLifeChange += OnEntityLifeChange;
            }
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
