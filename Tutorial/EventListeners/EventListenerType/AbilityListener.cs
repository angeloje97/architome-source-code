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
        

        void Start()
        {
            if (ability == null) return;

            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();
            ability.OnSuccessfulCast += OnSuccesfulCast;
        }

        public void OnSuccesfulCast(AbilityInfo ability)
        {
            if (particularTarget && ability.targetLocked != particularTarget.gameObject)
            {
                return;
            }

            ActivateEventListener();

            if(activated)
            {
                ability.OnSuccessfulCast -= OnSuccesfulCast;
            }
        }
    }
}
