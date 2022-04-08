using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffFixateTarget : BuffType
    {
        // Start is called before the first frame update
        public GameObject originalFocusTarget;
        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffEnd += OnBuffEnd;

            ApplyBuff();
        }

        public void ApplyBuff()
        {
            if (buffInfo.targetObject == null)
            {
                return;
            }

            if (!buffInfo.targetInfo.isAlive)
            {
                return;
            }

            originalFocusTarget = buffInfo.hostInfo.CombatBehavior().GetFocus();

            buffInfo.hostInfo.CombatBehavior().SetFocus(buffInfo.targetObject);

            if (buffInfo.hostInfo.AbilityManager().attackAbility)
            {
                var combatEventData = new CombatEventData(buffInfo, buffInfo.sourceInfo, buffInfo.properties.value);
                buffInfo.targetInfo.combatEvents.OnFixate?.Invoke(combatEventData);
                buffInfo.hostInfo.AbilityManager().target = buffInfo.targetObject;
                buffInfo.hostInfo.AbilityManager().Attack();
                buffInfo.hostInfo.AbilityManager().target = null;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnBuffEnd(BuffInfo buff)
        {
            buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocusTarget);
        }
    }

}
