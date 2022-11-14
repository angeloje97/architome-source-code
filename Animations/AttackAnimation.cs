using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    [Serializable]
    public class AttackAnimation
    {
        EntityAnimationEvents animations;
        public List<AbilityInfo> alternativeCasting;
        public AbilityManager manager;

        public bool active;


        public AttackAnimation(EntityInfo entity, Animator animation)
        {
            animations = animation.GetComponent<EntityAnimationEvents>();
            alternativeCasting = new();
            if (animations == null) return;
            manager = entity.AbilityManager();
            if (!manager) return;

            foreach (var ability in manager.GetComponentsInChildren<AbilityInfo>())
            {
                AddAbility(ability);
            }

            manager.OnAbilityUpdate += HandleAbilityUpdate;

            animations.events.OnHit += HandleRelease;
            animations.events.OnShoot += HandleRelease;
        }

        void AddAbility(AbilityInfo ability)
        {
            if (alternativeCasting.Contains(ability)) return;
            if (ability.catalystInfo == null) return;
            if (!ability.catalystInfo.useAlternateCasting) return;
            alternativeCasting.Add(ability);
            ability.OnAlternativeCastCheck += HandleAlternativeCastCheck;


            ability.OnRemoveAbility += HandleRemoveAbility;

        }

        void HandleRemoveAbility(AbilityInfo ability)
        {
            if (!alternativeCasting.Contains(ability)) return;
            alternativeCasting.Remove(ability);
            ability.OnRemoveAbility -= HandleRemoveAbility;
            ability.OnAlternativeCastCheck -= HandleAlternativeCastCheck;
        }

        public void HandleRelease()
        {
            if (!active) return;
            active = false;
        }

        async void HandleAlternativeCastCheck(AbilityInfo ability, List<bool> checks)
        {
            checks.Add(true);
            active = true;


            while (active)
            {
                await Task.Yield();
            }

            ability.alternateCastingActive = false;

        }


        void HandleAbilityUpdate(AbilityInfo ability)
        {
            AddAbility(ability);
        }

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
