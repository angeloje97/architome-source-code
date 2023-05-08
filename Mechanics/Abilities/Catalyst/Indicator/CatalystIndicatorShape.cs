using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystIndicatorShape : CatalystIndicator
    {
        [Header("Shape Properties")]
        public float projectorThickness;
        float radius;


        public override void SetCatalyst(CatalystInfo catalyst, CatalystIndicatorEmitter source)
        {
            base.SetCatalyst(catalyst, source);
            radius = source.generalRadius * 2;

            SetProjector(true);
            SetScale(new Vector3(radius, radius, decalThickness));
        }


    }
}
