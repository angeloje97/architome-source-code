using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffStatChange : BuffType
    {
        // Start is called before the first frame update
        public bool stacks;
        public bool zeroOut;
        public bool affectedByValue;
        public Stats stats;

        //Warning : If planning to use percentage type stats and floating stats, do not use affected by value = true
        //Reason : Might give an insane amount of haste or attack speed which will break the game.

        Stats starting;

        private void OnValidate()
        {
            if (!zeroOut) return;
            zeroOut = false;
            stats.ZeroOut();
            
        }
        new void GetDependencies()
        {
            base.GetDependencies();
            
            if (affectedByValue)
            {
                stats *= value;
            }

            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;

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
