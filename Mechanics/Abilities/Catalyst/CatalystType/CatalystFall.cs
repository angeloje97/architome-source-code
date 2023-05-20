using Architome.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace Architome
{
    public class CatalystFall : MonoBehaviour
    {
        CatalystInfo catalyst;
        public float startingHeight;
        public Vector3 targetLocation;
        public LayerMask groundLayer;
        public Collider catalystCollider;
        public bool canManipulateCollider;
        private void Start()
        {
            GetDependencies();
            DisableLockOn();
            Reposition();
            ManipulateCollider();
        }

        private void Update()
        {
            TrackPosition();
        }

        void GetDependencies()
        {
            catalyst = GetComponent<CatalystInfo>();
            groundLayer = LayerMasksData.active.walkableLayer;
            catalystCollider = GetComponent<Collider>();
        }

        void DisableLockOn()
        {
            var lockOn = GetComponent<CatalystLockOn>();
            if (lockOn == null) return;
            lockOn.disableLockOn = true;
        }

        void TrackPosition()
        {
            if (catalyst == null) return;
            targetLocation = catalyst.metrics.targetLocation;
            var pos = targetLocation;
            pos.y = transform.position.y;
            transform.position = pos;

        }

        void Reposition()
        {
            if (catalyst == null) return;
            targetLocation = catalyst.metrics.TargetPosition();

            var position = targetLocation;
            position.y = position.y + startingHeight;

            var groundPosition = V3Helper.GroundPosition(catalyst.metrics.targetLocation, groundLayer);
            catalyst.metrics.SetLocation(groundPosition);


            transform.position = position;
            transform.LookAt(targetLocation);
            catalyst.ResetStartingPosition();
        }

        async void ManipulateCollider()
        {
            if (!canManipulateCollider) return;
            if (catalystCollider == null) return;
            if (catalyst == null) return;

            catalystCollider.enabled = false;
            await Task.Yield();


            while(catalyst.metrics.distanceFromTarget >= 5f)
            {
                await Task.Yield();
            }

            catalystCollider.enabled = true;

        }
    }
}
