using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Architome
{
    public class CatalystSplash : CatalystProp
    {
        // Start is called before the first frame update
        public AbilityInfo.SplashProperties splash;

        new void GetDependencies()
        {
            base.GetDependencies();


            if (catalyst)
            {
                catalyst.OnHit += OnHit;
            }

            this.splash = ability.splash;
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame

        async public void OnHit(GameObject target)
        {
            await Task.Delay((int) (splash.delay * 1000));
            if (catalyst == null) return;
            var obstructionMask = GMHelper.LayerMasks().structureLayerMask;
            var originalValue = catalyst.value;

            catalyst.value *= splash.valueContribution;

            catalystHit.appliesBuff = splash.appliesBuffs;

            var entities = catalyst.EntitiesWithinRadius(splash.radius).Where(entity => entity != target).ToList();
            
            foreach (var entity in catalyst.EntitiesWithinRadius(splash.radius))
            {
                if (V3Helper.IsObstructed(entity.transform.position, catalyst.transform.position, obstructionMask) && splash.requiresLOS) continue;
                catalystHit.HandleTargetHit(entity.GetComponent<EntityInfo>());
            }

            catalystHit.appliesBuff = true;
            catalyst.value = originalValue;

        }
    }

}