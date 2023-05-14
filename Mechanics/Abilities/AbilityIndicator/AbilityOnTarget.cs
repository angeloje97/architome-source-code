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


        bool firstIteration;
        float groundPositionTimer;
        float groundPosition;

        public override async void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;
            SetProjector(true);
            firstIteration = false;
            await ability.HandleTargetLocation(StayOnTarget);
            SetProjector(false);
        }

        void StayOnTarget(Vector3 target)
        {
            UpdateGroundPosition();
            target.y = groundPosition;


            transform.position = target;

            void UpdateGroundPosition()
            {
                if (!firstIteration)
                {
                    firstIteration = true;
                    groundPosition = target.y;
                }

                if(groundPositionTimer > 0)
                {
                    groundPosition -= Time.deltaTime;
                    return;
                }

                groundPositionTimer = 1f;

                groundPosition = V3Helper.GroundPosition(target, groundLayer).y;
            }
        }

    }
}
