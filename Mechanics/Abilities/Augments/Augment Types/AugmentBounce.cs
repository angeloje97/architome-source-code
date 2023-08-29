using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentBounce : AugmentType
    {
        [Header("Bounce Properties")]
        public bool requiresLos;
        public bool canBounceOnSelf;
        public bool seekTargets;
        public bool structureBounce;
        public float radius;


        [SerializeField] bool manifestAbilityRequiresLOS;

        LayerMask structureLayerMask;
        LayerMask entityLayer;

        async void Start()
        {
            await GetDependencies(() => {
                EnableCatalyst();

                var layerMaskData = LayerMasksData.active;
                if (layerMaskData != null)
                {
                    structureLayerMask = layerMaskData.structureLayerMask;
                    entityLayer = layerMaskData.entityLayerMask;
                }

                if (manifestAbilityRequiresLOS)
                {
                    requiresLos = augment.ability.restrictions.requiresLineOfSight;
                }

                if (ability.abilityType == AbilityType.LockOn)
                {
                    seekTargets = true;
                }
                else
                {
                    structureBounce = true;
                }
            });
        }

        protected override string Description()
        {
            return $"Bounces between targets that are {radius} meters between each other.";
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnTickChange += OnTickChange;

            //catalyst.OnHit += OnCatalystHit;

            catalyst.OnHit += OnCatalystHit;

            catalyst.OnStructureHit += OnCatalystStructureHit;

            catalyst.OnDeadTarget += (GameObject target) => OnDeadTarget(catalyst, target);
        }

        void OnTickChange(CatalystInfo catalyst, int ticks)
        {
        }

        void OnCatalystHit(CatalystInfo catalyst, EntityInfo entity)
        {
            if (catalyst.Ticks() == 0) return;
            SetCatalyst(catalyst, true);

            HandleNearTargets();

            catalyst.ResetStartingPosition();

            if (!LookForNewTarget(entity, catalyst))
            {
                ReflectCatalyst(catalyst);
            }

            SetCatalyst(catalyst, false);
        }

        void OnCatalystStructureHit(CatalystInfo catalyst, Collider collider)
        {
            if (!structureBounce) return;
            SetCatalyst(catalyst, true);


            if (!LookForNewTarget(null, catalyst))
            {
                ReflectCatalyst(catalyst);
                augment.TriggerAugment(new(this));
            }

            SetCatalyst(catalyst, false);
        }

        void ReflectCatalyst(CatalystInfo catalyst)
        {
            if (catalyst == null) return;

            var ray = new Ray(catalyst.transform.position, catalyst.transform.forward);


            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                var newDirection = Vector3.Reflect(catalyst.transform.forward, hit.normal);

                var newPosition = catalyst.transform.position + (newDirection * 5);

                newPosition.y = catalyst.transform.position.y;

                catalyst.transform.LookAt(newPosition);
            }
        }

        void OnDeadTarget(CatalystInfo catalyst, GameObject target)
        {
            if (catalyst.Ticks() == 0) return;
            var info = target.GetComponent<EntityInfo>();
            SetCatalyst(catalyst, true);

            if (LookForNewTarget(info, catalyst))
            {
                ReflectCatalyst(catalyst);
            }

            SetCatalyst(catalyst, false);
        }

        public void HandleNearTargets()
        {

            var position = activeCatalyst.transform.position;

            var entitiesWithinRange = activeCatalyst.EntitiesWithinRadius(2f, requiresLos);

            foreach (var entity in entitiesWithinRange)
            {

                if (!activeHit.CanHit(entity)) continue;

                if (requiresLos)
                {
                    var direction = V3Helper.Direction(entity.transform.position, position);
                    var distance = V3Helper.Distance(entity.transform.position, position);

                    var ray = new Ray(position, direction);

                    if (Physics.Raycast(ray, out RaycastHit hit, distance, structureLayerMask)) continue;
                }

                activeHit.HandleTargetHit(entity);
            }
        }

        public bool LookForNewTarget(EntityInfo targetHit, CatalystInfo activeCatalyst)
        {
            if (activeCatalyst.Ticks() == 0) return false;
            if (!seekTargets) return false;
            ArchAction.Yield(() => {
                activeCatalyst.ResetStartingPosition();
            });

            NPCType priority;

            if (targetHit != null)
            {
                priority = targetHit.GetComponent<EntityInfo>().npcType;
            }
            else
            {
                priority = augment.ability.entityInfo.EnemyType();
            }

            var entities = activeCatalyst.EntitiesWithinRadius(radius, requiresLos);

            if (FindEntity(priority))
            {
                augment.TriggerAugment(new(this));
                return true;
                
            }

            foreach (NPCType type in Enum.GetValues(typeof(NPCType)))
            {
                if (type == priority) continue;
                if (FindEntity(type))
                {
                    augment.TriggerAugment(new(this));
                    return true;
                }
            }


            activeCatalyst.OnCantFindEntity?.Invoke(activeCatalyst);

            return false;

            bool FindEntity(NPCType priorityNPC)
            {

                foreach (var entity in entities)
                {
                    if(!activeHit.CanHit(entity)) continue;
                    if(entity == targetHit) continue;
                    if (entity == activeCatalyst.entityInfo) continue;
                    if(!entity.isAlive) continue;
                    if (entity.npcType != priorityNPC) continue;

                    if (requiresLos)
                    {
                        if (V3Helper.IsObstructed(entity.transform.position, activeCatalyst.transform.position, structureLayerMask)) continue;
                    }

                    activeCatalyst.target = entity.gameObject;
                    activeCatalyst.transform.LookAt(entity.transform);


                    return true;
                }

                return false;
            }
        }

    }
}
