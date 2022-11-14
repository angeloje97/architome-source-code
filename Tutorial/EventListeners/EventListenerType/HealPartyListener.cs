using Architome.Tutorial;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome.Tutorial
{
    public class HealPartyListener : EventListener
    {
        [Header("Heal Party Properties")]

        public PartyInfo partyTarget;
        public EntityInfo sourceHealer;

        public bool stopNaturalRegeneration;
        public bool hurtOnStart;
        public bool cleanseBuffs;
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
                HandleCleanseBuffs(member);

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

        void HandleCleanseBuffs(EntityInfo member)
        {
            if (!cleanseBuffs) return;

            var buffsManager = member.Buffs();

            buffsManager.CleanseBuffs((buff) => { return true; });
        }
        public override string Directions()
        {
            var result = new List<string>()
            {
                base.Directions()
            };

            if (!simple)
            {
                var newDirection = $"To heal your allies, use the action move from your healer onto any ally (or themself) that needs healing same way you send out a party member to attack an enemy.";
                result.Add(newDirection);

            }

            return ArchString.NextLineList(result);
        }

        public override string Tips()
        {
            return base.Tips();
        }
    }
}
