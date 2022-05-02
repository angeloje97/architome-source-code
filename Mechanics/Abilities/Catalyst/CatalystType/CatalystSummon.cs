using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystSummon : CatalystProp
    {
        // Start is called before the first frame update
        public AbilityInfo.SummoningProperty summoning;
        WorldActions world;
        MapEntityGenerator entityGenerator;

        new void GetDependencies()
        {
            base.GetDependencies();

            world = WorldActions.active;
            entityGenerator = MapEntityGenerator.active;
            if (ability)
            {
                summoning = ability.summoning;
            }

            if (catalyst)
            {
                catalyst.OnCatalystDestroy += OnCatalystDestroy;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnCatalystDestroy(CatalystDeathCondition deathCondition)
        {

            SummonEntity();

        }

        void SummonEntity()
        {
            if (summoning.summonableEntities.Count == 0) return;


            var entity = summoning.summonableEntities[Random.Range(0, summoning.summonableEntities.Count)];

            var info = entity.GetComponent<EntityInfo>();

            var original = info.npcType;

            var summoned = world.SpawnEntity(entity, transform.position).GetComponent<EntityInfo>();


            summoned.transform.SetParent(entityGenerator.summons, true);
            summoned.ChangeNPCType(this.entity.npcType);
            summoned.summon.isSummoned = true;
            summoned.summon.master = this.entity;
            summoned.summon.timeRemaining = summoning.liveTime;

            UpdateStats(summoned.entityStats);
        }

        void UpdateStats(Stats stats)
        {
            var value = (int) (catalyst.value * summoning.valueContributionToStats);
            stats.Level = entity.stats.Level;
            stats.Vitality = value;
            stats.Strength = value;
            stats.Wisdom = value;
            stats.Dexterity = value;
        }

    }

}
