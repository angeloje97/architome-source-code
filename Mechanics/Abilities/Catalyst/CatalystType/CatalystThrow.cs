using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class CatalystThrow : MonoBehaviour
    {
        CatalystInfo catalyst;
        public GameObject target;
        public Vector3 location;
        public float tiltAmount;
        public float minDistance = 3f;
        public Quaternion startAngle;

        [Range(0, 1)]
        public float distanceOffset;

        bool usingSkillShotLocation;
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
            if (usingSkillShotLocation)
            {
                location = catalyst.metrics.targetLocation;
                return;
            }
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

            if(catalyst.abilityInfo.abilityType == AbilityType.SkillShot)
            {
                usingSkillShotLocation = true;
                SetGroundPosition();
            }
            
        }

        void SetGroundPosition()
        {
            var groundLayer = LayerMasksData.active.walkableLayer;
            var groundPosition = V3Helper.GroundPosition(catalyst.metrics.targetLocation, groundLayer);

            catalyst.metrics.location = groundPosition;


        }

        void TiltCatalyst()
        {
            transform.LookAt(location);
            var distance = V3Helper.Distance(transform.position, location);
            if (distance < minDistance) return;

            var angle = transform.eulerAngles;
            angle.x += -tiltAmount;
            transform.eulerAngles = angle;
            startAngle = transform.rotation;
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

            distancePercent = Mathf.Clamp(distancePercent, .01f, 1f);
            transform.rotation = transform.LerpLookAt(startAngle, location, distancePercent + distanceOffset);
        }

    }
}
