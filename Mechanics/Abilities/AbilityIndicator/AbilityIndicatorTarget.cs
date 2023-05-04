using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class AbilityIndicatorTarget : AbilityIndicatorScaler
    {
        [Header("Target Properties")]
        public Transform target;
        public Transform transformSource;
        public Transform anchor;
        public Action<Transform> OnAcquireTarget;

        float catalystWidth;
        public float currentDistance;

        protected override void GetDependencies()
        {
            base.GetDependencies();
            transformSource = GetComponentInParent<EntityInfo>().transform;
            catalystWidth = useCatalystWidth ? ability.catalyst.transform.localScale.x : .5f;
        }

        public override async void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;
            if (!ability.targetLocked) return;

            target = ability.targetLocked.transform;
            OnAcquireTarget?.Invoke(target);

            var targetTransform = target.transform;

            SetProjector(true);

            while(ability.activated && ability.targetLocked != null)
            {
                var target2dPosition = targetTransform.transform.position;

                target2dPosition.y = transformSource.position.y;
                anchor.LookAt(target2dPosition);


                currentDistance = Vector3.Distance(transformSource.position, target.transform.position);
                if(currentDistance > ability.range)
                {
                    currentDistance = ability.range;
                }
                SetScale(Scale(currentDistance));

                await Task.Yield();
            }

            SetProjector(false);
        }
    }
}
