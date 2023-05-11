using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Indicator
{
    public class CatalystFill : CatalystIndicator
    {
        [Header("Fill Properties")]
        public float currentPercent;
        public float percentOffset;
        protected override void Update()
        {
            if (catalyst == null) return;
            if (source == null) return;
            currentPercent = catalyst.metrics.DistancePercent() + percentOffset;
            currentPercent = Mathf.Clamp(currentPercent, 0f, 1f);
            SetScale(Vector3.one * currentPercent);
        }
    }
}
