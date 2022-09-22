using Architome.Tutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class HealPartyListener : EventListener
    {
        [Header("Heal Party Properties")]

        public PartyInfo partyTarget;
        public EntityInfo sourceHealer;

        public bool stopNaturalRegeneration;
        public bool hurtOnStart;
        [Range(0, 1)] public float hurtPercent;

        void Start()
        {
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            foreach (var member in partyTarget.members)
            {
                HandleStopRegen(member);
                HandleHurt(member);

                member.OnHealthChange += OnMemberHealthChange;
            }
        }


        void OnMemberHealthChange(float health, float shield, float maxHealth)
        {
            foreach (var member in partyTarget.members)
            {
                if (member.health != member.maxHealth) return;
                member.OnHealthChange -= OnMemberHealthChange;
            }

            CompleteEventListener();
        }

        void HandleStopRegen(EntityInfo member)
        {
            if (!stopNaturalRegeneration) return;
            var originalPercent = member.healthRegenPercent;

            member.healthRegenPercent = 0f;

            OnSuccessfulEvent += (EventListener listener) =>
            {
                member.healthRegenPercent = originalPercent;
            };
                
        }

        void HandleHurt(EntityInfo member)
        {
            if (!hurtOnStart) return;
            member.health = member.maxHealth - (member.maxHealth * hurtPercent);
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
