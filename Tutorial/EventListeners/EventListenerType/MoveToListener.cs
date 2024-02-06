using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Settings.Keybindings;

namespace Architome.Tutorial
{
    public class MoveToListener : EventListener
    {
        public enum MoveToType
        {
            Location,
            Target,
            StartMove,
            QuickMoveOnly,
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
            if (moveToType != MoveToType.Location) return;
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
            var result = new List<string>() {
                base.Directions()
            };
            var selectKeyIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "Select");
            var actionKeyIndex = keyBindData.SpriteIndex(KeybindSetType.Party, "Action");

            


            if (!simple)
            {
                if(moveToType == MoveToType.StartMove)
                {
                    result.Add($"To move {sourceInfo.entityName}, select them with (Select <sprite={selectKeyIndex}>) and then use (Action <sprite={actionKeyIndex}>) on a location you want to move them.");
                }

                if(moveToType == MoveToType.QuickMoveOnly)
                {
                    var memberIndex = 0;

                    var members = sourceInfo.transform.parent.GetComponentsInChildren<EntityInfo>();

                    for (int i = 0; i < members.Length; i++)
                    {
                        if (members[i] != sourceInfo) continue;
                        memberIndex = i;
                    }

                    var alternateActionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, $"AlternateAction{memberIndex+1}");

                    result.Add($"To use quick move on {sourceInfo}, use <sprite={alternateActionIndex}> since they are member {memberIndex + 1}");
                }

            }
            return ArchString.NextLineList(result);
        }

        public override string Tips()
        {
            var stringList = new List<string>() {
                base.Tips()
            };

            var memberIndex = MemberIndex(sourceInfo);

            var alternateActionIndex = keyBindData.SpriteIndex(KeybindSetType.Party, $"AlternateAction{memberIndex + 1}");

            if (!simple)
            {
                if(moveToType == MoveToType.StartMove)
                {
                    stringList.Add(
                        $"Tip: Alternatively, you can use the quick move action (<sprite={alternateActionIndex}>) on a desired location to move party member ({memberIndex + 1}) without having to select the them."
                    );

                }

            }

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
                movement.AddListenerLimit(eMovementEvent.OnStartMove, () => {
                    CompleteEventListener();
                }, this);
                return;
            }

            if (moveToType == MoveToType.QuickMoveOnly)
            {
                movement.AddListenerLimit(eMovementEvent.OnQuickMove, () => {
                    CompleteEventListener();
                }, this);
            }





            HandleLocation();
        }
    }
}
