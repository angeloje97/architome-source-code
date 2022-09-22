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
            base.GetDependencies();
            HandleStart();
        }

        private void Update()
        {
        }

        async void HandleLocation()
        {
            if (active) return;
            active = true;
            while (!completed)
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

            CompleteEventListener();
        }

        public override string Directions()
        {

            var selectKeyIndex = keyBindData.SpriteIndex("Select");
            var actionKeyIndex = keyBindData.SpriteIndex("Action");
            return $"To move {sourceInfo.entityName}, select them with (Select <sprite={selectKeyIndex}> ) and then use (Action <sprite={actionKeyIndex}>) on a location you want to move them.";
        }

        public override string Tips()
        {
            var stringList = new List<string>() {
                base.Tips()
            };
        
            var members = sourceInfo.transform.parent.GetComponentsInChildren<EntityInfo>();

            var memberIndex = 0;

            for (int i = 0; i < members.Length; i++)
            {
                if (members[i] != sourceInfo) continue;
                memberIndex = i;
            }

            var alternateActionIndex = keyBindData.SpriteIndex($"AlternateAction{memberIndex}");
            stringList.Add(
                $"Tip: Alternatively, you can use (Member Action {memberIndex + 1} <sprite={alternateActionIndex}> ) on a desired location to move party member ({memberIndex + 1}) without having to select the them."
            );

            return ArchString.NextLineList(stringList);

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
            CompleteEventListener();
        }
    }
}
