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
        public int managerIndex;

        [SerializeField] float speed;
        [SerializeField] float acceleration;
        [SerializeField] bool stopped;
        [SerializeField] bool maxSpeed;

        bool disabled { get; set; }
        bool disableTranslation;

        public void DisableKinematics()
        {
            disabled = true;
        }

        public void SetKinematics(CatalystInfo catalyst)
        {
            this.catalyst = catalyst;

            HandleStartKinematics();
            //HandleJobs();
            //HandleCatalystKinematics();
        }

        async void HandleJobs()
        {
            var manager = CatalystManager.active;
            if (!manager) return;

            disableTranslation = true;

            await Task.Yield();

            if (disabled) return;
            var trans = catalyst.transform;

            managerIndex = manager.AddCatalyst(catalyst);
            manager.OnRemoveCatalyst += HandleRemoveCatalyst;
            manager.BeforeCreateJob += HandleBeforeCreateJob;

            Debugger.Combat(7549, $"Is catalyst destroyed before yielding to jobs? :{catalyst.isDestroyed}");
            while (!catalyst.isDestroyed)
            {
                await Task.Yield();
            }


            manager.OnRemoveCatalyst -= HandleRemoveCatalyst;
            manager.BeforeCreateJob -= HandleBeforeCreateJob;
            manager.RemoveCatalyst(catalyst, managerIndex);

            void HandleRemoveCatalyst(CatalystInfo catalyst, int index)
            {
                if (managerIndex < index) return;
                managerIndex--;
            }

            void HandleBeforeCreateJob(CatalystManager manager)
            {
                manager.speeds[managerIndex] = speed;
                manager.directions[managerIndex] = trans.forward;
                manager.transforms[managerIndex] = trans;
            }

        }

        void HandleStartKinematics()
        {
            try
            {
                if (!catalyst.isCataling)
                {
                    if (catalyst.abilityInfo.abilityType == AbilityType.Use) return;
                    if (catalyst.abilityInfo.abilityType == AbilityType.Spawn) return;

                }
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

        public void Update()
        {
            if (!catalyst) return;
            if (disabled) return;
            if (catalyst.isDestroyed) return;

            HandleStop();
            HandleMaxSpeed();
            if (disableTranslation) return;

            HandleAcceleration(true);
            catalyst.transform.Translate(speed * Time.deltaTime * Vector3.forward);
            HandleAcceleration(true);
        }


        async void HandleCatalystKinematics()
        {
            var condition = new ArchCondition() { isMet = !catalyst.isDestroyed };

            await Task.Delay(125);

            while (!catalyst.isDestroyed)
            {
                
                if (disabled)
                {
                    await Task.Yield();
                    continue;    
                }

                catalyst.metrics.currentPosition = catalyst.transform.position;
                HandleStop();
                HandleAcceleration();
                HandleMaxSpeed();

                catalyst.transform.Translate(speed * Time.deltaTime * Vector3.forward);
                await Task.Yield();
            }

            //ArchAction.UpdateWhile(() => {

            //    condition.isMet = !catalyst.isDestroyed;


            //    if (condition.isMet == false) return;

                

            //}, condition);
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

        void HandleAcceleration(bool isHalf = false)
        {
            if (isHalf)
            {
                speed += Time.deltaTime * acceleration * .5f;
            }
            else
            {
                speed += Time.deltaTime * acceleration;

            }

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