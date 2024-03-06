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
            var combatEventData = new CombatEvent(buffInfo);
            combatBehavior.OnCanFocusCheck += OnCanFocusCheck;

            if (buffInfo.hostInfo.AbilityManager().attackAbility)
            {
                combatEventData.SetFixate(true);
                //buffInfo.targetInfo.combatEvents.OnFixate?.Invoke(combatEventData, true);
                targetCombatEvent.InvokeGeneral(eCombatEvent.OnFixateChange, combatEventData);
                buffInfo.hostInfo.AbilityManager().target = buffInfo.targetInfo;
                buffInfo.hostInfo.AbilityManager().Attack();
                buffInfo.hostInfo.AbilityManager().target = null;
            }

            buffInfo.OnBuffEnd += delegate (BuffInfo buff)
            {
                combatBehavior.OnCanFocusCheck -= OnCanFocusCheck;
                combatEventData.SetFixate(false);
                //buffInfo.targetInfo.combatEvents.OnFixate?.Invoke(eventData, false);
                targetCombatEvent.InvokeGeneral(eCombatEvent.OnFixateChange, combatEventData);

                isFixating = false;
                buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocusTarget, $"Setting original focus target");
            };

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
