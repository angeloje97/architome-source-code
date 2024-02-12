using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentDestroySummons : AugmentType
    {
        protected override void GetDependencies()
        {
            EnableCatalyst();
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnEntityTrigger += OnEntityTrigger;
            SetCatalyst(catalyst, true);
        }

        protected override string Description()
        {
            return $"If this ability hits a unit that the caster summoned, the unit will instantly die.";
        }

        public void HandleAffectedEntity(EntityInfo entity, CatalystInfo catalyst)
        {
            if (!entity.isAlive) return;
            KillSummoned(entity, catalyst);
        }

        void OnEntityTrigger(CatalystInfo catalyst, EntityInfo info)
        {

            if (info == null) return;
            KillSummoned(info, catalyst);
        }

        void KillSummoned(EntityInfo entity, CatalystInfo catalyst)
        {
            var summoned = IsSummon(entity);
            if (!summoned) return;
            SetCatalyst(catalyst, true);
            var hit = catalyst.GetComponent<CatalystHit>();

            var combatData = new CombatEventData(catalyst, augment.entity, entity.maxHealth);
            entity.Damage(combatData);
            catalyst.ReduceTicks();
            catalyst.OnDamage?.Invoke(catalyst, entity);

            hit.AddEnemyHit(entity);

            augment.TriggerAugment(new(this));
        }

        public bool IsSummon(EntityInfo target)
        {
            if (target == null) return false;
            if (!target.summon.isSummoned) return false;
            if (target.summon.master != augment.entity) return false;

            return true;

        }
    }
}
