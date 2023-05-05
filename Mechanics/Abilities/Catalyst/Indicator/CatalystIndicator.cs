using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Architome
{
    public class CatalystIndicator : AbilityIndicator
    {
        public CatalystInfo catalyst;
        

        bool catalystSet = false;

        protected override void Start()
        {
        }

        protected override void GetDependencies()
        {
            if (catalyst == null) return;
            var catalystManager = CatalystManager.active;
            transform.SetParent(catalystManager.transform);
            HandleDestoyedCatalyst();
        }

        public virtual void SetCatalyst(CatalystInfo catalyst)
        {
            foreach(var indicator in GetComponentsInChildren<CatalystIndicator>())
            {
                if (indicator == this) continue;
                indicator.SetCatalyst(catalyst);
            }

            if (catalystSet) return;
            catalystSet = true;
            this.catalyst = catalyst;
            GetDependencies();


        }

        protected void HandleDestoyedCatalyst()
        {
            catalyst.AddEventAction(Enums.CatalystEvent.OnDestroy, () => {
                Destroy(gameObject);
            });
        }


    }
}
