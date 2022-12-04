using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentActivateAbility : AugmentType 
    {
        [Header("Activate Ability Properties")]
        public AbilityInfo abilityToActivate;
        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableSuccesfulCast();
        }

        public override string Description()
        {
            return $"Upon successfully casting. This ability will activate ${abilityToActivate}";
        }


        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            base.HandleSuccessfulCast(ability);
            if (!abilityToActivate) return;
            abilityToActivate.targetLocked = ability.targetLocked;
            abilityToActivate.target = ability.target;
            abilityToActivate.location = ability.location;
            abilityToActivate.locationLocked = ability.locationLocked;

            abilityToActivate.HandleAbilityType();
        }
    }
}
