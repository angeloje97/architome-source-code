using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class EExperienceHandler : EntityProp
    {
        public Stats stats => entityInfo.entityStats;
        public override void GetDependencies()
        {

            combatEvents.AddListenerHealth(eHealthEvent.OnDamagePreventedFromShields, OnDamagePreventedFromShields, this);
            combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, OnDamageTaken, this);
            combatEvents.AddListenerHealth(eHealthEvent.OnDamageDone, OnDamageDone, this);
            combatEvents.AddListenerHealth(eHealthEvent.OnHealingDone, OnHealingDone, this);

            entityInfo.OnExperienceGainOutside += OnGainExperienceOutside;
        }

        #region Experience Handler Functions
        public void GainExp(float value)
        {
            if (!entityInfo.canLevel) { return; }
            if(value < 0) { return; }

            stats.UpdateExperienceRequiredToLevel();

            if (stats.experience + value > stats.experienceReq)
            {
                var pastExperienceReq = stats.experienceReq;
                var currentExperience = stats.experience;
                
                value = currentExperience + value - pastExperienceReq;
                var leftOver = pastExperienceReq - currentExperience;

                entityInfo.OnExperienceGain?.Invoke(leftOver);
                LevelUp();
                GainExp(value);

                return;
            }
            else if (stats.experience + value == entityInfo.entityStats.experienceReq)
            {
                LevelUp();
            }
            else
            {
                stats.experience += value;
            }
        }
        public void LevelUp()
        {
            stats.Level++;
            stats.experience = 0;
            stats.UpdateExperienceRequiredToLevel();

            entityInfo.OnLevelUp?.Invoke(stats.Level);
            stats.UpdateCoreStats();
            entityInfo.UpdateCurrentStats();

            if (entityInfo.isAlive)
            {
                entityInfo.RestoreFull();
            }
        }

        #endregion

        #region Listeners
        public void OnDamagePreventedFromShields(HealthEvent eventData)
        {
            GainExp(eventData.value * .25f);
        }

        public void OnGainExperienceOutside(object sender, float amount)
        {
            GainExp(amount);
        }


        public void OnDamageTaken(HealthEvent eventData)
        {
            GainExp(eventData.value * .0625f);
        }

        public void OnDamageDone(HealthEvent eventData)
        {
            if (!IsValidtarget()) return;

            GainExp(eventData.value * .25f);

            bool IsValidtarget()
            {
                if (eventData.target.summon.isSummoned) return false;
                return true;
            }
        }

        public void OnHealingDone(HealthEvent eventData)
        {
            GainExp(eventData.value * .25f);
        }

        #endregion

    }

}
