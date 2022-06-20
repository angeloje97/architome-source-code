using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffShield : BuffType
    {
        // Start is called before the first frame update

        public float shieldAmount;
        public bool applied;
        new void GetDependencies()
        {
            base.GetDependencies();
            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;

                //if (buffInfo.hostInfo == buffInfo.sourceInfo)
                //{

                //}

                //if (buffInfo && buffInfo.hostInfo && buffInfo.sourceInfo)
                //{
                //    if (buffInfo.hostInfo == buffInfo.sourceInfo)
                //    {
                //        if (buffInfo && buffInfo.sourceAbility)
                //        {
                //            shieldAmount = buffInfo.properties.value * buffInfo.sourceAbility.selfCastMultiplier;
                //        }

                //    }
                //    else
                //    {
                //        shieldAmount = buffInfo.properties.value;
                //    }
                //}

                shieldAmount = value;

                ApplyBuff();
            }
        }


        public override string Description()
        {
            string result = "";

            result += $"Absorbs the next {ArchString.FloatToSimple(shieldAmount)} damage.\n";

            return result;
        }

        public override string GeneralDescription()
        {
            return $"Prevents any type of damage from being taken\n";
        }

        void Start()
        {
            GetDependencies();
        }
        void ApplyBuff()
        {
            if (buffInfo && buffInfo.hostInfo)
            {
                applied = true;
                buffInfo.hostInfo.UpdateShield();
            }
        }
        // Update is called once per frame
        void Update()
        {
        }

        public void OnBuffEnd(BuffInfo buff)
        {
            shieldAmount = 0;
            buffInfo.hostInfo.UpdateShield();
        }

        public float DamageShield(float value)
        {
            var nextValue = shieldAmount > value ? 0 : value - shieldAmount;

            if (value > shieldAmount)
            {
                value = shieldAmount;
            }

            shieldAmount -= value;

            if (buffInfo.hostInfo != buffInfo.sourceInfo)
            {
                buffInfo.sourceInfo.OnDamagePreventedFromShields?.Invoke(new CombatEventData(buffInfo, buffInfo.sourceInfo, value) { target = buffInfo.sourceInfo });
            }

            if (shieldAmount == 0)
            {
                buffInfo.Deplete();
            }

            buffInfo.hostInfo.UpdateShield();

            return nextValue;
        }
    }

}