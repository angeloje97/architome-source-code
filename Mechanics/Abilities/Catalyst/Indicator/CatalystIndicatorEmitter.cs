using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystIndicatorEmitter : MonoBehaviour
    {
        public AbilityInfo ability;
        public CatalystIndicator indicatorPrefab;
        CatalystManager catalystManager;

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
        }

        void HandleCatalystRelease(CatalystInfo catalyst)
        {
            var newIndicator = Instantiate(indicatorPrefab, catalystManager.transform);
            newIndicator.SetCatalyst(catalyst);
        }


    }
}
