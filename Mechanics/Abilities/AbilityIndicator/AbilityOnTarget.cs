using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome.Indicator
{
    public class AbilityOnTarget : AbilityIndicator
    {
        [Header("On Target Properties")]
        public Vector3 targetLocation;

        public override async void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;
            SetProjector(true);
            await ability.HandleTargetLocation(StayOnTarget);
            SetProjector(false);
        }

        void StayOnTarget(Vector3 target)
        {

            var groundPosition = V3Helper.GroundPosition(target, groundLayer);
            transform.position = groundPosition;
        }
    }
}
