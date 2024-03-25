using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class PopupContainer : MonoActor
    {
        public static PopupContainer active;
        Camera mainCamera => MainCamera3.active;

        protected override void Awake()
        {
            active = this;
        }

        public async void StickToTarget(Transform uiTrans, Transform worldTarget, Func<bool> predicate, bool targetValue = false)
        {
            if (mainCamera == null) return;
            var camera = mainCamera;

            await ArchAction.WaitUntil(() => {
                if (predicate() == targetValue) return targetValue;

                var targetPosition = mainCamera.WorldToScreenPoint(worldTarget.position);
                uiTrans.position = new Vector3(targetPosition.x, targetPosition.y, 0f);

                return predicate();
            }, targetValue);
        }
    }
}