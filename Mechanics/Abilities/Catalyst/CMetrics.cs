using Architome.Enums;
using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        public Vector3 location, startingLocation, startDirectionRange, currentPosition;

        public float value, startingHeight, currentRange, liveTime, inertia, inertiaFallOff, distanceFromTarget;

        public CatalystEvent intervalTrigger;

        public float intervals;



        [Header("Growing Properties")]

        public Vector3 growthDirection;

        public float maxGrowth, growthSpeed, startSpeed;

        public float startDistance;


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
            get
            {
                return lockOn ? target.transform.position : location;
            }
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

        public void ResetStartingPosition()
        {
            startingLocation = catalyst.transform.position;
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
            var targetPosition = target ? target.transform.position : location;

            distanceFromTarget = V3Helper.Distance(targetPosition, catalyst.transform.position);

        }

        public float DistancePercent()
        {
            var targetLocation = target ? target.transform.position : location;

            var totalDistance = V3Helper.Distance(targetLocation, startingLocation);

            return currentRange / totalDistance;

        }
    }
}
