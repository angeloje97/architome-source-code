using System.Collections;
using UnityEngine;
using Architome.Enums;
using System.Text;

namespace Architome
{
    public class AugmentTriggerListener : AugmentType
    {

        [Header("Trigger Listener Properties")]
        public bool onlyOnReset;
        async void Start()
        {
            await base.GetDependencies(() => {
                EnableAugmentTrigger();
            });
        }

        public override void HandleAugmentTrigger(Augment augment)
        {
            if (augment.ability.abilityType != AbilityType.Use) return;
            var augmentEvent = new Augment.AugmentEventData(this);

            if (onlyOnReset && augment.amountsTriggered != augment.amountToReset) return;

            augment.ability.HandleAbilityType();
            augment.ActivateAugment(augmentEvent);
        }

        protected override string Description()
        {
            var val = new StringBuilder("Activates ability ");

            if (onlyOnReset)
            {
                val.Append("upon other augments reaching max trigger amounts");
            }
            else
            {
                val.Append("upon other augments getting triggered");
            }

            return val.ToString();
        }
    }
}