
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{

    public class CatalystIndicatorEmitter : MonoBehaviour
    {
        public AbilityInfo ability;
        public CatalystIndicator indicatorPrefab;
        CatalystManager catalystManager;


        [Header("General Properties")]
        public float generalRadius;
        public RadiusType radiusType;

        private void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            if (ability == null) return;
            if (indicatorPrefab == null) return;
            catalystManager = CatalystManager.active;
            if (catalystManager == null) return;
            ability.OnCatalystRelease += HandleCatalystRelease;

            ArchAction.Delay(() => {
                generalRadius = ability.Radius(radiusType);
            }, .125f);
        }

        void HandleCatalystRelease(CatalystInfo catalyst)
        {
            var newIndicator = Instantiate(indicatorPrefab, catalystManager.transform);
            newIndicator.SetAsMain();
            newIndicator.SetCatalyst(catalyst, this);
        }

        public void SetGeneralRadius(float radius)
        {
            generalRadius = radius;
        }
    }
}
