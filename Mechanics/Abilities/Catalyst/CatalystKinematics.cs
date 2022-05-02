using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
using Architome.Enums;

namespace Architome
{
    [Serializable]
    public class CatalystKinematics
    {
        // Start is called before the first frame update
        CatalystInfo catalyst;

        [SerializeField] float speed;
        [SerializeField] float acceleration;
        [SerializeField] bool stopped;
        [SerializeField] bool maxSpeed;




        public void SetKinematics(CatalystInfo catalyst)
        {
            this.catalyst = catalyst;

            HandleStartKinematics();
            HandleCatalystKinematics();
        }

        void HandleStartKinematics()
        {
            try
            {
                if (catalyst.abilityInfo.abilityType == AbilityType.Use) return;
                if (catalyst.abilityInfo.abilityType == AbilityType.Spawn && !catalyst.isCataling) return;
                if (catalyst.metrics.stops)
                {
                    speed = catalyst.speed;

                    acceleration = CalculateDecelleration();
                }
                else if (catalyst.metrics.accelBenchmarks.Count > 0)
                {
                    speed = catalyst.metrics.startSpeed;
                }
                else
                {
                    speed = catalyst.speed;
                }
            }
            catch
            {
                catalyst.transform.SetParent(CatalystManager.active.defectiveCatalysts);
            }
        }

        float CalculateDecelleration()
        {
            float distance;
            if (catalyst.target)
            {
                distance = V3Helper.Distance(catalyst.target.transform.position, catalyst.transform.position);
            }
            else
            {
                distance = V3Helper.Distance(catalyst.location, catalyst.transform.position);
            }

            if (distance > catalyst.range)
            {
                distance = catalyst.range;
            }

            return -((catalyst.speed * catalyst.speed) / (2 * distance));

        }


        async void HandleCatalystKinematics()
        {
            var condition = new ArchCondition() { isMet = !catalyst.isDestroyed };

            await Task.Delay(125);

            ArchAction.UpdateWhile(() => {

                condition.isMet = !catalyst.isDestroyed;


                if (condition.isMet == false) return;

                HandleStop();
                HandleAcceleration();
                HandleMaxSpeed();

                catalyst.transform.Translate(Vector3.forward * speed * Time.deltaTime);

            }, condition);
        }

        

        void HandleStop()
        {
            if (stopped) return;


            if(speed <= 0)
            {
                stopped = true;

                catalyst.OnCatalystStop?.Invoke(this);
            }
        }

        void HandleAcceleration()
        {
            speed += Time.deltaTime * acceleration;

            if (speed > catalyst.speed)
            {
                acceleration = 0f;
                speed = catalyst.speed;
            }

            if (speed < 0)
            {
                acceleration = 0f;
                speed = 0f;
            }
        }

        void HandleMaxSpeed()
        {
            if (maxSpeed) return;

            if (speed >= catalyst.speed)
            {
                catalyst.OnCatalystMaxSpeed?.Invoke(this);
                maxSpeed = true;
            }
        }

        
    }

}