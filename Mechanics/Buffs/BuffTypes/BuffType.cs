using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffType : MonoBehaviour
    {
        #region Common Data

        public BuffInfo buffInfo;

        public float valueContributionToBuffType = 1;
        public float value = 1;
        public float selfCastMultiplier = 1;

        public EntityInfo host => buffInfo.hostInfo;
        public EntityInfo source => buffInfo.sourceInfo;

        public CombatEvents hostCombatEvent => host.combatEvents;

        #endregion
        public void GetDependencies()
        {
            buffInfo = GetComponent<BuffInfo>();


            if (buffInfo == null)
            {
                Destroy(gameObject);
                return;
            }

            value = valueContributionToBuffType * buffInfo.properties.value;

            if (buffInfo.hostInfo == buffInfo.sourceInfo)
            {
                value = selfCastMultiplier * buffInfo.properties.value;
            }
            
        }



        public virtual string Description()
        {
            return GeneralDescription();
        }

        public virtual string GeneralDescription()
        {
            return "";
        }

        public virtual string FaceDescription(float theoreticalValue)
        {
            buffInfo = GetComponent<BuffInfo>();
            value = theoreticalValue * valueContributionToBuffType;
            var selfCastValue = theoreticalValue * selfCastMultiplier;
            var description = Description();

            if (valueContributionToBuffType != selfCastMultiplier)
            {
                description += $"({selfCastValue} value if the host is the source.)";
            }

            return description;
        }
    }

}
