using System.Threading.Tasks;
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
            Target,
            StartMove
        };

        [Header("Move To Listener Properties")]
        public EntityInfo sourceInfo;
        public MoveToType moveToType;
        public Transform target;
        public Vector3 location;
        

        public float validRadius = 3f;

        [Header("Actions")]
        public bool setLocationToTarget;

        bool active;

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

        private void Update()
        {
        }

        async void HandleLocation()
        {
            if (active) return;
            active = true;
            while (!activated)
            {
                await Task.Yield();

                var location = moveToType == MoveToType.Location ? this.location : target.transform.position;

                var distance = V3Helper.Distance(sourceInfo.transform.position, location);

                if (distance <= validRadius)
                {
                    break;
                }
            }

            active = false;

            ActivateEventListener();
        }

        public override void StartEventListener()
        {
            if (sourceInfo == null) return;
            var movement = sourceInfo.Movement();
            if (movement == null) return;


            base.StartEventListener();
            if (moveToType == MoveToType.StartMove)
            {
                movement.OnStartMove += OnStartMove; 
                return;
            }

            HandleLocation();
        }

        void OnStartMove(Movement movement)
        {
            movement.OnStartMove -= OnStartMove;
            ActivateEventListener();
        }
    }
}
