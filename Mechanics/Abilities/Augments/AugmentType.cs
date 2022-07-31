using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentType : MonoBehaviour
    {
        protected Augment augment;
        protected AbilityInfo ability;

        public float value;
        public float valueContribution = 1f;

        protected CatalystInfo activeCatalyst;
        protected CatalystHit activeHit;

        async void Start()
        {
            await GetDependencies();
        }

        protected async Task GetDependencies()
        {
            augment = GetComponent<Augment>();


            while (!augment.dependenciesAcquired)
            {
                await Task.Yield();
            }
            
            if (augment)
            {
                value = valueContribution * augment.info.value;
                ability = augment.ability;
            }

        }

        protected void EnableCatalyst()
        {
            augment.OnNewCatalyst += HandleNewCatlyst;
        }

        public virtual void SetCatalyst(CatalystInfo catalyst, bool active)
        {
            if (active)
            {
                activeCatalyst = catalyst;
                activeHit = catalyst.GetComponent<CatalystHit>();

            }
            else
            {
                if (activeCatalyst == catalyst)
                {
                    activeCatalyst = null;
                    activeHit = null;
                }
            }
        }

        public virtual void HandleNewCatlyst(CatalystInfo catalyst)
        {

        }
        public virtual string Description()
        {
            var result = "";
            return result;
        }
    }
}
