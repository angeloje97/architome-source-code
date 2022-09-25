using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
namespace Architome
{
    public class BuffTaunt : BuffType
    {
        // Start is called before the first frame update
        public BuffStateChanger stateChanger;
        public EntityInfo originalFocus;

        bool active;

        public new void GetDependencies()
        {
            base.GetDependencies();

            stateChanger = GetComponent<BuffStateChanger>();

            if (buffInfo == null) return;

            if (stateChanger)
            {
                if (stateChanger == null || buffInfo == null)
                {
                    return;
                }

                stateChanger.OnSuccessfulStateChange += OnSuccessfulStateChange;
                //stateChanger.OnStateChangerEnd += OnStateChangerEnd;

            }
            
        }
        void Awake()
        {
            GetDependencies();
        }

        // Update is called once per frame
        public override string Description()
        {

            var descriptions = new List<string>() {
                "Forces unit to focus caster."
            };

            if (buffInfo && buffInfo.sourceInfo)
            {
                descriptions.Add($"Currently focusing {buffInfo.sourceInfo.name}");
            }

            return ArchString.NextLineList(descriptions);
        }

        public override string GeneralDescription()
        {
            return $"Forces a unit to focus the source of the buff.";
        }

        void OnSuccessfulStateChange(BuffStateChanger stateChanger, EntityState state)
        {
            if (state != EntityState.Taunted) return;
            if (buffInfo == null) { return; };

            active = true;

            var combatBehavior = buffInfo.hostInfo.CombatBehavior();

            originalFocus = combatBehavior.GetFocus();

            combatBehavior.OnCanFocusCheck += OnCanFocusCheck;

            buffInfo.OnBuffEnd += delegate (BuffInfo buff) {
                combatBehavior.OnCanFocusCheck -= OnCanFocusCheck;

                buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocus);
            };

            Debugger.Combat(4395, $"Taunting target {buffInfo.hostInfo}");
            combatBehavior.SetFocus(buffInfo.sourceInfo);
            
            
            void OnCanFocusCheck(EntityInfo entity, EntityInfo target, List<bool> setFocusChecks)
            {
                if (!active) return;
                setFocusChecks.Add(target == buffInfo.sourceInfo);
                
            }
        }


        //void OnStateChangerEnd(BuffStateChanger stateChanger, EntityState newState)
        //{
        //    if (buffInfo == null) { return; }
        //    if (newState == EntityState.Taunted)
        //    {
        //        return;
        //    }

        //    active = false;
        //    buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocus);
        //}
    }

}
