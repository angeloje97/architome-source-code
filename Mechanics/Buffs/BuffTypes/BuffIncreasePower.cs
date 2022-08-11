using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BuffIncreasePower : BuffType
    {
        public float powerIncreasePercent;
        public float currentPowerIncrease;
        public int currentStacks;

        public bool sameHealthPercent;
        private void Start()
        {
            GetDependencies();
            IncreasePower();

            buffInfo.OnBuffEnd += OnBuffEnd;
            buffInfo.OnStack += OnBuffStack;
        }

        public override string Description()
        {
            return $"Power increased by {currentPowerIncrease * 100}%";
        }

        public override string FaceDescription(float theoreticalValue)
        {
            return GeneralDescription();
        }

        public override string GeneralDescription()
        {
            return $"Increases power by {powerIncreasePercent * 100}%";
        }

        public void IncreasePower()
        {
            currentPowerIncrease = buffInfo.stacks * powerIncreasePercent;

            var buffStacks = buffInfo.stacks;

            var healthPercent = buffInfo.hostInfo.health / buffInfo.hostInfo.maxHealth;

            while (currentStacks != buffStacks)
            {
                int difference = 0;
                if (currentStacks > buffStacks)
                {
                    currentStacks--;
                    difference = -1;
                }
                
                if (currentStacks < buffStacks)
                {
                    currentStacks++;

                    difference = 1;
                }

                buffInfo.hostInfo.entityStats.IncreasePower(difference * powerIncreasePercent);
            }


            buffInfo.hostInfo.UpdateCurrentStats();

            buffInfo.hostInfo.health = buffInfo.hostInfo.maxHealth * healthPercent;
        }

        void OnBuffStack(BuffInfo buff, int stacks, float value)
        {
            IncreasePower();
        }

        void OnBuffEnd(BuffInfo buff)
        {
            buffInfo.hostInfo.entityStats.IncreasePower(-currentPowerIncrease);
        }
    }
}
