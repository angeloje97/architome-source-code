using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Pathfinding;

namespace Architome
{
    public enum AugmentCatalingEvent
    {
        OnReleaseCataling
    }


    public class AugmentCataling : AugmentType
    {
        

        public GameObject cataling;
        public CatalystInfo catalingInfo;
        public AbilityType catalingType;
        public CatalystEvent releaseCondition;

        public AugmentProp.DestroyConditions catalingDestroyConditions;

        public int releasePerInterval;
        public float interval = 1f;
        public float radius, rotationPerInterval, startDelay;

        public bool targetsCaster;
        public bool combatInclusive;

        

        public Action<CatalystInfo, CatalystInfo> OnCatalystRelease; // (original catalyst, cataling)

        ArchEventHandler<AugmentCatalingEvent, (Augment.AugmentEventData, CatalystInfo, CatalystInfo)> events;

        async void Start()
        {

            await GetDependencies(() => {
                catalingInfo = cataling.GetComponent<CatalystInfo>();

                EnableCatalyst();
            });
        }

        private void Awake()
        {
            events = new(this);
        }
        void Update()
        {
        
        }
        
        public Action AddListener(AugmentCatalingEvent eventType, Action<(Augment.AugmentEventData, CatalystInfo, CatalystInfo)> action, Component listener)
        {
            return events.AddListener(eventType, action, listener);
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            if (releaseCondition == CatalystEvent.OnInterval) return;
            if (releaseCondition == CatalystEvent.OnCatalingRelease) return;
            if (catalyst.isCataling) return;

            Debugger.Combat(7451, $"Cataling's Parent: {catalyst}");

            catalyst.AddEventAction(releaseCondition, () => ActivateCataling(catalyst));


        }

        protected override string Description()
        {
            var catalingName = ArchString.CamelToTitle(cataling.name);
            var catalingType = ArchString.CamelToTitle(this.catalingType.ToString());

            if (augment)
            {
                return $"Releases {releasePerInterval} {catalingType} {catalingName} every {interval} seconds with {ArchString.FloatToSimple(value, 0)} power.";

            }
            else
            {
                return $"Releases {releasePerInterval} {catalingType} {catalingName} every {interval} seconds with {valueContribution * 100}% power of its ability's value";
            }

        }
        async void ActivateCataling(CatalystInfo catalyst)
        {

            await Task.Delay((int)(startDelay * 1000));

            var catalystHit = catalyst.GetComponent<CatalystHit>();
            int catalingsReleased = 0;
            float currentAngle = 0f;
            var targets = new List<EntityInfo>();

            FindTargets();

            Debugger.Combat(1532, $"Target Count: {targets.Count}");

            foreach(var target in targets)
            {
                Debugger.Combat(1533, $"Cataling found target {target}");
            }

            var eventData = new Augment.AugmentEventData(this) { active = true };
            augment.ActivateAugment(eventData);
            



            do
            {
                bool released = false;

                for (int i = 0; i < releasePerInterval; i++)
                {
                    if (OnCatalingInterval(i))
                    {
                        Debugger.Combat(4152, $"Current cataling release {i} for {targets.Count} targets");
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
                    var delay = position / (float) releasePerInterval;

                    var index = (catalingsReleased + position) % targets.Count;



                    if (position >= targets.Count)
                    {
                        ArchAction.Delay(() => ReleaseLockOn(targets[index]), delay * .5f);
                    }
                    else
                    {
                        ReleaseLockOn(targets[index]);
                    }

                    return true;

                    void ReleaseLockOn(EntityInfo target)
                    {
                        catalingInfo.target = target.gameObject;
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
                    targets = catalyst.EntitiesWithinRadius(radius, HandleEntity, true);
                    await Task.Delay(500);
                }while (!catalyst.isDestroyed) ;

                bool HandleEntity(EntityInfo entity)
                {
                    var canHit = catalystHit.CanHit(entity);

                    Debugger.Combat(1243, $"Augment Can Hit {entity} : {canHit}");

                    return canHit;
                }
            }

        }

        // Update is called once per frame
    }
}
