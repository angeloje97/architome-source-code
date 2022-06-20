using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffStatChange : BuffType
    {
        // Start is called before the first frame update
        public Stats stats;
        public bool buffValueMultiplies;
        public float multiplier;
        public bool stacks;
        public bool zeroOut;
        [SerializeField]Stats starting;

        private void OnValidate()
        {
            if (!zeroOut) return;
            zeroOut = false;
            stats.ZeroOut();
            
        }
        new void GetDependencies()
        {
            base.GetDependencies();

            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;

                if (buffValueMultiplies)
                {
                    stats *= buffInfo.properties.value * multiplier;
                }

                if (stacks)
                {

                    buffInfo.OnStack += OnStack;
                }

                ApplyBuff();
            }
            else
            {
                Destroy(gameObject);
            }

        }
        void Start()
        {
            GetDependencies();
        }

        public override string Description()
        {

            var result = "";

            var attributes = stats.Attributes();

            if (attributes.Count == 0) return result;

            result += $"Stats provided from buff :\n";

            var percentageFields = Stats.PercentageFields;

            foreach (var attribute in attributes)
            {
                var sign = attribute.Positive ? "+" : "-";
                if (percentageFields.Contains(attribute.Name))
                {
                    result += $"{ArchString.CamelToTitle(attribute.Name)}: {sign}({(float) attribute.Data * 100}%)";
                }
                else
                {
                    result += $"{ArchString.CamelToTitle(attribute.Name)}: {sign}({attribute.Data})";
                }


                result += "\n";
            }

            return result;
        }

        public override string GeneralDescription()
        {
            return Description();
        }

        void OnStack(BuffInfo info, int stacks, float value)
        {
            stats = starting*stacks;
            buffInfo.buffsManager.UpdateStats();
        }

        public void ApplyBuff()
        {
            starting = new();
            starting.Copy(stats);
            buffInfo.buffsManager.UpdateStats();
        }

        public void OnBuffEnd(BuffInfo buff)
        {
            stats.ZeroOut();
            buffInfo.buffsManager.UpdateStats();
        }
    }

}
