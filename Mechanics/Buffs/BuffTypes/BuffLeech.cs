using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class BuffLeech : BuffType
    {
        public enum LeechSource
        {
            Source,
            Host,
        }

        [Header("Leeching Properties")]
        public float leechPercentage;
        public LeechSource leechSource;

        private void Start()
        {
            GetDependencies();
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            var info = leechSource == LeechSource.Host ? buffInfo.hostInfo : buffInfo.sourceInfo;

            info.OnDamageDone += OnDamageDone;

            buffInfo.OnBuffEnd += (BuffInfo buff) => {
                info.OnDamageDone -= OnDamageDone;
            };
        }

        void OnDamageDone(CombatEventData eventData)
        {
            var leechAmount = eventData.value * leechPercentage;

            buffInfo.HandleTargetHealth(buffInfo.hostInfo, leechAmount, BuffTargetType.Assist);
        }

        public override string GeneralDescription()
        {
            var result = "";

            result += $"Damage done by {LeechSourceString()} will be converted into {leechPercentage * 100}% healing";

            return result;
        }

        string LeechSourceString()
        {
            if (buffInfo)
            {
                if (leechSource == LeechSource.Source && buffInfo.sourceInfo)
                {
                    return buffInfo.sourceInfo.name;
                }
                else if (leechSource == LeechSource.Host && buffInfo.hostInfo)
                {
                    return buffInfo.hostInfo.name;
                }
            }

            return $"The {leechSource} of the buff";
        }
    }
}
