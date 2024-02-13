using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using UnityEditor;

namespace Architome
{
    public class AugmentReturn : AugmentType
    {
        [Header("Return Properties")]
        Dictionary<CatalystInfo, bool> catalystsReturning;

        public bool appliesBuffs, appliesHealing;


        protected override void GetDependencies()
        {
            EnableCatalyst();

            catalystsReturning = new();
        }

        protected override string Description()
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
            catalystsReturning.Add(catalyst, false);
            HandleChecks(catalyst);
            catalyst.OnTickChange += OnCatalystTickChange;
            catalyst.OnCantFindEntity += OnCatalystCantFindTarget;
            catalyst.OnCatalystTrigger += OnCatalystTrigger;
            catalyst.OnCloseToTarget += OnCatalystCloseToTarget;


            
        }

        void HandleChecks(CatalystInfo catalyst)
        {

            bool appliedBuff = false;
            bool appliedHeal = false;
            if (appliesBuffs || appliesHealing)
            {
                catalyst.OnCanHitCheck += delegate (CatalystHit hit, EntityInfo entity, List<bool> checks)
                {
                    if (entity != augment.entity) return;
                    if (!catalystsReturning[catalyst]) return;
                    
                    hit.forceHit = true;


                    ApplyReturnAction(catalyst);
                };
            }


            if (appliesBuffs)
            {

                catalyst.OnCanAssistCheck += delegate (CatalystHit hit, EntityInfo entity, List<bool> checks) {
                    if (entity != augment.entity) return;
                    if (!catalystsReturning[catalyst]) return;
                    if (appliedBuff) return;
                    checks.Add(true);
                    appliedBuff = true;
                };
            }

            if (appliesHealing)
            {
                catalyst.OnCanHealCheck += delegate (CatalystHit hit, EntityInfo entity, List<bool> checks)
                {
                    if (entity != augment.entity) return;
                    if (!catalystsReturning[catalyst]) return;
                    if (appliedHeal) return;
                    checks.Add(true);
                    appliedHeal = true;
                };
            }
        }

        private void OnCatalystCantFindTarget(CatalystInfo catalyst)
        {
            ReturnCatalyst(catalyst);
        }

        void OnCatalystTickChange(CatalystInfo catalyst, int ticks)
        {
            if (ticks > 0) return;
            if (catalystsReturning[catalyst] == true) return;
            ReturnCatalyst(catalyst);
        }

        async void ReturnCatalyst(CatalystInfo catalyst)
        {
            Debugger.Combat(5914, $"{catalyst} is returning");
            if (catalystsReturning[catalyst]) return;
            catalystsReturning[catalyst] = true;


            await Task.Yield();

            catalyst.IncreaseTicks(false);
            

            catalyst.metrics.target = augment.entity.gameObject;

            HandleTargetting();

            catalyst.transform.LookAt(augment.entity.transform);


            if(V3Helper.Distance(catalyst.transform.position, augment.entity.transform.position) < 1f)
            {
                var hit = catalyst.GetComponent<CatalystHit>();
                hit.HandleTargetHit(augment.entity);
            }

            async void HandleTargetting()
            {
                while (catalystsReturning[catalyst])
                {
                    await Task.Yield();
                    if (this == null) return;
                    if(catalyst.target != augment.entity.target)
                    {
                        catalyst.target = augment.entity.gameObject;
                    }
                }
            }
        }

        void OnCatalystTrigger(CatalystInfo catalyst, Collider other, bool enter)
        {
            if (!catalystsReturning[catalyst]) return;

        }

        void OnCatalystCloseToTarget(CatalystInfo catalyst, GameObject target)
        {
        }



        void ApplyReturnAction(CatalystInfo catalyst)
        {

            

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
