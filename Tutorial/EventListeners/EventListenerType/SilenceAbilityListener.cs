using System;
using System.Collections;
using UnityEngine;

namespace Architome.Tutorial
{
    public class SilenceAbilityListener : EventListener
    {
        [Header("Silence Ability Listener")]

        public AbilityInfo abilityToSilence, silencingAbility;

        bool abilityUsed;


        void Start()
        {
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            Action<AbilityInfo> action1 = (AbilityInfo ability) => {
                abilityUsed = true;

                ArchAction.Delay(() => {
                    abilityUsed = false;
                }, 2);
            };

            Action<AbilityInfo> action2 = (AbilityInfo ability) => {
                if (!abilityUsed) return;

                CompleteEventListener();
            };


            if(!silencingAbility)
            {
                abilityUsed = true;
            }
            else
            {
                silencingAbility.OnSuccessfulCast += action1;
            }

            abilityToSilence.OnInterrupt += action2;
            PreventEntityDeath(abilityToSilence.entityInfo);


            OnSuccessfulEvent += (EventListener eventListener) => {

                if (silencingAbility)
                {
                    silencingAbility.OnSuccessfulCast -= action1;
                }

                abilityToSilence.OnInterrupt -= action2;
            };
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