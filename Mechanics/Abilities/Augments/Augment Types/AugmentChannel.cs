using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentChannel : AugmentType
    {
        Movement entityMovement;
        AbilityManager abilityManager;

        [Header("Channel Properties")]
        public bool active;
        public float time;
        public int invokeAmount;
        public bool cancel;

        [Header("Restrictions")]
        public bool canMove;
        public bool cancelChannelOnMove;
        public float deltaMovementSpeed;

        bool canceledChannel;

        float timer = 0f;

        protected override void GetDependencies()
        {
            EnableAugmentAbility();
            AllowInterruptable();

            if (augment.entity)
            {
                entityMovement = augment.entity.Movement();
            }

            abilityManager = ability.GetComponentInParent<AbilityManager>();

            if (entityMovement)
            {
                var unsubscribe = entityMovement.AddListener(eMovementEvent.OnStartMove, OnStartMove, this);

                augment.OnRemove += (Augment augment) => {
                    unsubscribe();
                };
            }

            HandleBusyCheck();
        }


        public override async Task<bool> Ability()
        {
            StartChannel();

            var eventData = new Augment.AugmentEventData(this)
            {
                active = true,
                hasEnd = true,
            };
            augment.ActivateAugment(eventData);

            timer = time - time * ability.Haste();
            var startTime = timer;

            float progressPerInvoke = (1f / invokeAmount);
            float progressBlock = 1 - progressPerInvoke;

            var success = true;
            

            while (timer > 0)
            {
                await Task.Yield();

                timer -= Time.deltaTime;
                Debugger.Combat(2945, $"{ability} channel timer : {timer}");
                ability.progress = (timer / startTime);
                ability.progressTimer = timer;

                WhileChanneling();

                

                if (canceledChannel)
                {
                    LogCancel("Canceled Channel");
                    canceledChannel = false;
                    success = false;
                    break;
                }

                if (progressBlock > ability.progress)
                {
                    if (!ability.CanContinue())
                    {
                        LogCancel("Ability Can't Continuie");
                        success = false;
                        break;
                    }

                    ability.HandleAbilityType();
                    abilityManager.OnChannelInterval?.Invoke(ability, this);
                    augment.TriggerAugment(eventData);
                    progressBlock -= progressPerInvoke;
                }

            }

            EndChannel();

            eventData.active = false;

            return success;

            void StartChannel()
            {
                active = true;
                ability.currentChannel = this;
                ability.progress = 1;
                abilityManager.OnChannelStart?.Invoke(ability, this);
            }

            void WhileChanneling()
            {
                abilityManager.WhileCasting?.Invoke(ability);
                ability.WhileCasting?.Invoke(ability);

                if (ability.isAttack) return;

                if (ability.targetLocked != ability.target)
                {
                    ability.targetLocked = ability.target;
                }

                if (ability.abilityType == AbilityType.LockOn && ability.target == null)
                {
                    CancelChannel();
                }
            }

            void EndChannel()
            {
                active = false;
                if (ability.currentChannel == this)
                {
                    ability.currentChannel = null;
                }
                ability.progress = 0;
                abilityManager.OnChannelEnd?.Invoke(ability, this);
            }
        }

        public async Task<AugmentChannel> EndChanneling()
        {
            while (active) await Task.Yield();
            return this;
        }
        public override void HandleCancelAbility(AugmentType augment)
        {
            if (!active) return;
            canceledChannel = true;
        }
        void HandleBusyCheck()
        {
            Action<AbilityInfo, List<bool>> action = (AbilityInfo ability, List<bool> busyList) => {
                busyList.Add(active);

                Debugger.Combat(4123, $"Busy List Count {busyList.Count}");
            };

            ability.OnBusyCheck += action;

            augment.OnRemove += (Augment augment) => {
                ability.OnBusyCheck -= action;
            };
        }
        public void OnStartMove(MovementEventData movement)
        {
            if (!active) return;
            if (cancelChannelOnMove)
            {
                CancelChannel();
            }
        }
        protected override string Description()
        {
            var result = "";

            result += $"Channels for {time} seconds repeating the same ability {invokeAmount} times.";

            return result;
        }
        public void LogCancel(string reason)
        {
            Debugger.Combat(2946, $"{this} stopped channeling because {reason}");
        }
        public void CancelChannel()
        {
            if (!active) return;

            canceledChannel = true;
            abilityManager.OnCancelChannel?.Invoke(ability, this);
        }

    }
}
