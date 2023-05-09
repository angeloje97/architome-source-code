using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class CombatEventData
    {
        public CatalystInfo catalyst { get; set; }
        public BuffInfo buff { get; set; }

        public AbilityInfo ability { get; set; }
        public EntityInfo source { get; set; }
        public EntityInfo target { get; set; }
        public float value { get; set; }
        public float percentValue { get; set; }
        public bool critical { get; set; }

        public bool lethalDamage { get; set; }

        public CombatEventData(CatalystInfo catalyst, EntityInfo source, float value)
        {
            this.catalyst = catalyst;
            this.ability = catalyst.abilityInfo;
            this.source = source;
            this.value = value;
        }

        public CombatEventData(BuffInfo buff, float value)
        {
            this.buff = buff;
            this.source = buff.sourceInfo;
            this.ability = buff.sourceAbility;
            this.value = value;
        }

        public CombatEventData(EntityInfo source)
        {
            this.source = source;
        }

        public DamageType DataDamageType()
        {
            if (buff)
            {
                return buff.damageType;
            }

            if (catalyst)
            {
                return catalyst.damageType;
            }

            return DamageType.True;
        }
        public AbilityInfo SourceAbility()
        {
            if (ability) return ability;

            if (buff && buff.sourceAbility)
            {
                return buff.sourceAbility;
            }

            if (catalyst && catalyst.abilityInfo)
            {
                return catalyst.abilityInfo;
            }

            return null;
        }

    }

    public class SocialEventData
    {
        public EntityInfo source { get; set; }
        public EntityInfo target { get; set; }

        public SocialEventData(EntityInfo source, EntityInfo target)
        {
            this.source = source;
            this.target = target;
        }
    }
}

