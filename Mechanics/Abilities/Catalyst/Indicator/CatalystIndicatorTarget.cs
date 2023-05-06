using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystIndicatorTarget : CatalystIndicator
    {
        [Header("Catalyst Target Properties")]
        public bool setTarget;
        public bool setLocation;


        [SerializeField] Vector3 targetPosition;
        float yPositionTimer;
        float yPos;


        protected override void GetDependencies()
        {
            base.GetDependencies();
            groundLayer = LayerMasksData.active.walkableLayer;
        }

        public override void SetCatalyst(CatalystInfo catalyst, CatalystIndicatorEmitter source)
        {
            base.SetCatalyst(catalyst, source);
            SetProjector(true);
            
        }

        protected override void Update()
        {
            if (catalyst == null) return;

            targetPosition = TargetPosition();
            UpdateYPosition();


            transform.position = new Vector3(targetPosition.x, yPos, targetPosition.z);

        }

        void UpdateYPosition()
        {
            if(yPositionTimer > 0)
            {
                yPositionTimer -= Time.deltaTime;
                return;
            }

            yPositionTimer = 1f;
            SetToFloor(0);
        }

        Vector3 TargetPosition()
        {
            if (setLocation)
            {
                return catalyst.metrics.targetLocation;
            }

            if (setTarget)
            {
                return catalyst.target.transform.position;
            }

            return transform.position;
        }
    }
}
