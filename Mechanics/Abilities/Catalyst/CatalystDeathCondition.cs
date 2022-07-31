using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class CatalystDeathCondition : MonoBehaviour
    {
        // Start is called before the first frame update

        public AbilityInfo abilityInfo;
        public CatalystInfo catalystInfo;

        public AugmentProp.DestroyConditions conditions;

        public LayerMask structureLayer;

        public string destroyReason;
        public float destroyDelay = 0f;

        public void GetDependencies()
        {
            catalystInfo = GetComponent<CatalystInfo>();
            var layerMasksData = LayerMasksData.active;

            if (layerMasksData)
            {
                structureLayer = layerMasksData.structureLayerMask;
            }

            if (catalystInfo)
            {
                abilityInfo = catalystInfo.abilityInfo;

                catalystInfo.OnWrongTargetHit += OnWrongTargetHit;
                catalystInfo.OnTickChange += OnTickChange;
                catalystInfo.OnReturn += OnReturn;
                catalystInfo.OnCantFindEntity += OnCantFindEntity;
                catalystInfo.OnDeadTarget += OnDeadTarget;
            }

            if (abilityInfo)
            {
                conditions = catalystInfo.abilityInfo.destroyConditions;
                abilityInfo = catalystInfo.abilityInfo;
            }

            if (catalystInfo.isCataling)
            {
                conditions.destroyOnOutOfRange = true;
                conditions.destroyOnLiveTime = true;
                conditions.destroyOnStructure = true;
                conditions.destroyOnDeadTarget = true;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            HandleLiveTime();
            HandleRange();
            HandleDeadEntity();
        }
        public void OnTriggerEnter(Collider other)
        {
            if (structureLayer == (structureLayer | (1 << other.gameObject.layer)))
            {
                catalystInfo.OnStructureHit?.Invoke(catalystInfo, other);

                if (conditions.destroyOnStructure)
                {
                    DestroySelf("Collided with structure");
                }

            }


            //if (conditions.destroyOnStructure)
            //{
            //    var layermask = GMHelper.LayerMasks().structureLayerMask;
            //    var layer = other.gameObject.layer;
            //    if (layermask == (layermask | (1 << layer)))
            //    {
            //        DestroySelf("Collided with structure");
            //    }
            //}
        }

        void OnDeadTarget(GameObject target)
        {
            if (conditions.destroyOnDeadTarget)
            {
                DestroySelf("Target Died");
            }
        }

        public void OnTickChange(CatalystInfo catalyst, int ticks)
        {
            if (conditions.destroyOnNoTickDamage)
            {
                if (ticks <= 0)
                {
                    DestroySelf("No Ticks");
                }

            }
        }

        public void OnCantFindEntity(CatalystInfo catalyst)
        {
            if (conditions.destroyOnCantFindTarget)
            {
                DestroySelf("Cant Find Entity");
            }
        }

        public void OnWrongTargetHit(CatalystInfo catalyst, GameObject target)
        {
            DestroySelf("Wrong Target Hit");
        }

        public void HandleDeadEntity()
        {
            try
            {
                if (abilityInfo.targetsDead) { return; }
                if (abilityInfo.abilityType != AbilityType.LockOn) return;
                if (catalystInfo.target && !catalystInfo.target.GetComponent<EntityInfo>().isAlive)
                {
                    if (conditions.destroyOnDeadTarget)
                    {
                        DestroySelf("Dead Target");
                    }
                }

            }
            catch
            {
                transform.SetParent(CatalystManager.active.defectiveCatalysts);
            }
        }



        public void OnReturn(CatalystInfo catalyst)
        {
            if (gameObject == null) { return; }
            if (conditions.destroyOnReturn)
            {

                if (GetComponent<CatalystReturn>() && GetComponent<CatalystReturn>().hasReturned)
                {
                    DestroySelf("Returned");
                }
            }
        }
        public void HandleRange()
        {
            if (catalystInfo.range == -1) { return; }
            if (!conditions.destroyOnOutOfRange) return;
            if (catalystInfo.metrics.currentRange > catalystInfo.range)
            {
                DestroySelf("Out of Range");
            }

            if (gameObject.GetComponent<CatalystFreeScan>())
            {
                if (V3Helper.Abs(gameObject.transform.localScale) > catalystInfo.range * 2)
                {
                    DestroySelf("Out of Range");
                }
            }
        }
        public void HandleLiveTime()
        {
            if (catalystInfo == null || abilityInfo == null) return;
            if (!conditions.destroyOnLiveTime) return;
            if (catalystInfo.liveTime < abilityInfo.liveTime) return;

            DestroySelf("Expired");
        }


        public void DestroySelf(string reason = "Generic Destroy")
        {
            if (catalystInfo.isDestroyed) return;
            catalystInfo.isDestroyed = true;
            Disappear();
            try
            {
                catalystInfo.OnCatalystDestroy?.Invoke(this);
                destroyReason = reason;
                Debugger.InConsole(1294, $"Catalyst Destroyed for {reason}");

                ArchAction.Delay(() =>
                {
                    if (gameObject != null)
                    {
                        Destroy(gameObject);
                    }
                }, destroyDelay);
            }
            catch
            {

            }
        }

        void Disappear()
        {
            if (GetComponent<Renderer>())
            {
                GetComponent<Renderer>().enabled = false;

            }

            foreach (var renderer in GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            foreach (var light in GetComponentsInChildren<Light>())
            {
                light.enabled = false;
            }
        }

    }

}