using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Architome
{
    public class CombatMeter : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entity;
        public CombatInfo combatInfo;
        public CombatMeterManager meterManager;
        public CombatInfo.CombatLogs combatLogs;
        public CombatInfo.CombatLogs.Values startingValues;
        public float value;
        public bool isActive;
        public bool isUpdating;
        public float secondsInCombat;

        [Serializable]
        public struct Info
        {
            public Image progress;
            public Image entityIcon;
            public TextMeshProUGUI valueText;
        }



        public Info info;


        void Start()
        {
            meterManager = GetComponentInParent<CombatMeterManager>();
        }

        // Update is called once per frame

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;

            if (entity.entityPortrait)
            {
                info.entityIcon.sprite = entity.entityPortrait;
            }

            if (entity.archClass)
            {
                info.progress.color = entity.archClass.classColor;
            }

            this.entity.OnCombatChange += OnCombatChange;

            combatInfo = entity.GetComponentInChildren<CombatInfo>();

            info.progress.fillAmount = 0;

            if (combatInfo)
            {
                combatLogs = combatInfo.combatLogs;
            }
        }

        public void ResetMeter()
        {
            value = 0;
            info.progress.fillAmount = 0;
        }

        async public void OnCombatChange(bool isInCombat)
        {
            if (!isInCombat) return;
            if (isActive) return;

            isActive = true;
            secondsInCombat = 0;
            UpdateProgressBar();

            startingValues = combatLogs.values;

            while (entity.isInCombat || (entity.PartyInfo() && entity.PartyInfo().partyIsInCombat))
            {
                await Task.Delay(250);
                secondsInCombat += .25f;


                UpdateMeter();
            }

            isActive = false;

        }

        public void UpdateMeter()
        {
            HandleDamage();
            HandleHeal();
            meterManager.UpdateMeters();
        }

        async public void UpdateProgressBar()
        {
            await Task.Delay(500);
            var progress = 1f;
            while (isActive)
            {
                await Task.Yield();
                if (meterManager.highestValue == 0)
                {
                    info.progress.fillAmount = 0;
                    continue;
                }
                if (meterManager.highestValue > 0)
                {
                    progress = value / meterManager.highestValue;
                }

                info.progress.fillAmount = Mathf.Lerp(info.progress.fillAmount, progress, .0625f);
            }
        }

        public void HandleDamage()
        {
            if (entity.role == Enums.Role.Healer) return;

            var difference = combatLogs.values.damageDone - startingValues.damageDone;
            value = difference / secondsInCombat;
            info.valueText.text = $"{Mathg.Round(difference / secondsInCombat, 1)} dps";
        }

        public void HandleHeal()
        {
            if (entity.role != Enums.Role.Healer) return;
            var difference = combatLogs.values.healingDone - startingValues.healingDone;

            value = difference / secondsInCombat;
            info.valueText.text = $"{Mathg.Round(difference / secondsInCombat, 1)} hps";
        }
    }

}