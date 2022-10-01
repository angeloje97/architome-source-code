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
            ability.OnCatalystRelease += OnCatalystRelease;

            if (particularTarget)
            {
                PreventEntityDeath(particularTarget);
            }


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
            var actionBarName = $"Ability{currentActionBarBehavior.actionBarNum}";

            var newDirection = $"Use {ability} ";

            if (particularTarget)
            {
                newDirection += $"on {particularTarget} ";
            }

            newDirection += $"by hovering over an enemy and using the <sprite={keyBindData.SpriteIndex(actionBarName)}> button.";

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


            
            if (particularTarget)
            {
                Action<bool> action = delegate (bool isAlive)
                {
                    if (!isAlive)
                    {
                        CompleteEventListener();
                    }
                };

                particularTarget.OnLifeChange += action;

                OnSuccessfulEvent += delegate (EventListener listener) {
                    particularTarget.OnLifeChange -= action;
                };

                return;
            }

            CompleteEventListener();
        }
    }
}
