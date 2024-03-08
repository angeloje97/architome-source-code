using System.Collections;
using System.Collections.Generic;
using Architome.Enums;
using UnityEngine;

namespace Architome
{
    public class BuffInstantKill : BuffType
    {
        // Start is called before the first frame update
        void Start()
        {

            GetDependencies();
            ApplyBuff();
        }

        private void OnValidate()
        {
            var buffInfo = GetComponent<BuffInfo>();

            if (buffInfo == null) return;
            buffInfo.damageType = DamageType.True;
        }

        public new void GetDependencies()
        {
            base.GetDependencies();
        }

        void ApplyBuff()
        {
            if (!buffInfo.hostInfo.isAlive) return;

            var host = buffInfo.hostInfo;
            if (host == null) return;

            if (host.states.Contains(EntityState.Immune))
            {
                var stateEventData = new StateChangeEvent(buffInfo, new() { EntityState.Immune });
                host.RemoveState(EntityState.Immune, stateEventData);
            }

            var combatEventData = new HealthEvent(buffInfo, host.health);

            host.Damage(combatEventData);

            if (host.isAlive)
            {
                host.Die();
            }

            buffInfo.CompleteEarly();
        }

        public override string GeneralDescription()
        {
            return "Instantly kills the target";

        }
    }
}
