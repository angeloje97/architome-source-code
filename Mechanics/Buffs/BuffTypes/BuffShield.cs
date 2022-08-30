using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffShield : BuffType
    {
        // Start is called before the first frame update

        public float shieldAmount { get { return value; } private set { this.value = value; } }
        public bool applied;
        new void GetDependencies()
        {
            base.GetDependencies();
            if (buffInfo)
            {
                buffInfo.OnBuffEnd += OnBuffEnd;

                shieldAmount = value;

                ApplyBuff();
            }
        }


        public override string Description()
        {
            string result = "";

            result += $"Absorbs the next {ArchString.FloatToSimple(shieldAmount)} damage.";

            return result;
        }

        public override string GeneralDescription()
        {
            return $"Prevents any type of damage from being taken.";
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

        public virtual float DamageShield(CombatEventData eventData)
        {
            var value = eventData.value;
            
            var nextValue = shieldAmount > value ? 0 : value - shieldAmount;

            if (value > shieldAmount)
            {
                value = shieldAmount;
            }

            shieldAmount -= value;

            if (buffInfo.hostInfo != buffInfo.sourceInfo)
            {
                buffInfo.sourceInfo.OnDamagePreventedFromShields?.Invoke(new CombatEventData(buffInfo, value) { target = buffInfo.hostInfo});
            }

            if (shieldAmount == 0)
            {
                buffInfo.Deplete();
            }

            buffInfo.hostInfo.UpdateShield();

            eventData.value = nextValue;

            return nextValue;
        }
    }

}