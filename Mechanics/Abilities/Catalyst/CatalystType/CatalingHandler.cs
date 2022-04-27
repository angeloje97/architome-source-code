using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Architome.Enums;

namespace Architome
{
    public class CatalingHandler : CatalystProp
    {
        public GameObject cataling;
        public List<GameObject> targets;
        AbilityType catalingType;
        int catalingsReleased, catalingPool;
        public float interval, currentAngle;
        new void GetDependencies()
        {
            base.GetDependencies();

            if (catalyst == null) Destroy(gameObject);

            catalyst.OnCatalystDestroy += OnCatalystDestroy;
            catalyst.OnCatalystStop += OnCatalystStop;
            catalyst.OnHit += OnHit;

            catalingType = ability.cataling.catalingType;
            cataling = ability.cataling.catalyst;
            catalingPool = ability.cataling.releasePerInterval;
            interval = ability.cataling.interval;

            if (catalingPool <= 0)
            {
                catalingPool = 1;
            }

            if (interval <= 0)
            {
                interval = 1;
            }

        }
        void Start()
        {
            GetDependencies();

            if (ability.cataling.releaseCondition == ReleaseCondition.OnAwake)
            {
                Activate();
            }
        }

        void OnCatalystStop(CatalystKinematics kinematics)
        {
            if (ability.cataling.releaseCondition == ReleaseCondition.OnStop)
            {
                Activate();
            }
        }

        void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {
            if (ability.cataling.releaseCondition == ReleaseCondition.OnDestroy)
            {
                ActivateOnce();
            }
        }

        private void OnHit(GameObject target)
        {
            if (ability.cataling.releaseCondition == ReleaseCondition.OnHit)
            {
                ActivateOnce();
            }

        }

        async void Activate()
        {
            await Task.Delay((int) (ability.cataling.startDelay * 1000));
            do
            {
                bool released = false;
                for (int i = 0; i < catalingPool; i++)
                {
                    if (OnCatalingInterval(i))
                    {
                        released = true;
                    }
                }


                if (released)
                {
                    catalingsReleased++;
                    currentAngle += ability.cataling.rotationPerInterval;
                    catalyst.ReduceTicks();
                }



                await Task.Delay((int) (interval * 1000));

            } while (!catalyst.isDestroyed);
        }

        void ActivateOnce()
        {
            bool released = false;
            for (int i = 0; i < catalingPool; i++)
            {
                if (OnCatalingInterval(i))
                {
                    released = true;
                }
            }


            if (released)
            {
                catalingsReleased++;
                currentAngle += ability.cataling.rotationPerInterval;
                catalyst.ReduceTicks();
            }
        }

        bool OnCatalingInterval(int position)
        {
            if (catalingType == AbilityType.LockOn)
            {
                targets = catalyst.TargetableEntities(ability.cataling.targetFinderRadius, true);
                if (targets.Count == 0) return false;

                var percent = position + 1 / catalingPool;
                var index = (catalingsReleased + (percent * catalingPool)) % targets.Count;
                

                
                if (position >= targets.Count)
                {
                    ArchAction.Delay(() => ReleaseLockOn(targets[index]), position * .0625f);
                }
                else
                {
                    ReleaseLockOn(targets[index]);
                }

                return true;
            }
            else if (catalingType == AbilityType.SkillShot)
            {
                float percent = (float) position / catalingPool;
                var offset = percent * 360f;

                Debugger.InConsole(23489, $"cataling {position} angle is {percent}");

                ReleaseDirection(offset + currentAngle);

                return true;
            }
            
            return false;
        }

        public void ReleaseLockOn(GameObject target)
        {
            var catalingInfo = this.cataling.GetComponent<CatalystInfo>();
            catalingInfo.abilityInfo = ability;
            catalingInfo.target = target.gameObject;
            catalingInfo.isCataling = true;
            catalingInfo.requiresLockOnTarget = true;

            var cataling = Instantiate(this.cataling, transform.position, transform.localRotation);

            catalyst.OnCatalingRelease?.Invoke(catalyst, cataling.GetComponent<CatalystInfo>());
        }
        
        void ReleaseDirection(float positionY)
        {

            var catalingInfo = this.cataling.GetComponent<CatalystInfo>();
            catalingInfo.abilityInfo = ability;
            catalingInfo.isCataling = true;

            var cataling = Instantiate(this.cataling, transform.position, Quaternion.Euler(0, positionY, 0));

            catalyst.OnCatalingRelease?.Invoke(catalyst, cataling.GetComponent<CatalystInfo>());


            
        }
    }
}