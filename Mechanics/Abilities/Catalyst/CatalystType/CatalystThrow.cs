using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystThrow : MonoBehaviour
    {
        CatalystInfo catalyst;
        public GameObject target;
        public Vector3 location;
        public float tiltAmount;
        public float minDistance = 3f;
        
        void Start()
        {
            GetDependencies();
            
            DisableLockOn();
            TiltCatalyst();
        }
        void Update()
        {
            UpdateLocation();
            HandleDirection();
        }

        void UpdateLocation()
        {
            location = target ? catalyst.target.transform.position : catalyst.location;
        }
        void DisableLockOn()
        {
            var lockOn = GetComponent<CatalystLockOn>();
            if (!lockOn) return;
            lockOn.disableLockOn = true;
        }

        void GetDependencies()
        {
            catalyst = GetComponent<CatalystInfo>();
            if (catalyst)
            {
                catalyst.OnNewTarget += HandleNewTarget;
                target = catalyst.target;
                UpdateLocation();
            }

            
        }

        void TiltCatalyst()
        {
            transform.LookAt(location);
            var distance = V3Helper.Distance(transform.position, location);
            if (distance < minDistance) return;

            var angle = transform.eulerAngles;
            angle.x += -tiltAmount;
            transform.eulerAngles = angle;
        }

        void HandleNewTarget(GameObject before, GameObject after)
        {
            target = after;
            TiltCatalyst();
        }

        void HandleDirection()
        {
            var distancePercent = catalyst.metrics.DistancePercent();
            if (distancePercent <= 0) return;

            transform.rotation = V3Helper.LerpLookAt(transform, location, distancePercent * .125f);
        }

    }
}
