using Architome.Enums;
using Architome.Settings.Keybindings;
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
        public bool fastCoolDown;
        public CatalystEvent completionEvent;

        ActionBarBehavior currentActionBarBehavior;

        

        void Start()
        {
            GetDependencies();

            if (ability == null) return;

            HandleStart();

            HandleFastCoolDown();
        }

        void HandleFastCoolDown()
        {
            if (!fastCoolDown) return;
            Action<AbilityInfo> OnSuccessfulCast = (AbilityInfo ability) => {
                ArchAction.Delay(() => {
                if (completed) return;
                if (ability.coolDown.timer > 1f)
                {
                        ability.coolDown.timer = 1f;
                    }
                }, 1f);
            };

            OnStartEvent +=  (EventListener lisntener) => {
                ability.OnSuccessfulCast += OnSuccessfulCast;
            };

            OnEndEvent += (EventListener listener) => {
                ability.OnSuccessfulCast -= OnSuccessfulCast;
            };
        }

        public override void StartEventListener()
        {
            //if (ability == null) return;
            base.StartEventListener();
            ability.OnCatalystRelease += OnCatalystRelease;


            OnSuccessfulEvent += (EventListener listener) => {
                ability.OnCatalystRelease -= OnCatalystRelease;
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
            var result = new List<string>() {
                base.Directions()
            };

            var newDirection = $"Use {ability} ";

            if (particularTarget)
            {
                newDirection += $"on {particularTarget} ";
            }

            var spriteIndex = keyBindData.SpriteIndex(KeybindSetType.Party, currentActionBarBehavior.keybindAlias);


            if (ability.abilityType == AbilityType.LockOn)
            {
                newDirection += $"by hovering over an ";
                newDirection += ability.isHealing ? "ally" : "enemy";
                newDirection += $" and using the <sprite={spriteIndex}> button.";

            }
            else
            {
                newDirection += $"by pressing <sprite={spriteIndex}>";
            }

            result.Add(newDirection);


            return ArchString.NextLineList(result);
        }

        public override string Tips()
        {
            return base.Tips();
        }

        public void OnCatalystRelease(CatalystInfo catalyst)
        {
            if (particularTarget && catalyst.target != particularTarget.gameObject)
            {
                return;
            }

            catalyst.AddEventAction(completionEvent, CompleteEventListener);

            
            //if (particularTarget)
            //{
            //    Action<bool> action = delegate (bool isAlive)
            //    {
            //        if (!isAlive)
            //        {
            //            CompleteEventListener();
            //        }
            //    };

            //    particularTarget.OnLifeChange += action;

            //    OnSuccessfulEvent += delegate (EventListener listener) {
            //        particularTarget.OnLifeChange -= action;
            //    };

            //    return;
            //}

            //CompleteEventListener();
        }
    }
}
