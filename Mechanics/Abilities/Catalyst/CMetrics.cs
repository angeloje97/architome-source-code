using Architome.Enums;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Ionic.Crc;
using Language.Lua;

namespace Architome
{
    [Serializable]
    public class CMetrics
    {
        [SerializeField] CatalystInfo catalyst;
        [Serializable]
        public struct AccelerationBenchmark
        {
            [Range(0, 1)]
            public float benchmark;
            [Range(0, 1)]
            public float smoothness;
        }

        public GameObject target;

        public bool lockOn;

        public Vector3 location;

        public Vector3 startingLocation, startDirectionRange, currentPosition;

        public float value, startingHeight, currentRange, liveTime, inertia, inertiaFallOff, distanceFromTarget;

        public CatalystEvent intervalTrigger;

        public float intervals;



        [Header("Growing Properties")]

        public Vector3 growthDirection;

        public float maxGrowth, growthSpeed, startSpeed;

        public float startDistance;

        bool calculatingDistancePercent;
        public float distancePercent
        {
            get;
            private set;
        }


        [Header("Change of Speed")]

        public bool stops;

        public List<AccelerationBenchmark> accelBenchmarks;

        public void Initialize(CatalystInfo catalyst)
        {
            this.catalyst = catalyst;
            startingLocation = catalyst.transform.position;

            HandleInterval();
        }
        public Vector3 targetLocation
        {
            get; private set;
        }

        void HandleInterval()
        {
            if (intervals <= 0) return;
            if (intervalTrigger == CatalystEvent.OnInterval) return;

            bool intervalActive = false;

            catalyst.AddEventAction(intervalTrigger, () => {
                IntervalLoop();
            });

            async void IntervalLoop()
            {
                if (intervalActive) return;
                intervalActive = true;
                while (!catalyst.isDestroyed)
                {
                    catalyst.OnInterval?.Invoke(catalyst);
                    await Task.Delay((int)(1000 * intervals));
                }
            }
        }


        public void Update()
        {
            if (!catalyst) return;
            UpdateMetrics();
        }

        void UpdateMetrics()
        {
            currentPosition = catalyst.transform.position;
            currentRange = V3Helper.Distance(startingLocation, catalyst.transform.position);
            liveTime += Time.deltaTime;
            targetLocation = catalyst.requiresLockOnTarget ? target ? target.transform.position : location : location;

            distanceFromTarget = V3Helper.Distance(targetLocation, catalyst.transform.position);

            if (calculatingDistancePercent)
            {
                distancePercent = currentRange / V3Helper.Distance(targetLocation, startingLocation);
            }
        }

        public Vector3 TargetPosition()
        {
            if(targetLocation == new Vector3())
            {
                targetLocation = catalyst.requiresLockOnTarget ? target ? target.transform.position : location : location;
                
            }

            return targetLocation;
        }

        public float DistancePercent()
        {
            if (!calculatingDistancePercent)
            {
                calculatingDistancePercent = true;
                return 0f;
            }

            return distancePercent;
        }

        public void SetLocation(Vector3 newLocation)
        {
            location = newLocation;
        }
    }
}
