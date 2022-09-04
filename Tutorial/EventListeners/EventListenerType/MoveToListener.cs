using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class MoveToListener : EventListener
    {
        public enum MoveToType
        {
            Location,
            Target
        };

        [Header("Move To Listener Properties")]
        public EntityInfo sourceInfo;
        public MoveToType moveToType;
        public Transform target;
        public Vector3 location;

        public float validRadius = 3f;

        [Header("Actions")]
        public bool setLocationToTarget;

        private void OnValidate()
        {

            if (!setLocationToTarget) return;
            setLocationToTarget = false;

            if (target)
            {
                location = target.position;
            }
        }


        void Start()
        {
            HandleStart();
        }

        public override void StartEventListener()
        {
            if (sourceInfo == null) return;
            var movement = sourceInfo.Movement();
            if (movement == null) return;
            base.StartEventListener();


            movement.OnEndMove += OnEndMove;
        }

        public void OnEndMove(Movement movement)
        {

            if (moveToType == MoveToType.Location)
            {
                var distance = V3Helper.Distance(sourceInfo.transform.position, location);

                if (distance > validRadius)
                {
                    return;
                }
            }

            if (moveToType == MoveToType.Target)
            {
                if (movement.Target() != target) return;
                if (!movement.HasReachedTarget(3f)) return;
            }

            ActivateEventListener();
            

            if (activated)
            {
                movement.OnEndMove -= OnEndMove;
            }
        }
    }
}
