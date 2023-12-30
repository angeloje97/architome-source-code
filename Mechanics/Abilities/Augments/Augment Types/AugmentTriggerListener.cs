using System.Collections;
using UnityEngine;

namespace Architome
{
    public class AugmentTriggerListener : AugmentType
    {

        // Use this for initialization
        async void Start()
        {
            await base.GetDependencies(() => {
                EnableAugmentTrigger();
            });
        }

        public override void HandleAugmentTrigger(Augment augment)
        {

        }
    }
}