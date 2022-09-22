using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class StateChangeListener : EventListener
    {

        [Header("StateChangeListener")]
        public List<EntityInfo> targetEntities;
        public EntityState validState;
        public AbilityInfo sourceAbility;

        bool castedAbility;

        void Start()
        {
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();
            
            foreach (var target in targetEntities)
            {
                target.combatEvents.OnStatesChange += HandleStateChange;
                PreventEntityDeath(target);
            }

            if (!sourceAbility)
            {
                castedAbility = true;
            }
            else
            {
                sourceAbility.OnSuccessfulCast += HandleSuccesfulCast;
            }



            OnSuccessfulEvent += (EventListener listener) => {
                foreach (var target in targetEntities)
                {
                    target.combatEvents.OnStatesChange -= HandleStateChange;
                }

                if (sourceAbility)
                {
                    sourceAbility.OnSuccessfulCast -= HandleSuccesfulCast;
                }
            };


            void HandleStateChange(List<EntityState> beforeStates, List<EntityState> afterStates)
            {
                if (!castedAbility) return;
                if (afterStates.Contains(validState))
                {
                    CompleteEventListener();
                }
            }
            
            void HandleSuccesfulCast(AbilityInfo ability)
            {
                castedAbility = true;
                ArchAction.Delay(() => {
                    castedAbility = false;
                }, 2f);
            }
        }

        public override string Directions()
        {
            return base.Directions();
        }

        public override string Tips()
        {
            return base.Tips();
        }

    }
}
