using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;
using System;
using PixelCrushers.DialogueSystem;

namespace Architome
{
    public class ArchSummonAgent : EntityProp
    {

        [Serializable]
        public class DeathTimer
        {
            public float liveTime;

            public async void HandleDeathTimer(EntityInfo entity, Action onTimerEnd)
            {
                while (!entity.isAlive) await Task.Yield();

                while(liveTime > 0f)
                {
                    liveTime -= Time.deltaTime;
                    await Task.Yield();

                    if (!entity.isAlive)
                    {
                        break;
                    }
                }

                onTimerEnd?.Invoke();
            }
        }

        public EntityInfo master;
        public ThreatManager threatManager;
        public AIBehavior behavior;
        public AbilityInfo sourceAbility;
        public EntityInfo.SummonedEntity summoning;
        public CombatBehavior combat;
        public CharacterInfo character;

        [SerializeField] DeathTimer deathTimerHandler;

        public float liveTime;

        public override void GetDependencies()
        {
            master = entityInfo.summon.master;

            liveTime = entityInfo.summon.timeRemaining;
            summoning = entityInfo.summon;
            behavior = GetComponentInParent<AIBehavior>();

            character = entityInfo.CharacterInfo();
            combat = behavior.GetComponentInChildren<CombatBehavior>();
            threatManager = behavior.GetComponentInChildren<ThreatManager>();

            combatEvents.AddListenerHealth(eHealthEvent.OnDamageDone, OnDamageDone, this);

            sourceAbility = entityInfo.summon.sourceAbility;


            HandleEvents(true);
            DisableConflicts();
            AcquireThreats();


            deathTimerHandler = new() { liveTime = liveTime };
            deathTimerHandler.HandleDeathTimer(entityInfo, () => {
                HandleEvents(false);

                if (entityInfo.isAlive)
                {
                    entityInfo.Die();
                }
            });

        }


        void HandleEvents(bool enter)
        {
            if (enter)
            {
                if (summoning.diesOnMasterDeath)
                {
                    master.OnLifeChange += OnMasterLifeChange;
                    if (!master.isAlive) liveTime = 0f;
                }

                if (summoning.diesOnMasterCombatFalse)
                {
                    master.OnCombatChange += OnMasterCombatChange;
                    if (!master.isInCombat) liveTime = 0f;
                }

                if (sourceAbility)
                {
                    sourceAbility.OnRemoveAbility += HandleRemoveAbility;
                }
            }
            else
            {
                if (summoning.diesOnMasterDeath)
                {
                    master.OnLifeChange -= OnMasterLifeChange;
                }

                if (summoning.diesOnMasterCombatFalse)
                {
                    master.OnCombatChange -= OnMasterCombatChange;
                }

                if (sourceAbility)
                {
                    sourceAbility.OnRemoveAbility -= HandleRemoveAbility;
                }
            }
        }

        void HandleRemoveAbility(AbilityInfo ability)
        {
            deathTimerHandler.liveTime = 0f;
        }

        void DisableConflicts()
        {
            var noCombat = behavior.GetComponentInChildren<NoCombatBehavior>();
            if (noCombat) noCombat.enabled = false;
        }

        void OnDamageDone(HealthEvent eventData)
        {

            master.combatEvents.InvokeHealthChange(eHealthEvent.OnDamageDone, eventData);
        }

        void OnMasterLifeChange(bool isAlive)
        {
            if (isAlive) return;
            liveTime = 0f;
        }

        void OnMasterCombatChange(bool isInCombat)
        {
            if (isInCombat) return;
            liveTime = 0f;
        }

        void AcquireThreats()
        {
            character.LookAt(master.transform);
            ArchAction.Delay(() => {
                var masterThreat = master.GetComponentInChildren<ThreatManager>();

                foreach (var threat in masterThreat.threats)
                {
                    var value = threatManager.ThreatMultiplier(threat.threatInfo);
                    combatEvents.InvokeThreat(eThreatEvent.OnPingThreat, new(threat, value));
                }
            }, .50f);
        }

    }
}
