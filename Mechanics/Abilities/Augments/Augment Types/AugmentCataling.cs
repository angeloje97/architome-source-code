using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentCataling : AugmentType
    {
        public GameObject cataling;
        public CatalystInfo catalingInfo;
        public AbilityType catalingType;
        public CatalystEvent releaseCondition;

        public AugmentProp.DestroyConditions catalingDestroyConditions;

        public int releasePerInterval;
        public float interval, radius, rotationPerInterval, startDelay;
        public bool combatInclusive;

        

        public Action<CatalystInfo, CatalystInfo> OnCatalystRelease; // (original catalyst, cataling)

        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            catalingInfo = cataling.GetComponent<CatalystInfo>();

            EnableCatalyst();
        }
        void Update()
        {
        
        }
        

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            if (releaseCondition == CatalystEvent.OnInterval) return;
            if (releaseCondition == CatalystEvent.OnCatalingRelease) return;

            catalyst.AddEventAction(releaseCondition, () => ActivateCataling(catalyst));


        }

        public override string Description()
        {
            var catalingName = ArchString.CamelToTitle(cataling.name);
            var catalingType = ArchString.CamelToTitle(this.catalingType.ToString());

            if (augment)
            {
                return $"Releases {releasePerInterval} {catalingType} {catalingName} every {interval} seconds with a {ArchString.FloatToSimple(value, 0)} value";

            }
            else
            {
                return $"Releases {releasePerInterval} {catalingType} {catalingName} every {interval} seconds with a {valueContribution * 100}% ability value";
            }

        }
        async void ActivateCataling(CatalystInfo catalyst)
        {
            await Task.Delay((int)(startDelay * 1000));

            int catalingsReleased = 0;
            float currentAngle = 0f;
            var targets = new List<GameObject>();

            FindTargets();

            var eventData = new Augment.AugmentEventData(this) { active = true };
            augment.ActivateAugment(eventData);
            



            do
            {
                bool released = false;

                for (int i = 0; i < releasePerInterval; i++)
                {
                    if (OnCatalingInterval(i))
                    {
                        released = true;
                        SetCatalyst(catalyst, true);
                        SetCatalyst(catalyst, false);
                    }
                }

                if (released)
                {
                    catalingsReleased++;
                    currentAngle += rotationPerInterval;
                    augment.TriggerAugment(eventData);
                    catalyst.ReduceTicks();
                }

                await Task.Delay((int)(1000 * interval));
            } while (!catalyst.isDestroyed);

            eventData.active = false;

            bool OnCatalingInterval(int position)
            {
                if (HandleLockOn()) return true;
                if (HandleUse()) return true;
                if (HandleFreeCast()) return true;

                return false;

                bool HandleLockOn()
                {
                    if (catalingType != AbilityType.LockOn) return false;

                    if (targets.Count == 0) return false;

                    var percent = position + 1 / releasePerInterval;

                    var index = (catalingsReleased + (percent * releasePerInterval)) % targets.Count;

                    if (position >= targets.Count)
                    {
                        ArchAction.Delay(() => ReleaseLockOn(targets[index]), position * 0.625f);
                    }
                    else
                    {
                        ReleaseLockOn(targets[index]);
                    }

                    return true;

                    void ReleaseLockOn(GameObject target)
                    {
                        catalingInfo.target = target;
                        catalingInfo.requiresLockOnTarget = true;

                        var newInfo = catalyst.ReleaseCataling(catalingInfo, catalyst.transform.rotation);

                        OnCatalystRelease?.Invoke(catalyst, newInfo);

                        ArchAction.Yield(() => {
                            newInfo.value = this.value;
                            newInfo.gameObject.AddComponent<CatalystLockOn>();
                            newInfo.GetComponent<CatalystDeathCondition>().conditions = catalingDestroyConditions;
                        });
                    }
                }
                bool HandleUse()
                {
                    if (catalingType != AbilityType.Use) return false;

                    var info = cataling.GetComponent<CatalystInfo>();
                    info.abilityInfo = ability;
                    info.isCataling = true;

                    var newCataling = Instantiate(cataling, catalyst.transform.position, catalyst.transform.rotation);

                    var newInfo = newCataling.GetComponent<CatalystInfo>();

                    OnCatalystRelease?.Invoke(catalyst, newInfo);
                    catalyst.OnCatalingRelease?.Invoke(catalyst, newInfo);
                    ability.OnCatalystRelease?.Invoke(catalyst);


                    ArchAction.Yield(() => {
                        newInfo.value = value;
                        newInfo.range = radius;
                        newInfo.GetComponent<CatalystDeathCondition>().conditions = catalingDestroyConditions;
                        newCataling.AddComponent<CatalystUse>();
                    });

                    return true;
                }
                bool HandleFreeCast()
                {
                    if (catalingType != AbilityType.SkillShot) return false;
                    float percent = (float)position / releasePerInterval;

                    var offset = percent * 360f;

                    ReleaseDirection(offset + currentAngle);

                    return true;

                    void ReleaseDirection(float positionY)
                    {
                        var info = cataling.GetComponent<CatalystInfo>();
                        info.abilityInfo = ability;
                        info.isCataling = true;

                        var newCataling = Instantiate(this.cataling, catalyst.transform.position, Quaternion.Euler(0, positionY, 0));

                        var newInfo = newCataling.GetComponent<CatalystInfo>();

                        OnCatalystRelease?.Invoke(catalyst, newInfo);
                        catalyst.OnCatalingRelease?.Invoke(catalyst, newInfo);

                        if (ability.abilityType == AbilityType.LockOn)
                        {
                            newInfo.AddTarget(catalyst.target.GetComponent<EntityInfo>());
                        }

                        ArchAction.Yield(() => { 
                            newInfo.value = value;
                            newInfo.range = radius;
                            newInfo.GetComponent<CatalystDeathCondition>().conditions = catalingDestroyConditions;

                        });

                    }
                }
            }

            async void FindTargets()
            {
                if (catalingType != AbilityType.LockOn) return;

                do
                {
                    targets = catalyst.TargetableEntities(radius, true);
                    Debugger.Combat(9531, $"{targets.Count}");
                    await Task.Delay(500);
                }while (!catalyst.isDestroyed) ;
            }
        }

        // Update is called once per frame
    }
}
