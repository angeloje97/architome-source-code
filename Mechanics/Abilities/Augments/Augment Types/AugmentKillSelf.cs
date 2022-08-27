using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentKillSelf : AugmentType
    {
        // Start is called before the first frame update
        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableSuccesfulCast();
        }

        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            augment.entity.Die();
        }

        public override string Description()
        {
            return "Caster dies when this ability is successfuly casted.";
        }
    }
}
