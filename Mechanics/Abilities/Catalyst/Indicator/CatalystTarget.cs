using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Indicator
{
    public class CatalystTarget : CatalystIndicator
    {
        [Header("Catalyst Target Properties")]
        public bool setTarget;
        public bool setLocation;

        [SerializeField] Vector3 targetPosition;
        float yPositionTimer;
        float yPos;

        protected override void Start()
        {
            groundLayer = LayerMasksData.active.walkableLayer;
            SetProjector(true);
        }


        protected override void Update()
        {
            if (catalyst == null) return;

            targetPosition = TargetPosition();
            UpdateYPosition();


            transform.position = new Vector3(targetPosition.x, yPos, targetPosition.z);

        }

        void UpdateYPosition(bool forceUpdate = false)
        {
            if(yPositionTimer > 0 && !forceUpdate)
            {
                yPositionTimer -= Time.deltaTime;
                
                return;
            }

            yPositionTimer = 1f;

            var groundPosition = V3Helper.GroundPosition(targetPosition, groundLayer);
            yPos = groundPosition.y;
        }

        Vector3 TargetPosition()
        {
            targetPosition = catalyst.metrics.TargetPosition();

            if(yPos == 0f)
            {
                UpdateYPosition(true);
            }

            return targetPosition;
        }
    }
}
