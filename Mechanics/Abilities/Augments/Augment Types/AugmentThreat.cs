using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine;

namespace Architome
{
    public class AugmentThreat : AugmentType
    {
        protected override void GetDependencies()
        {
            EnableCatalyst();
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnDamage += (CatalystInfo catalyst, EntityInfo target) => {
                target.combatEvents.InvokeThreat(eThreatEvent.OnPingThreat, new(catalyst.entityInfo, target, value));
            };
        }

        protected override string Description()
        {
            var result = "";


            result += "Upon damaging a target with this ability's catalyst: Increase the threat that the target has on the caster by ";

            augment = GetComponent<Augment>();

            if (augment.ability)
            {
                result += $"{value} value";
            }
            else
            {
                result += $"{augment.info.valueContributionToAugment * 100}% of the ability's value.";
            }

            return result;
        }


        // Update is called once per frame

    }
}
