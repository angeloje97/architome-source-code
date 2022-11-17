using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public struct AbilityResources
    {
        public float requiredAmount;
        public float requiredPercent;
        public float producedAmount;
        public float producedPercent;


        public float ManaRequired(float maxMana)
        {
            return requiredAmount + (maxMana * requiredPercent);
        }

        public float ManaProduced(float maxMana)
        {
            return producedAmount + (producedPercent * maxMana);
        }
        public void Initiate(AbilityInfo ability)
        {
            ability.OnReadyCheck += OnReadyCheck;
            ability.OnSuccessfulCast += OnSuccessfulCast;
        }

        void OnReadyCheck(AbilityInfo ability, List<(string, bool)> readyChecks)
        {
            var entity = ability.entityInfo;
            if (entity == null) return;


            readyChecks.Add(("Sufficient Resources", entity.mana >= ManaRequired(entity.maxMana)));
        }

        void OnSuccessfulCast(AbilityInfo ability)
        {
            var entity = ability.entityInfo;
            if (entity == null) return;
            entity.Use(ManaRequired(entity.maxMana));
            entity.GainResource(ManaProduced(entity.maxMana));
        }
    }
}
