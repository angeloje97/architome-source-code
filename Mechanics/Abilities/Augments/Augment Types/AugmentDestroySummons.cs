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

        void OnCatalystTrigger(CatalystInfo catalyst, Collider other, bool enter)
        {

            var info = other.GetComponent<EntityInfo>();
            if (info == null) return;
            var isSummon = IsSummon(info);
            if (!isSummon) return;

            SetCatalyst(catalyst, true);
            var hit = catalyst.GetComponent<CatalystHit>();

            var combatData = new CombatEventData(catalyst, augment.entity, info.maxHealth);
            info.Damage(combatData);
            catalyst.ReduceTicks();
            catalyst.OnDamage?.Invoke(catalyst, info);

            hit.AddEnemyHit(info);
            info.Die();

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
