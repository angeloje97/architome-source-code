using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class ArchSummonAgent : EntityProp
    {
        public EntityInfo master;
        public ThreatManager threatManager;
        public AIBehavior behavior;
        public AbilityManager ability;
        public EntityInfo.SummonedEntity summoning;
        public CombatBehavior combat;
        public CharacterInfo character;

        public float liveTime;

        new void GetDependencies()
        {
            base.GetDependencies();

            master = entityInfo.summon.master;
            
            liveTime = entityInfo.summon.timeRemaining;
            summoning = entityInfo.summon;
            behavior = GetComponentInParent<AIBehavior>();

            character = entityInfo.CharacterInfo();
            combat = behavior.GetComponentInChildren<CombatBehavior>();
            threatManager = behavior.GetComponentInChildren<ThreatManager>();

            entityInfo.OnDamageDone += OnDamageDone;
            

        }
        void Start()
        {
            GetDependencies();
            HandleEvents(true);
            DisableConflicts();
            AcquireThreats();
            DeathTimer();
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
            }
        }

        void DisableConflicts()
        {
            var noCombat = behavior.GetComponentInChildren<NoCombatBehavior>();
            if (noCombat) noCombat.enabled = false;
        }

        void OnDamageDone(CombatEventData eventData)
        {
            master.OnDamageDone?.Invoke(eventData);
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
                    var value = threatManager.ThreatMultiplier(threat.threatObject.GetComponent<EntityInfo>());
                    
                    //threatManager.IncreaseThreat(threat.threatObject, value);
                    entityInfo.combatEvents.OnPingThreat?.Invoke(threat.threatInfo, value);
                }

                //threatManager.Bump();
            }, .50f);
        }
        async void DeathTimer()
        {
            while (!entityInfo.isAlive)
            {
                await Task.Yield();
            }

            while (liveTime > 0)
            {
                await Task.Yield();
                liveTime -= Time.deltaTime;
                if (!entityInfo.isAlive) break;
            }

            HandleEvents(false);
            entityInfo.Die();
        }

    }
}
