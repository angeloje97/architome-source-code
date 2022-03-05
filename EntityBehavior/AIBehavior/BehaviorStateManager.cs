using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class BehaviorStateManager
    {
        public StateMachineBehaviour stateMachine;
        public EntityInfo entityInfo;
        public AIBehavior behavior;
        public Movement movement;

        private bool wantsToCast;
        // Start is called before the first frame update
        public void Activate(AIBehavior behavior)
        {
            this.behavior = behavior;
            entityInfo = behavior.GetComponentInParent<EntityInfo>();

            entityInfo.OnLifeChange += OnLifeChange;
            
            if(entityInfo.Movement())
            {
                movement = entityInfo.Movement();
                movement.OnStartMove += OnStartMove;
                movement.OnEndMove += OnEndMove;
            }

            if(entityInfo.AbilityManager())
            {
                var abilityManager = entityInfo.AbilityManager();
                abilityManager.OnWantsToCastChange += OnWantsToCastChange;
                abilityManager.OnCastStart += OnCastStart;
                abilityManager.OnCastRelease += OnCastRelease;
                abilityManager.OnCancelCast += OnCancelCast;
                abilityManager.OnTryCast += OnTryCast;
            }
        }


        public void OnStartMove(Movement movement)
        {
            if (wantsToCast) { return; }
            if(behavior.behaviorState == BehaviorState.Casting) { return; }
            behavior.behaviorState = BehaviorState.Moving;
        }

        public void OnEndMove(Movement movement)
        {
            if (wantsToCast) { return; }
            if(behavior.behaviorState == BehaviorState.Casting) { return; }
            behavior.behaviorState = BehaviorState.Idle;
        }

        public void OnWantsToCastChange(AbilityInfo ability, bool wantsToCast)
        {
            this.wantsToCast = wantsToCast;

            if (!this.wantsToCast)
            {
                if (movement.isMoving)
                {
                    behavior.behaviorState = BehaviorState.Moving;
                }
                else
                {
                    behavior.behaviorState = BehaviorState.Idle;
                }
            }
            else
            {
                behavior.behaviorState = BehaviorState.WantsToCast;
            }
        }

        public void OnTryCast(AbilityInfo ability)
        {
            if (behavior.behaviorState == BehaviorState.Inactive) return;

            

            ArchAction.Delay(() => 
            {
                if(ability.WantsToCast())
                {
                    behavior.behaviorState = BehaviorState.WantsToCast;
                }
            }, 0125);
        }

        


        public void OnCastStart(AbilityInfo ability)
        {
            if(!ability.target)
            {
                behavior.behaviorState = BehaviorState.Casting;
            }
            else
            {
                if(entityInfo.CanAttack(ability.target))
                {
                    behavior.behaviorState = BehaviorState.Attacking;
                }
                else
                {
                    behavior.behaviorState = BehaviorState.Assisting;
                }
            }
        }

        public void OnCancelCast(AbilityInfo ability)
        {
            if(ability.abilityManager.wantsToCastAbility)
            {
                behavior.behaviorState = BehaviorState.WantsToCast;
            }
            else if(movement.isMoving)
            {
                behavior.behaviorState = BehaviorState.Moving;
            }
            else
            {
                behavior.behaviorState = BehaviorState.Idle;
            }
        }
        
        public void OnCastRelease(AbilityInfo ability)
        {
            if(movement.isMoving)
            {
                behavior.behaviorState = BehaviorState.Moving;
            }
            else
            {
                behavior.behaviorState = BehaviorState.Idle;
            }
        }

        public void OnLifeChange(bool isAlive)
        {
            behavior.behaviorState = isAlive ? BehaviorState.Idle : BehaviorState.Inactive;
        }
    }

}
