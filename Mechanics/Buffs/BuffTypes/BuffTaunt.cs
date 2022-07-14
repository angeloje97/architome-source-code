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
        public GameObject originalFocus;


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
                stateChanger.OnStateChangerEnd += OnStateChangerEnd;

            }
            
        }
        void Awake()
        {
            GetDependencies();
        }

        // Update is called once per frame
        public override string Description()
        {
            var description = "Forces unit to focus caster.\n";

            if (buffInfo && buffInfo.sourceInfo)
            {
                description += $"Currently focusing {buffInfo.sourceInfo.name}\n";
            }
            return description;
        }

        public override string GeneralDescription()
        {
            return $"Forces a unit to focus the source of the buff.\n";
        }

        void OnSuccessfulStateChange(BuffStateChanger stateChanger, EntityState state)
        {
            if (state != EntityState.Taunted) return;
            if (buffInfo == null) { return; };

            originalFocus = buffInfo.hostInfo.CombatBehavior().GetFocus();
            buffInfo.hostInfo.AIBehavior().CombatBehavior().SetFocus(buffInfo.sourceObject);
            
        }

        void OnStateChangerEnd(BuffStateChanger stateChanger, EntityState newState)
        {
            if (buffInfo == null) { return; }
            if (newState == EntityState.Taunted)
            {
                return;
            }

            
            buffInfo.hostInfo.CombatBehavior().SetFocus(originalFocus);

        }
    }

}
