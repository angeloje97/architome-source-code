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
        [SerializeField] float movementOffset;

        void Start()
        {
            GetDependencies();
        }

        protected async new void GetDependencies()
        {
            await base.GetDependencies();
            entityMovement = augment.entity.Movement();
            EnableAbilityStartEnd();
        }

        protected override string Description()
        {
            var description = "";

            if(augmentMovementType == AugmentMovementType.ChangeSpeed)
            {
                var prefix = movementOffset < 0 ? "Decreases" : "Increases";
                description = $"{prefix} movement speed by {movementOffset}%";
            }

            if(augmentMovementType == AugmentMovementType.LockMovement)
            {
                description = $"Prevents the caster from being able to move.";
            }

            if(augmentMovementType == AugmentMovementType.CancelOnMove)
            {
                description = $"Cancels ability if the caster movies";
            }

            return description;
        }
        public override void HandleAbility(AbilityInfo ability, bool start)
        {
            if (!start) return;
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
                Debugger.Combat(1054, $"Starting augment movement cancel: {augment}");
                await entityMovement.StopMovingAsync();
                Debugger.Combat(1055, $"Stopped movement from: {augment}");

                while (ability.activated)
                {
                    if (entityMovement.isMoving)
                    {
                        ability.CancelCast($"Moved while casting (From ({augment})");
                        Debugger.Combat(1056, $"Augment Cancels Cast {augment}");
                        break;
                    }
                    await Task.Yield();
                }
            }
        }

    }
}