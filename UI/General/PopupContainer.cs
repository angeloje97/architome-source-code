using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private void LateUpdate()
        {
            
        }

        public async Task StickToTarget(Transform uiTrans, Transform worldTarget, Func<bool> predicate, bool targetValue = false, Vector3 offset = new Vector3())
        {
            if (mainCamera == null) return;
            var camera = mainCamera;

            var newParent = NewParent(uiTrans);

            await ArchAction.WaitUntil(() => {
                if (predicate() == targetValue) return targetValue;

                var targetPosition = mainCamera.WorldToScreenPoint(worldTarget.position);
                //uiTrans.position = new Vector3(targetPosition.x, targetPosition.y, 0f);
                newParent.position = new Vector3(targetPosition.x + offset.x, targetPosition.y + offset.y, 0f);

                return predicate();
            }, targetValue, useLateupdate: true);
        }

        Transform NewParent(Transform targetChild)
        {
            var parent = new GameObject($"{targetChild.name} Parent");

            parent.transform.SetParent(transform);
            parent.transform.position = new();

            targetChild.SetParent(parent.transform);
            targetChild.transform.position = new();

            return parent.transform;
        }
    }
}