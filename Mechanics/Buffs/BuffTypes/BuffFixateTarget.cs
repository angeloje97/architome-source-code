using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffFixateTarget : BuffType
    {
        // Start is called before the first frame update
        public EntityInfo originalFocusTarget;
        public bool isFixating;
        new void GetDependencies()
        {
            base.GetDependencies();

            buffInfo.OnBuffStart += delegate (BuffInfo buff)
            {
                ApplyBuff();

            };
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

            var combatBehavior = buffInfo.hostInfo.CombatBehavior();

            originalFocusTarget = combatBehavior.GetFocus();

            isFixating = true;
            combatBehavior.SetFocus(buffInfo.targetInfo, null);

            combatBehavior.OnCanFocusCheck += OnCanFocusCheck;

            buffInfo.OnBuffEnd += delegate (BuffInfo buff)
            {
                combatBehavior.OnCanFocusCheck -= OnCanFocusCheck;
                var eventData = new CombatEventData(buff, buff.properties.value);
                buffInfo.targetInfo.combatEvents.OnFixate?.Invoke(eventData, false);
                isFixating = false;
                buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocusTarget, $"Setting original focus target");

            };

            if (buffInfo.hostInfo.AbilityManager().attackAbility)
            {
                var combatEventData = new CombatEventData(buffInfo, buffInfo.properties.value);
                buffInfo.targetInfo.combatEvents.OnFixate?.Invoke(combatEventData, true);
                buffInfo.hostInfo.AbilityManager().target = buffInfo.targetInfo;
                buffInfo.hostInfo.AbilityManager().Attack();
                buffInfo.hostInfo.AbilityManager().target = null;
            }

            void OnCanFocusCheck(EntityInfo entity, EntityInfo target, List<bool> setFocusChecks)
            {
                setFocusChecks.Add(target == buffInfo.targetInfo);
                
            }
        }
        void Start()
        {
            GetDependencies();
        }



        public override string Description()
        {
            var description = new List<string>()
            {
                "Forces the caster to focus a target",
            };


            if (buffInfo.targetInfo)
            {
                description.Add($"- Currently fixated on {buffInfo.targetInfo.entityName}.");
            }

            return ArchString.NextLineList(description);

        }

        public override string GeneralDescription()
        {
            return "Forced to focus a target.\n";
        }
    }

}
