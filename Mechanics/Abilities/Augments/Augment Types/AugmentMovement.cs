using System.Collections;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Architome.Enums;

namespace Architome
{
    public class AugmentMovement : AugmentType
    {
        public enum AugmentMovementType 
        {
            ChangeSpeed,
            LockMovement,
            CancelOnMove,
        }

        Movement entityMovement;
        [SerializeField] AugmentMovementType augmentMovementType;
        public float movementOffset;
        
        protected async new void GetDependencies()
        {
            await base.GetDependencies();
            entityMovement = augment.entity.Movement();
        }
        public override void HandleAbility(AbilityInfo ability, bool start)
        {
            if (start) return;
            if (entityMovement == null) return;
            Func<Task> endActivation = ability.EndActivation;
            HandleChangeSpeed();
            HandleLockMovement();
            HandleCancelOnMove();


            async void HandleChangeSpeed()
            {
                if (augmentMovementType != AugmentMovementType.ChangeSpeed) return;
                var reset = entityMovement.SetOffSetMovementSpeed(movementOffset, this);
                await endActivation();
                reset();
            }

            async void HandleLockMovement()
            {
                if (augmentMovementType != AugmentMovementType.LockMovement) return;
                augment.entity.AddState(EntityState.Immobalized);
                await endActivation();
                augment.entity.RemoveState(EntityState.Immobalized);
            }

            async void HandleCancelOnMove()
            {
                if (augmentMovementType != AugmentMovementType.CancelOnMove) return;
                await entityMovement.StopMovingAsync();
                while (ability.activated)
                {
                    if (entityMovement.isMoving)
                    {
                        ability.CancelCast($"Moved while casting (From ({augment})");
                        break;
                    }
                    await Task.Yield();
                }
            }
        }

    }
}