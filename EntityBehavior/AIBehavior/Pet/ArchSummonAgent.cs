using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System.Linq;

namespace Architome
{
    public class ArchSummonAgent : EntityProp
    {
        public EntityInfo master;
        public ThreatManager threatManager;
        public AIBehavior behavior;
        public AbilityManager ability;
        public CombatBehavior combat;
        public float liveTime;

        new void GetDependencies()
        {
            base.GetDependencies();

            master = entityInfo.summon.master;
            liveTime = entityInfo.summon.timeRemaining;
            behavior = GetComponentInParent<AIBehavior>();

            combat = behavior.GetComponentInChildren<CombatBehavior>();
            threatManager = behavior.GetComponentInChildren<ThreatManager>();

            entityInfo.OnDamageDone += OnDamageDone;
        }
        void Start()
        {
            GetDependencies();
            DisableConflicts();
            AcquireThreats();
            DeathTimer();
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

        void AcquireThreats()
        {
            ArchAction.Delay(() => {
                var masterThreat = master.GetComponentInChildren<ThreatManager>();

                foreach (var threat in masterThreat.threats)
                {
                    threatManager.IncreaseThreat(threat.threatObject, 15f);
                }

                threatManager.Bump();
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
                if (!entityInfo.isAlive) return;
            }

            entityInfo.Die();
        }

    }
}
