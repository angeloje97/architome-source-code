using System.Collections;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Architome.Enums;
using System.Collections.Generic;

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


        protected override void GetDependencies()
        {
            entityMovement = augment.entity.Movement();
            if (phaseType == AbilityPhase.Activating)
            {
                EnableAbilityStartEnd();
            }
            else if (phaseType == AbilityPhase.Casting)
            {
                EnableStartCast();
            }

            else if (phaseType == AbilityPhase.Channeling)
            {
                EnableAbilityChanneling();
            }

            HandleCancelOnMove();

        }
        protected override string Description()
        {
            var description = augmentMovementType switch
            {
                AugmentMovementType.LockMovement => "Prevents the caster from being able to move ",
                 AugmentMovementType.CancelOnMove => "Cancels ability if the caster moves ",
                _ => movementOffset < 0 ? "Decreases" : "Increases" + $" movement speed by {movementOffset}% ",
            };

            description += phaseType switch
            {
                AbilityPhase.Channeling => " only while channeling.",
                AbilityPhase.Casting => " only while casting.",
                _ => " for the duration of the ability.",
            };

            return description;
        }

        void HandleCancelOnMove()
        {

            EnableAbilityBeforeCast(async (AbilityInfo ability, List<Func<Task>> tasks) => {
                
            });
        }

        public async void HandleMovement(AbilityInfo ability, Func<Task> endActivation, Action escapeCallBack = null)
        {
            if (entityMovement == null) return;

            var eventData = new Augment.AugmentEventData(this) 
            { 
                active = true,
                hasEnd = true,
            };
            augment.ActivateAugment(eventData);

            await HandleChangeSpeed();
            await HandleLockMovement();
            await HandleCancelOnMove();

            eventData.active = false;

            async Task HandleChangeSpeed()
            {
                if (augmentMovementType != AugmentMovementType.ChangeSpeed) return;
                var reset = entityMovement.SetOffSetMovementSpeed(movementOffset, this);
                await endActivation();
                reset();
            }

            async Task HandleLockMovement()
            {
                if (augmentMovementType != AugmentMovementType.LockMovement) return;
                augment.entity.AddState(EntityState.Immobalized, new(augment, augment.entity, new() { EntityState.Immobalized }));
                await endActivation();
                augment.entity.RemoveState(EntityState.Immobalized);
            }

            async Task HandleCancelOnMove()
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
            HandleMovement(ability, ability.EndActivation, CancelAll);
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

        void CancelAll()
        {
            ability.CancelCast($"Moved during casting from {this}");
            var channelAugments = ability.GetComponentsInChildren<AugmentChannel>();
            foreach (var channel in channelAugments)
            {
                channel.CancelChannel();
            }
        }

    }
}