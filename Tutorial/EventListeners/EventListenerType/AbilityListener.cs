using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class AbilityListener : EventListener
    {
        [Header("Ability Listener Properties")]
        public AbilityInfo ability;
        public EntityInfo particularTarget;
        public string keybindName;
        public bool deactiveAbilityUntilActive;

        ActionBarBehavior currentActionBarBehavior;
        

        void Start()
        {
            GetDependencies();

            if (ability == null) return;

            HandleStart();
            ArchAction.Delay(() => {
                HandleDeactivateAbility();
            }, 3f);
            HandleDeactivateAbility();
        }

        void HandleDeactivateAbility()
        {
            if (!deactiveAbilityUntilActive) return;

            Action<AbilityInfo, bool> action = (AbilityInfo ability, bool active) => {
                if (active)
                {
                    ability.active = false;
                }
            }; 

            ability.active = false;

            ability.OnActiveChange += action;

            OnStartEvent += (EventListener listener) => {
                ability.OnActiveChange -= action;
                ability.active = true;

            };

        }

        public override void StartEventListener()
        {
            //if (ability == null) return;
            base.StartEventListener();
            ability.OnSuccessfulCast += OnSuccesfulCast;


            OnSuccessfulEvent += (EventListener listener) => {
                ability.OnSuccessfulCast -= OnSuccesfulCast;
            };
            GetCurrentActionBarBehavior();
        }

        void GetCurrentActionBarBehavior()
        {
            var actionBarBehaviors = ActionBarsInfo.active.GetComponentsInChildren<ActionBarBehavior>();

            foreach(var actionBar in actionBarBehaviors)
            {
                if (actionBar.abilityInfo == ability)
                {
                    currentActionBarBehavior = actionBar;
                    break;
                }
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

        public void OnSuccesfulCast(AbilityInfo ability)
        {
            if (particularTarget && ability.targetLocked != particularTarget.gameObject)
            {
                return;
            }

            CompleteEventListener();

        }
    }
}
