using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class CatalystSplash : CatalystProp
    {
        // Start is called before the first frame update
        public Augment.SplashProperties splash;
        public int currentSplash;
        public int maxSplash;
        new void GetDependencies()
        {
            base.GetDependencies();

            splash = ability.splash;
            maxSplash = catalyst.Ticks();

            if (catalyst)
            {
                catalyst.OnHit += (GameObject target) => { HandleSplash(CatalystEvent.OnHit, target); };
                catalyst.OnAssist += (GameObject target) => { HandleSplash(CatalystEvent.OnAssist, target); };
                catalyst.OnDamage += (GameObject target) => { HandleSplash(CatalystEvent.OnHarm, target); };
                catalyst.OnHeal += (GameObject target) => { HandleSplash(CatalystEvent.OnHeal, target); };
                catalyst.OnCatalystDestroy += (CatalystDeathCondition condition) => { HandleSplash(CatalystEvent.OnDestroy); };
                catalyst.OnCatalystStop += (CatalystKinematics kinematics) => { HandleSplash(CatalystEvent.OnStop); };
                catalyst.OnCatalingRelease += (CatalystInfo catalyst, CatalystInfo cataling) => { HandleSplash(CatalystEvent.OnCatalingRelease); };

            }

        }
        void Start()
        {
            GetDependencies();
            HandleSplash(CatalystEvent.OnAwake);
        }

        async public void HandleSplash(CatalystEvent trigger, GameObject target = null )
        {
            if (splash.trigger != trigger) return;
            await Task.Delay((int)(splash.delay * 1000));
            if (currentSplash >= maxSplash) return;
            currentSplash++;
            if (catalyst == null) return;
            var originalValue = catalyst.value;


            catalyst.value *= splash.valueContribution;
            catalystHit.appliesBuff = splash.appliesBuffs;
            catalystHit.splashing = true;

            var entities = catalyst.EntitiesWithinRadius(splash.radius, splash.requiresLOS);
            int splashedTargets = 0;

            foreach (var entity in entities)
            {
                if (target && entity.gameObject == target) continue;
                if (!catalystHit.CanHit(entity)) continue;
                if (!entity.isAlive) continue;
                if (splashedTargets >= splash.maxSplashTargets) continue;

                Debugger.Combat(6492, $"{gameObject} splashing for {catalyst.value}");
                catalyst.IncreaseTicks(false);
                catalystHit.HandleTargetHit(entity, true);
                splashedTargets++;
                Debugger.Combat(6493, $"{gameObject} Splashed {splashedTargets}/{splash.maxSplashTargets}");


            }


            catalystHit.splashing = false;
            catalystHit.appliesBuff = true;
            catalyst.value = originalValue;
        }

        
    }

}