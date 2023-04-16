using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AbilityIndicatorTarget : AbilityIndicator
    {
        [Header("Target Properties")]
        public Transform target;
        public Transform transformSource;
        public Action<Transform> OnAcquireTarget;

        protected override void GetDependencies()
        {
            base.GetDependencies();
            transformSource = GetComponentInParent<EntityInfo>().transform;
        }

        public override void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;
            if (!ability.targetLocked) return;

            target = ability.targetLocked.transform;
            OnAcquireTarget?.Invoke(target);

            var targetTransform = target.transform;


            while(ability.activated && ability.targetLocked != null)
            {
                var target2dPosition = targetTransform.transform.position;
                target2dPosition.y = transformSource.position.y;
                var direction = V3Helper.LerpLookAtWithAxis(transformSource, Vector3.up, target2dPosition, 1);
                var distance = Vector3.Distance(transformSource.position, target2dPosition);
            }
        }
    }
}
