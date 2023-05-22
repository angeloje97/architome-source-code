using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Architome.Enums;

namespace Architome.Indicator
{
    public class AbilityLookTarget : AbilityScaler
    {
        [Header("Target Properties")]
        public Transform target;
        public Vector3 targetLocation;
        public Transform transformSource;
        public Transform anchor;
        public Action<Transform> OnAcquireTarget;
        public bool ignoreDistance;

        public float currentDistance;

        bool usesLockOn;

        protected override void GetDependencies()
        {
            base.GetDependencies();
            transformSource = GetComponentInParent<EntityInfo>().transform;
            catalystWidth = useCatalystWidth ? ability.catalyst.transform.localScale.x : .5f;

            if(ability.abilityType == AbilityType.LockOn)
            {
                usesLockOn = true;
            }
        }

        public override async void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;

            if (ability.targetLocked)
            {
                target = ability.targetLocked.transform;
            }


            SetProjector(true);

            await ability.HandleTargetLocation((Vector3 position) => {
                var target2dPosition = position;
                target2dPosition.y = transformSource.position.y;

                anchor.LookAt(target2dPosition);

                currentDistance = V3Helper.Distance(transform.position, targetLocation);

                if(currentDistance > ability.range || ignoreDistance)
                {
                    if(ability.range != -1)
                    {
                        currentDistance = ability.range;
                    }
                }

                SetScale(Scale(currentDistance));
            });

            SetProjector(false);
        }

    }
}
