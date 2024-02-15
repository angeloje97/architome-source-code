using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.Events;

namespace Architome
{
    public class AugmentSplash : AugmentType
    {
        public CatalystEvent splashTrigger;
        public bool requiresLOS, appliesBuffs;
        public int maxSplashTargets;
        public float radius, delay;

        public Dictionary<CatalystInfo, ActiveCatalysts> activeCatalysts;
        public UnityEvent<float> OnSetRadius;
        public UnityEvent<EntityInfo,CatalystInfo> OnEntityInRadius;
        
        [Serializable]
        public class ActiveCatalysts
        {
            public CatalystInfo catalyst;
            public CatalystHit hit;
            public int currentSplash;

            public ActiveCatalysts(CatalystInfo catalyst)
            {
                this.catalyst = catalyst;
                this.hit = catalyst.GetComponent<CatalystHit>();
            }
        }

        protected override void GetDependencies()
        {
            EnableCatalyst();

            if (ability == null) return;
            if (augment == null) return;

            Action<AbilityInfo> action = (AbilityInfo abilty) => {
                ability.floatCheck = radius;
            };

            ability.OnSplashRadiusCheck += action;

            augment.OnRemove += (Augment augment) => {
                ability.OnSplashRadiusCheck -= action;
            };

            OnSetRadius?.Invoke(radius);

        }

        protected override string Description()
        {
            var result = $"Does splash effects {ArchString.CamelToTitle(splashTrigger.ToString())} in a {radius} meter radius equal to ";

            if (augment)
            {
                result += $"{value} value";
            }
            else
            {
                result += $"{ArchString.FloatToSimple(valueContribution * 100)}% value of the baility's original value";
            }

            return result;
        }


        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            if (activeCatalysts == null) activeCatalysts = new();

            activeCatalysts.Add(catalyst, new(catalyst));

            catalyst.AddEventAction(splashTrigger, () => HandleSplash(catalyst));

            //catalyst.OnCatalystDestroy += (CatalystDeathCondition condition) => ArchAction.Yield(() => activeCatalysts.Remove(catalyst));

            //if (splashTrigger == CatalystEvent.OnAwake)
            //{
            //    HandleSplash(catalyst); return;
            //}


            //if (splashTrigger == CatalystEvent.OnDestroy) catalyst.OnCatalystDestroy += (CatalystDeathCondition condition) => HandleSplash(catalyst);
            //if (splashTrigger == CatalystEvent.OnHit) catalyst.OnHit += (CatalystInfo catalyst, EntityInfo target) => HandleSplash(catalyst, target);
            //if (splashTrigger == CatalystEvent.OnAssist) catalyst.OnAssist += (CatalystInfo catalyst, EntityInfo target) => HandleSplash(catalyst, target);
            //if (splashTrigger == CatalystEvent.OnHeal) catalyst.OnHeal += (CatalystInfo catalyst, EntityInfo target) => HandleSplash(catalyst, target);
            //if (splashTrigger == CatalystEvent.OnHarm) catalyst.OnDamage += (CatalystInfo catalyst, EntityInfo target) => HandleSplash(catalyst, target);
            //if (splashTrigger == CatalystEvent.OnStop) catalyst.OnCatalystStop += (CatalystKinematics kinematics) => HandleSplash(catalyst);
            //if (splashTrigger == CatalystEvent.OnCatalingRelease) catalyst.OnCatalingRelease += (CatalystInfo original, CatalystInfo cataling) => HandleSplash(catalyst);


        }

        async public void HandleSplash(CatalystInfo catalyst, EntityInfo target = null)
        {
            if (!activeCatalysts.ContainsKey(catalyst)) return;
            if (activeCatalysts[catalyst].currentSplash > ability.ticksOfDamage) return;
            activeCatalysts[catalyst].currentSplash++;

            var hit = activeCatalysts[catalyst].hit;

            activeCatalyst = catalyst;

            await Task.Delay((int)(delay * 1000));

            var originalValue = catalyst.value;
            var originalApply = hit.appliesBuff;
            hit.appliesBuff = appliesBuffs;
            catalyst.value = value;
            hit.splashing = true;

            //Start

            var entities = catalyst.EntitiesWithinRadius(radius, requiresLOS);

            int splashedTargets = 0;

            foreach (var entity in entities)
            {
                if (entity == null) continue;
                OnEntityInRadius?.Invoke(entity, catalyst);
                if (target && entity == target) continue;
                if (!hit.CanHit(entity)) continue;
                if (!entity.isAlive) continue;
                if (splashedTargets >= maxSplashTargets) break;

                catalyst.IncreaseTicks(false);
                hit.HandleTargetHit(entity, true);
                splashedTargets++;
            }

            //End

            catalyst.value = originalValue;
            hit.appliesBuff = originalApply;
            hit.splashing = false;
            
        }
        void Update()
        {
        
        }
    }
}
