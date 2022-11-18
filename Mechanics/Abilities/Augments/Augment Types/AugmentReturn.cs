using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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
            if (ticks > 0) return;
            if (catalystsReturning[catalyst] == true) return;
            ReturnCatalyst(catalyst);
        }

        void ReturnCatalyst(CatalystInfo catalyst)
        {
            Debugger.Combat(5914, $"{catalyst} is returning");
            if (catalystsReturning[catalyst]) return;
            catalystsReturning[catalyst] = true;
            catalyst.metrics.target = augment.entity.gameObject;

            HandleTargetting();

            //catalyst.OnCanAssistCheck += (CatalystHit hit, EntityInfo target, List<bool> checks) => {
            //    if(target == augment.entity && appliesBuffs)
            //    {
            //        hit.forceAssist = true;
            //    }
            //};

            //catalyst.OnCanHealCheck += (CatalystHit hit, EntityInfo target, List<bool> checks) => {
            //    if (target == augment.entity && appliesHealing)
            //    {
            //        hit.forceHeal = true;
            //    }
            //};

            catalyst.transform.LookAt(augment.entity.transform);

            async void HandleTargetting()
            {
                while (catalystsReturning[catalyst])
                {
                    await Task.Yield();
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

            ApplyReturnAction(catalyst, other.gameObject);
        }

        void OnCatalystCloseToTarget(CatalystInfo catalyst, GameObject target)
        {
            if (!catalystsReturning[catalyst]) return;

            ApplyReturnAction(catalyst, target);
        }



        void ApplyReturnAction(CatalystInfo catalyst, GameObject target)
        {
            var hit = catalyst.GetComponent<CatalystHit>();
            if (target != augment.entity.gameObject) return;

            if (appliesBuffs)
            {
                hit.isAssisting = true;
                hit.canSelfCast = true;
                hit.forceAssist = true;
                hit.forceHit = true;
            }

            if (appliesHealing)
            {
                hit.canSelfCast = true;
                hit.isHealing = true;
                hit.forceHeal = true;
                hit.forceHit = true;
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
