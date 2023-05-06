using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AbilityIndicatorTarget : AbilityIndicatorScaler
    {
        [Header("Target Properties")]
        public Transform target;
        public Vector3 targetLocation;
        public Transform transformSource;
        public Transform anchor;
        public Action<Transform> OnAcquireTarget;

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

            while(ability.activated)
            {
                targetLocation = Location();
                var target2dPosition = targetLocation;

                target2dPosition.y = transformSource.position.y;
                anchor.LookAt(target2dPosition);


                currentDistance = V3Helper.Distance(transformSource.position, targetLocation);
                if(currentDistance > ability.range)
                {
                    if(ability.range != -1)
                    {
                        currentDistance = ability.range;
                    }

                }
                SetScale(Scale(currentDistance));

                await Task.Yield();
            }

            SetProjector(false);
        }

        public Vector3 Location()
        {

            if (usesLockOn)
            {
                return target.position;
            }
            else
            {
                return ability.locationLocked;
            }
            
        }
    }
}
