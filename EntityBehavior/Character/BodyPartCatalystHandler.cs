using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BodyPartCatalystHandler
    {
        AbilityManager manager;
        CharacterBodyParts bodyParts;
        public BodyPartCatalystHandler(CharacterBodyParts bodyParts)
        {
            this.bodyParts = bodyParts;
            var entity = bodyParts.entity;
            if (entity == null) return;

            manager = entity.AbilityManager();
            if (manager == null) return;

            manager.OnCatalystRelease += HandleCatalystRelease;
        }

        void HandleCatalystRelease(AbilityInfo ability, CatalystInfo catalyst)
        {
            if (catalyst.isCataling) return;
            if (catalyst.effects.startingBodyPart == BodyPart.Root) return;
            if (ability.abilityType == AbilityType.Spawn) return;

            var bodyPart = catalyst.effects.startingBodyPart;

            var trans = bodyParts.BodyPartTransform(bodyPart);

            catalyst.transform.position = trans.position;
            catalyst.transform.LookAt(catalyst.location);
        }
    }
}
