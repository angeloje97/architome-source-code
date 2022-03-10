using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public class BehaviorStateManager
    {
        public enum Transition
        {
            OnLifeChange,
            OnStartMove,
            OnEndMove,
            OnWantsToCastChange,
            OnCastStart,
            OnCastEnd,
            OnTryCast,
            OnStateChange,
        }

        [Serializable]
        public class SimpleStateMachine
        {
            public AIBehavior behavior;

            public class StatePermit
            {
                public Transition transition;
                public BehaviorState fromState;
                public BehaviorState toState;

                public bool CheckPermit(Transition transition, BehaviorState fromState, BehaviorState toState)
                {
                    if(this.transition == transition && this.fromState == fromState && this.toState == toState)
                    {
                        return true;
                    }

                    return false;
                }
            }

            public List<StatePermit> statePermits;

            public void Activate(AIBehavior behavior)
            {
                this.behavior = behavior;
                statePermits = new List<StatePermit>();
            }

            public bool ContainsPermit(Transition action, BehaviorState fromState, BehaviorState toState)
            {
                foreach(var current in statePermits)
                {
                    if(current.CheckPermit(action, fromState, toState))
                    {
                        return true;
                    }
                }

                return false;
            }

            public void Permit(BehaviorState fromState, Transition action, BehaviorState toState, bool visVersa = false)
            {
                if(!ContainsPermit(action, fromState, toState))
                {
                    statePermits.Add(new StatePermit() { transition = action, fromState = fromState, toState = toState });
                    if(visVersa)
                    {
                        statePermits.Add(new StatePermit() { transition = action, fromState = toState, toState = fromState });
                    }
                }
            }

            public void Transition(Transition action, BehaviorState toState)
            {

                if(!ContainsPermit(action, behavior.behaviorState , toState)) { return; }
                behavior.behaviorState = toState;
            }

            public void TransitionAnyState(BehaviorState toState)
            {
                behavior.behaviorState = toState;
            }
        }

        public SimpleStateMachine stateMachine;
        public EntityInfo entityInfo;
        public AIBehavior behavior;
        public AbilityManager abilityManager;
        public Movement movement;

        private bool wantsToCast;
        // Start is called before the first frame update
        public void Activate(AIBehavior behavior)
        {
            this.behavior = behavior;
            stateMachine.Activate(behavior);
            SetPermits();
            entityInfo = behavior.GetComponentInParent<EntityInfo>();

            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnChangeNPCType += OnChangeNPCType;
            
            if(entityInfo.Movement())
            {
                movement = entityInfo.Movement();
                movement.OnStartMove += OnStartMove;
                movement.OnEndMove += OnEndMove;
            }

            if(entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();
                abilityManager.OnWantsToCastChange += OnWantsToCastChange;
                abilityManager.OnCastStart += OnCastStart;
                abilityManager.OnCastRelease += OnCastRelease;
                abilityManager.OnCancelCast += OnCancelCast;
                abilityManager.OnTryCast += OnTryCast;
            }

            
        }

        public void SetPermits()
        {
            stateMachine.Permit(BehaviorState.Idle, Transition.OnStartMove, BehaviorState.Moving);
            stateMachine.Permit(BehaviorState.Idle, Transition.OnCastStart, BehaviorState.Attacking);
            stateMachine.Permit(BehaviorState.Idle, Transition.OnCastStart, BehaviorState.Casting);
            stateMachine.Permit(BehaviorState.Idle, Transition.OnCastStart, BehaviorState.Assisting);

            stateMachine.Permit(BehaviorState.Moving, Transition.OnEndMove, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Moving, Transition.OnCastStart, BehaviorState.Casting);
            stateMachine.Permit(BehaviorState.Moving, Transition.OnCastStart, BehaviorState.Assisting);
            stateMachine.Permit(BehaviorState.Moving, Transition.OnCastStart, BehaviorState.Attacking);

            stateMachine.Permit(BehaviorState.Attacking, Transition.OnCastEnd, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Attacking, Transition.OnCastEnd, BehaviorState.Moving);
            stateMachine.Permit(BehaviorState.Attacking, Transition.OnStateChange, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Attacking, Transition.OnStateChange, BehaviorState.Moving);

            stateMachine.Permit(BehaviorState.Casting, Transition.OnCastEnd, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Casting, Transition.OnCastEnd, BehaviorState.Moving);
            stateMachine.Permit(BehaviorState.Casting, Transition.OnStateChange, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Casting, Transition.OnStateChange, BehaviorState.Moving);


            stateMachine.Permit(BehaviorState.Assisting, Transition.OnCastEnd, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Assisting, Transition.OnCastEnd, BehaviorState.Moving);
            stateMachine.Permit(BehaviorState.Assisting, Transition.OnStateChange, BehaviorState.Idle);
            stateMachine.Permit(BehaviorState.Assisting, Transition.OnStateChange, BehaviorState.Moving);

        }


        public void OnStartMove(Movement movement)
        {
            stateMachine.Transition(Transition.OnStartMove, BehaviorState.Moving);

            //if (wantsToCast) { return; }
            //if(behavior.behaviorState == BehaviorState.Casting) { return; }
            //behavior.behaviorState = BehaviorState.Moving;
        }

        public void OnEndMove(Movement movement)
        {
            stateMachine.Transition(Transition.OnEndMove, BehaviorState.Idle);

        }

        public void OnChangeNPCType(NPCType before, NPCType after)
        {
            if(movement.isMoving)
            {
                stateMachine.Transition(Transition.OnStateChange, BehaviorState.Moving);
            }
            else
            {
                stateMachine.Transition(Transition.OnStateChange, BehaviorState.Idle);
            }
        }

        public void OnWantsToCastChange(AbilityInfo ability, bool wantsToCast)
        {

            if(wantsToCast)
            {
                if (ability.target)
                {
                    if (entityInfo.CanAttack(ability.target))
                    {
                        stateMachine.Transition(Transition.OnCastStart, BehaviorState.Attacking);
                    }
                    else
                    {
                        stateMachine.Transition(Transition.OnCastStart, BehaviorState.Assisting);
                    }
                }
            }
            else
            {
                if(!ability.isCasting)
                {
                    if(movement.isMoving)
                    {
                        stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Moving);
                    }
                    else
                    {
                        stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Idle);
                    }

                }

            }

            
        }

        public void OnTryCast(AbilityInfo ability)
        {
        }

        


        public void OnCastStart(AbilityInfo ability)
        {
            if(ability.target == null)
            {
                stateMachine.Transition(Transition.OnCastStart, BehaviorState.Casting);
            }

            if (entityInfo.CanAttack(ability.target))
            {
                stateMachine.Transition(Transition.OnCastStart, BehaviorState.Attacking);
            }

            else if(entityInfo.CanHelp(ability.target))
            {
                stateMachine.Transition(Transition.OnCastStart, BehaviorState.Assisting);
            }
        }

        public void OnCancelCast(AbilityInfo ability)
        {
            if(!movement.isMoving)
            {
                stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Idle);
            }
            else
            {
                stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Moving);
            }
        }
        
        public void OnCastRelease(AbilityInfo ability)
        {
            if (!movement.isMoving)
            {
                stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Idle);
            }
            else
            {
                stateMachine.Transition(Transition.OnCastEnd, BehaviorState.Moving);
            }
        }

        public void OnLifeChange(bool isAlive)
        {
            stateMachine.TransitionAnyState(isAlive ? BehaviorState.Idle : BehaviorState.Inactive);
        }
    }

}
