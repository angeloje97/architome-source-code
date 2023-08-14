using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentTracking : AugmentType
    {
        [Header("Tracking Properties")]
        public TrackingType trackingType;
        public float trackingInterpolation;

        public float catalystSpeed;

        public enum TrackingType
        {
            Predict,
            Track
        }


        bool predicting;
        bool tracking;
        
        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableCasting();

            catalystSpeed = augment.ability.catalystInfo.speed;
        }

        public override async void WhileCasting(AbilityInfo ability)
        {
            if (ability.targetLocked == null) return;
            if (predicting || tracking) return;

            var eventData = new Augment.AugmentEventData(this) 
            { 
                active = true,
                hasEnd = true,
            };

            augment.ActivateAugment(eventData);

            await TrackTarget();
            await PredictTarget();

            eventData.active = false;

            async Task TrackTarget()
            {
                if (trackingType != TrackingType.Track) return;
                if (tracking) return;
                tracking = true;

                ability.locationLocked = ability.targetLocked.transform.position;

                while (ability.isCasting)
                {
                    ability.locationLocked = Vector3.Lerp(ability.location, ability.targetLocked.transform.position, trackingInterpolation);
                    await Task.Yield();
                }

                tracking = false;
            }

            async Task PredictTarget()
            {
                if (trackingType != TrackingType.Predict) return;
                if (predicting) return;
                if (ability.targetLocked == null) return;

                var target = ability.targetLocked.transform;

                predicting = true;

                var movement = target.GetComponentInChildren<Movement>();

                var sourceTrans = ability.entityObject.transform;

                while (ability.isCasting)
                {
                    var distance = V3Helper.Distance(sourceTrans.position, target.position);
                    var travelTime = distance / catalystSpeed;

                    ability.locationLocked = target.position + (travelTime * movement.velocity);

                    await Task.Yield();
                }

                predicting = false;
            }
        }

        protected override string Description()
        {
            var result = "";

            if (trackingType == TrackingType.Track)
            {
                result += $"Tracks a target that the ability is aiming at with a {trackingInterpolation * 100} tracking interlopation.";
            }

            if (trackingType == TrackingType.Predict)
            {
                result += $"Predicts where to aim the catalyst based on catalyst speed and the movement speed of the ability's target.";
            }

            return result;
        }

    }
}
