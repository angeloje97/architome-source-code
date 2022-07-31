using System;
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
        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            if (this == null) return;
            await base.GetDependencies();


            if (augment.entity)
            {
                entityMovement = augment.entity.Movement();
            }

            abilityManager = ability.GetComponentInParent<AbilityManager>();


            ability.OnSuccessfulCast += OnSuccessfulCast;


            augment.OnRemove += (Augment augment) => {
                ability.OnSuccessfulCast -= OnSuccessfulCast;

                if (entityMovement)
                {
                    entityMovement.OnStartMove -= OnStartMove;
                }
            };

            if (entityMovement)
            {
                entityMovement.OnStartMove += OnStartMove;
            }

            HandleBusyCheck();

        }

        void HandleBusyCheck()
        {
            Action<AbilityInfo> action = (AbilityInfo ability) => {
                ability.busyList.Add(active);
            };

            ability.OnBusyCheck += action;

            augment.OnRemove += (Augment augment) => {
                ability.OnBusyCheck -= action;
            };
        }

        public void OnSuccessfulCast(AbilityInfo ability)
        {
            ability.tasksBeforeEndAbility.Add(Channel());
        }

        async public Task Channel()
        {
            StartChannel();

            var timer = time - time * ability.Haste();
            var startTime = timer;

            float progressPerInvoke = (1f / invokeAmount);
            float progressBlock = 1 - progressPerInvoke;

            while (timer > 0)
            {
                await Task.Yield();

                timer -= Time.deltaTime;
                Debugger.Combat(2945, $"{ability} channel timer : {timer}");
                ability.progress = (timer / startTime);

                WhileChanneling();

                if (!ability.CanContinue())
                {
                    LogCancel("Ability Can't Continue");
                    break;

                }

                if (canceledChannel)
                {
                    LogCancel("Canceled Channel");
                    canceledChannel = false;
                    break ;
                }

                if (progressBlock > ability.progress)
                {
                    ability.HandleAbilityType();
                    abilityManager.OnChannelInterval?.Invoke(ability, this);
                    progressBlock -= progressPerInvoke;
                }

            }

            EndChannel();

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
                ability.HandleTracking();

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

        public void OnStartMove(Movement movement)
        {
            if (cancelChannelOnMove)
            {
                CancelChannel();
            }
        }

        public override string Description()
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
