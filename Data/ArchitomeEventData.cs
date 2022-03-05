using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CombatEventData
    {
        public CatalystInfo catalyst { get; set; }
        public BuffInfo buff { get; set; }
        public EntityInfo source { get; set; }
        public EntityInfo target { get; set; }
        public float value { get; set; }
        public float percentValue { get; set; }
        public bool critical { get; set; }

        public CombatEventData(CatalystInfo catalyst, EntityInfo source, float value)
        {
            this.catalyst = catalyst;
            this.source = source;
            this.value = value;
        }

        public CombatEventData(BuffInfo buff, EntityInfo source, float value)
        {
            this.buff = buff;
            this.source = source;
            this.value = value;
        }

        public CombatEventData(EntityInfo source)
        {
            this.source = source;
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

