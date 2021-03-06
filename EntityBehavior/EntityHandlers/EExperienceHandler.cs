using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Architome
{
    public class EExperienceHandler : EntityProp
    {
        // Start is called before the first frame update
        

        public new void GetDependencies()
        {
            base.GetDependencies();

            entityInfo.OnDamagePreventedFromShields += OnDamagePreventedFromShields;
            entityInfo.OnDamageTaken += OnDamageTaken;
            entityInfo.OnDamageDone += OnDamageDone;
            entityInfo.OnHealingDone += OnHealingDone;

            entityInfo.OnExperienceGainOutside += OnGainExperienceOutside;
        }



        public void Start()
        {
            GetDependencies();
        }

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
            GainExp(eventData.value * .125f);
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

        public void OnHealingDone(CombatEventData eventData)
        {
            GainExp(eventData.value * .25f);
        }


    }

}
