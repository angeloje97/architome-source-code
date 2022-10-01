using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class AugmentSummon : AugmentType
    {
        [Header("Summon Properties")]
        public EntityGroup group;

       


        [Serializable]
        public class EntityGroup
        {
            public List<SummonableEntity> entities;
            public bool random;
            public int minAmount;
            public int maxAmount;
        }

        [Serializable]
        public class SummonableEntity
        {
            public GameObject entity;
            public Stats additiveStats;
            [SerializeField] bool zeroOut;

            public void Update()
            {
                if (!zeroOut) return;
                zeroOut = false;
                additiveStats.ZeroOut();
            }
        }

        public float radius, liveTime;


        [Header("Death Settings")]
        public bool masterDeath, masterCombatFalse;


        [SerializeField] bool update;
        LayerMask obstructionLayer;
        WorldActions world;

        void Start()
        {
            GetDependencies();
        }

        private void OnValidate()
        {
            if (!update) return;
            update = false;
            foreach (var summonableEntity in group.entities)
            {
                summonableEntity.Update();
            }
        }


        public override string Description()
        {
            var result = "";

            if (group.entities.Count > 0)
            {
                result += "Summons ";

                var entityNames = new List<string>();

                foreach (var summonableEntity in group.entities)
                {
                    var info = summonableEntity.entity.GetComponent<EntityInfo>();

                    entityNames.Add(info.entityName);
                }

                if (group.minAmount == group.maxAmount)
                {
                    if (group.minAmount > 1)
                    {
                        result += $"{group.minAmount} ";
                    }
                }
                else
                {
                    result += $"{group.minAmount} - {group.maxAmount}";
                }

                result += $"{ArchString.StringList(entityNames)} for {liveTime} seconds.";
            }

            return result;
        }

        new async void GetDependencies()
        {
            await base.GetDependencies();

            world = WorldActions.active;

            var layerMasksData = LayerMasksData.active;

            if (layerMasksData)
            {
                obstructionLayer = layerMasksData.structureLayerMask;
            }

            EnableCatalyst();
        }

        public override void HandleNewCatlyst(CatalystInfo catalyst)
        {
            catalyst.OnCatalystDestroy += (CatalystDeathCondition deathCondition) =>
            {
                SummonEntities(catalyst.transform.position);
                augment.TriggerAugment(new(this));
            };
        }

        public void SummonEntities(Vector3 position)
        {
            
            var amount = UnityEngine.Random.Range(group.minAmount, group.maxAmount);

            Debugger.Combat(4912, $"{amount} positions");
            Debugger.Combat(4913, $"Center: {position}");
            var randomOffset = UnityEngine.Random.Range(0f, 360f);
            var positions = V3Helper.PointsAroundPosition(position, amount, radius, obstructionLayer, randomOffset);

            

            for (int i = 0; i < positions.Count; i++)
            {
                var entity = group.entities[0];
                if (i < group.entities.Count)
                {
                    entity = group.entities[i];
                }

                if (group.random)
                {
                    entity = ArchGeneric.RandomItem(group.entities);
                }

                Debugger.Combat(4914, $"Spot {i}: {positions[i]}");
                SummonEntity(entity, positions[i]);
            }
        }

        public void SummonEntity(SummonableEntity summonable, Vector3 position)
        {
            var summoned = world.SpawnEntity(summonable.entity.gameObject, position).GetComponent<EntityInfo>();

            summoned.SetSummoned(new()
            {
                master = augment.entity,
                liveTime = liveTime,
                masterDeath = masterDeath,
                masterCombatFalse = masterCombatFalse
            });

            UpdateStats(summoned, summonable.additiveStats);

        }

        public void UpdateStats(EntityInfo entity, Stats additiveStats)
        {
            var stats = entity.entityStats;
            var value = (int) this.value;
            stats.Level = augment.entity.stats.Level;
            stats.Vitality = value;
            stats.Strength = value;
            stats.Wisdom = value;
            stats.Dexterity = value;

            entity.entityStats += additiveStats;

            entity.UpdateCurrentStats();
        }
    }
}
