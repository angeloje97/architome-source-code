using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    [Serializable]
    public struct AbilityResources
    {
        AbilityInfo ability;
        public float requiredAmount;
        public float requiredPercent;
        public float producedAmount;
        public float producedPercent;

        public bool hasMana;
        bool hasManaCheck;

        public Action<bool> OnHasManaChange;


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
            this.ability = ability;
            ability.OnReadyCheck += OnReadyCheck;
            ability.OnSuccessfulCast += OnSuccessfulCast;
            HandleManaChange();
        }

        async void HandleManaChange()
        {
            var entity = ability.entityInfo;
            while(ability && Application.isPlaying)
            {
                hasMana = entity.mana > ManaRequired(entity.maxMana);

                if(hasMana != hasManaCheck)
                {
                    hasManaCheck = hasMana;
                    OnHasManaChange?.Invoke(hasMana);
                }

                await Task.Delay(500);
            }
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
