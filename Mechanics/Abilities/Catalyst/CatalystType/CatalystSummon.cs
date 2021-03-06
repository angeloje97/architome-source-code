using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CatalystSummon : CatalystProp
    {
        // Start is called before the first frame update
        public Augment.SummoningProperty summoning;
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
            if (!catalyst.entityInfo.isAlive) return;
            SummonEntity();

        }

        void SummonEntity()
        {
            if (summoning.summonableEntities.Count == 0) return;


            var entity = summoning.summonableEntities[Random.Range(0, summoning.summonableEntities.Count)];

            var info = entity.GetComponent<EntityInfo>();

            var original = info.npcType;

            var summoned = world.SpawnEntity(entity, transform.position).GetComponent<EntityInfo>();



            summoned.SetSummoned(new()
            {
                master = this.entity,
                liveTime = summoning.liveTime,
            });


            summoned.summon.sourceAbility = ability;
            //summoned.transform.SetParent(entityGenerator.summons, true);
            //summoned.ChangeNPCType(this.entity.npcType);
            //summoned.summon.isSummoned = true;
            //summoned.summon.master = this.entity;
            //summoned.summon.timeRemaining = summoning.liveTime;

            this.entity.combatEvents.OnSummonEntity?.Invoke(summoned);

            UpdateStats(summoned);
        }

        void UpdateStats(EntityInfo entity)
        {
            var stats = entity.entityStats;
            var value = (int) (catalyst.value * summoning.valueContributionToStats);
            stats.Level = catalyst.entityInfo.stats.Level;
            stats.Vitality = value;
            stats.Strength = value;
            stats.Wisdom = value;
            stats.Dexterity = value;

            entity.entityStats += summoning.additiveStats;

            entity.UpdateCurrentStats();
        }

    }

}
