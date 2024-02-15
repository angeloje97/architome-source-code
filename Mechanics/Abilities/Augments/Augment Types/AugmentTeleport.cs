using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AugmentTeleport : AugmentType
    {
        #region Common Data

        [Header("Teleport Properties")]
        public bool teleportsBehindTarget;

        float heightOffset;

        CharacterInfo entityCharacter;
        LayerMask walkableLayer;
        LayerMask obstructionLayer;

        #endregion

        protected override void GetDependencies()
        {       
            EnableSuccesfulCast();
            EnableCatalyst();

            var layerDatas = LayerMasksData.active;

            if (layerDatas)
            {
                walkableLayer = layerDatas.walkableLayer;
                obstructionLayer = layerDatas.structureLayerMask;
            }

            var boxCollider = augment.entity.GetComponent<BoxCollider>();

            heightOffset = .5f * boxCollider.size.y;

            entityCharacter = augment.entity.GetComponentInChildren<CharacterInfo>();

        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            var target = catalyst.target;

            catalyst.OnCatalystDestroy += OnCatalystDestroy;
        }

        public void OnCatalystDestroy(CatalystDeathCondition condition)
        {
            var catalyst = condition.catalystInfo;
            SetCatalyst(catalyst, true);

            var target = catalyst.target;

            HandleTarget();
            HandleFreeCast();


            void HandleTarget()
            {
                if (target == null) return;

                if (teleportsBehindTarget)
                {
                    var character = target.GetComponentInChildren<CharacterInfo>();
                    var behindPosition = target.transform.position - (character.transform.forward * 2f);

                    var direction = V3Helper.Direction(behindPosition, target.transform.position);
                    var ray = new Ray(character.transform.position, direction);

                    if (Physics.Raycast(ray, out RaycastHit hit, 1f, obstructionLayer))
                    {
                        var distance = V3Helper.Distance(hit.point, target.transform.position);
                        behindPosition = target.transform.position - (character.transform.forward * distance * .90f);
                    }

                    var groundPosition = V3Helper.GroundPosition(behindPosition, walkableLayer, 0f, heightOffset);

                    if (groundPosition == new Vector3(0, 0, 0))
                    {
                        behindPosition = target.transform.position;
                    }

                    augment.entity.transform.position = behindPosition;
                    entityCharacter.LookAt(target.transform);
                    augment.TriggerAugment(new(this));
                    return;
                }

                var direction2 = V3Helper.Direction(augment.entity.transform.position, target.transform.position);

                var newPosition = target.transform.position + (direction2 * 3f);

                var fixedPosition = FixedPosition(newPosition);

                augment.entity.transform.position = fixedPosition;
                augment.TriggerAugment(new(this));
            }
            void HandleFreeCast()
            {
                if (ability.abilityType == AbilityType.LockOn) return;

                var fixedPosition = FixedPosition(catalyst.transform.position);

                augment.entity.transform.position = fixedPosition;
            }
        }

        public override void HandleSuccessfulCast(AbilityInfo ability)
        {
            
        }

        protected override string Description()
        {
            var result = "";

            result += "When the catalyst gets destroyed, the caster will teleport to it.";

            var teleportString = teleportsBehindTarget ? "behind" : "to";

            result += $" If this is attached to a lock on ability the caster will teleport {teleportString} the target.";

            return result;
        }

        public Vector3 FixedPosition(Vector3 position)
        {

            var groundPosition =  V3Helper.GroundPosition(position, walkableLayer, 0f, heightOffset);

            if (groundPosition == new Vector3(0, 0, 0))
            {
                return position;
            }

            return groundPosition;
        }

    }
}
