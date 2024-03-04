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

        public override void GetDependencies()
        {
            entityInfo.OnDamagePreventedFromShields += OnDamagePreventedFromShields;
            entityInfo.OnDamageTaken += OnDamageTaken;
            entityInfo.OnDamageDone += OnDamageDone;
            entityInfo.combatEvents.AddListenerHealth(eHealthEvent.OnHealingDone, OnHealingDone, this);

            entityInfo.OnExperienceGainOutside += OnGainExperienceOutside;
        }

        #region Experience Handler Functions
        public void GainExp(float value)
        {
            if (!entityInfo.canLevel) { return; }
            if(value < 0) { return; }

            entityInfo.entityStats.UpdateExperienceRequiredToLevel();

            if (entityInfo.entityStats.experience + value > entityInfo.entityStats.experienceReq)
            {
                var pastExperienceReq = entityInfo.entityStats.experienceReq;
                var currentExperience = entityInfo.entityStats.experience;
                
                value = currentExperience + value - pastExperienceReq;
                var leftOver = pastExperienceReq - currentExperience;

                entityInfo.OnExperienceGain?.Invoke(leftOver);
                LevelUp();
                GainExp(value);

                return;
            }
            else if (entityInfo.entityStats.experience + value == entityInfo.entityStats.experienceReq)
            {
                LevelUp();
            }
            else
            {
                entityInfo.entityStats.experience += value;
            }

            entityInfo.OnExperienceGain?.Invoke(value);
        }
        public void LevelUp()
        {
            entityInfo.entityStats.Level++;
            entityInfo.entityStats.experience = 0;
            entityInfo.entityStats.UpdateExperienceRequiredToLevel();

            entityInfo.OnLevelUp?.Invoke(entityInfo.entityStats.Level);
            entityInfo.entityStats.UpdateCoreStats();
            entityInfo.UpdateCurrentStats();

            if (entityInfo.isAlive)
            {
                entityInfo.RestoreFull();
            }
        }

        #endregion

        #region Listeners
        public void OnDamagePreventedFromShields(CombatEventData eventData)
        {
            GainExp(eventData.value * .25f);
        }

        public void OnGainExperienceOutside(object sender, float amount)
        {
            GainExp(amount);
        }


        public void OnDamageTaken(CombatEventData eventData)
        {
            GainExp(eventData.value * .0625f);
        }

        public void OnDamageDone(CombatEventData eventData)
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
