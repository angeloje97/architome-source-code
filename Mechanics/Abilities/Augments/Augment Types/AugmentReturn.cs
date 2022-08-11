using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentReturn : AugmentType
    {
        [Header("Return Properties")]
        Dictionary<CatalystInfo, bool> catalystsReturning;

        public bool appliesBuffs, appliesHealing;

        void Start()
        {
            GetDependencies();
        }

        async new void GetDependencies()
        {
            await base.GetDependencies();

            EnableCatalyst();

            catalystsReturning = new();
        }

        public override string Description()
        {
            var result = "";

            result += $"Catalyst returns to caster";

            var properties = "";

            if (appliesBuffs)
            {
                properties += $" applying any helpful buffs attached to the ability";
            }

            if (appliesHealing)
            {
                if (properties.Length > 0)
                {
                    properties += " and also";
                }

                properties += " healing for the abilitiy's original value";
            }

            result += $"{properties}.";

            return result;
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnTickChange += OnCatalystTickChange;
            catalyst.OnCantFindEntity += OnCatalystCantFindTarget;
            catalyst.OnCatalystTrigger += OnCatalystTrigger;
            catalyst.OnCloseToTarget += OnCatalystCloseToTarget;

            catalystsReturning.Add(catalyst, false);
            
        }

        private void OnCatalystCantFindTarget(CatalystInfo catalyst)
        {
            ReturnCatalyst(catalyst);
        }

        void OnCatalystTickChange(CatalystInfo catalyst, int ticks)
        {
            if (ticks != 0) return;
            if (catalystsReturning[catalyst] == true) return;
            ReturnCatalyst(catalyst);
        }

        void ReturnCatalyst(CatalystInfo catalyst)
        {
            Debugger.Combat(5914, $"{catalyst} is returning");
            if (catalystsReturning[catalyst]) return;
            catalystsReturning[catalyst] = true;
            catalyst.target = catalyst.entityObject;
            catalyst.transform.LookAt(catalyst.entityObject.transform);
        }

        void OnCatalystTrigger(CatalystInfo catalyst, Collider other, bool enter)
        {
            if(other.gameObject != catalyst.entityObject) return;
            if (!catalystsReturning[catalyst]) return;

            ApplyReturnAction(catalyst);
        }

        void OnCatalystCloseToTarget(CatalystInfo catalyst, GameObject target)
        {
            if (!catalystsReturning[catalyst]) return;
            if (target != catalyst.target) return;

            ApplyReturnAction(catalyst);
        }



        void ApplyReturnAction(CatalystInfo catalyst)
        {
            var hit = catalyst.GetComponent<CatalystHit>();

            if (appliesBuffs)
            {
                hit.isAssisting = true;
                hit.canSelfCast = true;
            }

            if (appliesHealing)
            {
                hit.canSelfCast = true;
                hit.isHealing = true;
            }

            if (hit.isHealing || hit.isAssisting)
            {
                catalyst.IncreaseTicks();
            }

            hit.HandleTargetHit(catalyst.entityInfo);

            ArchAction.Yield(() => {
                catalystsReturning[catalyst] = false;
                catalyst.OnReturn?.Invoke(catalyst);
            });
        }


        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
