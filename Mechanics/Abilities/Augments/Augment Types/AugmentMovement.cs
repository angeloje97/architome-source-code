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

        public enum AbilityPhase
        {
            Activating,
            Casting,
            Channeling
        }

        Movement entityMovement;
        [SerializeField] AugmentMovementType augmentMovementType;
        [SerializeField] AbilityPhase phaseType;
        [SerializeField] float movementOffset;

        void Start()
        {
            GetDependencies();
        }

        protected async new void GetDependencies()
        {
            await base.GetDependencies();
            entityMovement = augment.entity.Movement();
            if (phaseType == AbilityPhase.Activating)
            {
                EnableAbilityStartEnd();
            }
            else if (phaseType == AbilityPhase.Casting)
            {
                EnableStartCast();
            }

            else if(phaseType == AbilityPhase.Channeling)
            {
                EnableAbilityChanneling();
            }
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

        

        public void HandleMovement(AbilityInfo ability, Func<Task> endActivation, Action escapeCallBack = null)
        {
            if (entityMovement == null) return;
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

                var task = endActivation();

                while (!task.IsCompleted)
                {
                    if (entityMovement.isMoving)
                    {
                        ability.CancelCast($"Moved while casting (From ({augment})");
                        escapeCallBack?.Invoke();
                        break;
                    }
                    await Task.Yield();
                }
            }
        }

        public override void HandleAbility(AbilityInfo ability, bool start)
        {
            if (!start) return;
            HandleMovement(ability, ability.EndActivation, () => {
                ability.CancelCast($"Moved during casting from {this}");
                var channelAugments = ability.GetComponentsInChildren<AugmentChannel>();
                foreach(var channel in channelAugments)
                {
                    channel.CancelChannel();
                }
            });
        }

        protected override void HandleCastStart(AbilityInfo ability)
        {
            HandleMovement(ability, ability.EndCasting, () => {
                ability.CancelCast("Moved during casting");
            });
        }

        protected override void HandleChannelStart(AbilityInfo ability, AugmentChannel channel)
        {
            HandleMovement(ability, channel.EndChanneling, channel.CancelChannel);
        }

    }
}