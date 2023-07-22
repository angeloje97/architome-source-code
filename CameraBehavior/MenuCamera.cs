using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class MenuCamera : MonoBehaviour
    {
        MainMenuUI menuUI;

        public float travelTime = .25f;

        Transform target;

        bool isMoving;

        public bool _isMoving { get { return isMoving; } }

        [SerializeField] List<Transform> targetTransforms;
        [SerializeField] Transform defaultTransform;

        public void GetDependencies()
        {
            menuUI = MainMenuUI.active;

        }

        void Start()
        {
            GetDependencies();
            SetTarget(0);
        }


        public async void SetTarget(int targetIndex)
        {
            if (defaultTransform == null) return;
            var validIndex = targetTransforms != null && targetIndex >= 0 && targetIndex < targetTransforms.Count;

            var currentTarget = !validIndex ? defaultTransform : targetTransforms[targetIndex];
            target = currentTarget;

            var startposition = transform.position;
            var startRotation = transform.rotation;

            await ArchCurve.Smooth((float lerpValue) => {
                if (target != currentTarget) return;
                transform.SetPositionAndRotation(Vector3.Lerp(startposition, target.position, lerpValue),
                    Quaternion.Lerp(startRotation, target.rotation, lerpValue));

            }, CurveType.EaseInOut, travelTime);
        }

    }
}
