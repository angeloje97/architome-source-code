using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Architome
{
    public class CatalystIndicator : AbilityIndicator
    {
        [Header("Indicator Properties")]
        public CatalystInfo catalyst;
        public CatalystIndicatorEmitter source;
        bool catalystSet = false;

        bool emitterTarget;

        protected override void Start()
        {
        }

        protected override void GetDependencies()
        {
            if (catalyst == null) return;
            var catalystManager = CatalystManager.active;
            HandleDestoyedCatalyst();
        }

        public void SetAsMain()
        {
            emitterTarget = true;
        }

        public virtual void SetCatalyst(CatalystInfo catalyst, CatalystIndicatorEmitter source)
        {
            foreach(var indicator in GetComponentsInChildren<CatalystIndicator>())
            {
                if (indicator == this) continue;
                indicator.SetCatalyst(catalyst, source);
            }

            if (catalystSet) return;
            catalystSet = true;
            this.catalyst = catalyst;
            this.source = source;
            GetDependencies();


        }

        protected void HandleDestoyedCatalyst()
        {
            if (!emitterTarget) return;
            catalyst.AddEventAction(Enums.CatalystEvent.OnDestroy, () => {
                Destroy(gameObject);
            });
        }


    }
}
