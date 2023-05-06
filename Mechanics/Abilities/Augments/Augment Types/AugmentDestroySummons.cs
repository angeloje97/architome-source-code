using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentDestroySummons : AugmentType
    {
        // Start is called before the first frame update
        void Start()
        {
            GetDependencies();
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            EnableCatalyst();
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnCatalystTrigger += OnCatalystTrigger;
            SetCatalyst(catalyst, true);
        }

        public override string Description()
        {
            return $"If this ability hits a unit that the caster summoned, the unit will instantly die.";
        }

        public void HandleAffectedEntity(EntityInfo entity, CatalystInfo catalyst)
        {
            if (!entity.isAlive) return;
            KillSummoned(entity, catalyst);
        }

        void OnCatalystTrigger(CatalystInfo catalyst, Collider other, bool enter)
        {

            var info = other.GetComponent<EntityInfo>();
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
