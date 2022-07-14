using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffInstantHealth : BuffType
    {
        public bool heals;
        new void GetDependencies()
        {
            base.GetDependencies();

            ApplyBuff();
        }

        void ApplyBuff()
        {
            buffInfo.HandleTargetHealth(buffInfo.sourceInfo, value, heals ? BuffTargetType.Assist : BuffTargetType.Harm); 
        }

        void Start()
        {
            GetDependencies();
        }

        public override string Description()
        {
            var result = "Instantly ";

            result += heals ? "heals " : "damages ";

            result += $" {value} health.\n";

            return result;
            
        }

        public override string GeneralDescription()
        {
            var result = "Instantly ";

            result += heals ? "heals " : "damages ";

            result += "health at the beginning of the buff.\n";

            return result;
        }
    }
}
