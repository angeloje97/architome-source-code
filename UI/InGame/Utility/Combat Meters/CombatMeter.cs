using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Reflection;
using Architome.Enums;

namespace Architome
{
    public class CombatMeter : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entity;
        public CombatInfo combatInfo;
        public CombatMeterManager meterManager;
        public WidgetInfo widget;

        public CombatInfo.CombatLogs combatLogs;
        public CombatInfo.CombatLogs.Values startingValues;
        public CombatInfo.CombatLogs.Values inCombatValues;

        public float value;
        public float percentage;
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
            widget = GetComponentInParent<WidgetInfo>();

            widget.OnActiveChange += OnActiveChange;
            meterManager.OnChangeMode += OnChangeMode;
            meterManager.OnChangeField += OnChangeField;
            meterManager.OnMaxValue += OnMaxValue;
        }

        private void OnMaxValue(float maxValue)
        {
            percentage = maxValue > 0 ? value / maxValue : 0;
            UpdateProgressBar();
        }

        private void OnChangeField(FieldInfo obj)
        {
            UpdateMeter();
        }

        private void OnChangeMode(MeterRecordingMode obj)
        {
            UpdateMeter();
        }

        private void OnActiveChange(bool obj)
        {
            UpdateMeter();
            ArchAction.Yield(() => UpdateMeter());
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
            if (meterManager.recordingMode == MeterRecordingMode.Dungeon) return;
            value = 0;
            info.progress.fillAmount = 0;
        }

        async public void UpdateDungeonStats()
        {
            while (true)
            {
                await Task.Delay(3000);

                if (meterManager.recordingMode == MeterRecordingMode.CurrentFight)
                {
                    await Task.Delay(5000);
                    continue;
                }

                HandleValueDungeon();
                meterManager.UpdateMeters();
            }
        }

        async public void OnCombatChange(bool isInCombat)
        {
            if (!isInCombat) return;
            if (isActive) return;

            isActive = true;
            secondsInCombat = 0;

            startingValues = combatLogs.values;

            while (entity.isInCombat || (entity.PartyInfo() && entity.PartyInfo().partyIsInCombat))
            {
                await Task.Delay(250);
                secondsInCombat += .25f;

                if (!widget.isActive) continue;

                UpdateMeter();
            }

            UpdateCombatValues();

            isActive = false;

        }

        private void UpdateCombatValues()
        {
            foreach (var field in inCombatValues.GetType().GetFields())
            {
                var combatValue = (float)field.GetValue(inCombatValues);
                var startingValue = (float)field.GetValue(startingValues);
                var totalValue = (float)field.GetValue(combatLogs.values);

                field.SetValue(inCombatValues, combatValue + (totalValue - startingValue));

            }
        }

        public void UpdateMeter()
        {
            HandleValueCombat();
            HandleValueDungeon();
            //HandleHeal();
            meterManager.UpdateMeters();
        }

        async public void UpdateProgressBar()
        {
            if (isUpdating) return;

            isUpdating = true;
            await Task.Delay(500);
            while (info.progress.fillAmount != percentage)
            {
                await Task.Yield();

                info.progress.fillAmount = Mathf.Lerp(info.progress.fillAmount, percentage, .0625f);
            }

            isUpdating = false;
        }

        public void HandleValueCombat()
        {
            if (meterManager.recordingMode != MeterRecordingMode.CurrentFight) return;

            if (secondsInCombat == 0) return;
            var difference = (float)(meterManager.currentField.GetValue(combatLogs.values)) - (float)(meterManager.currentField.GetValue(startingValues));
            
            value = difference / secondsInCombat;
            //info.valueText.text = $"{Mathg.Round(difference / secondsInCombat, 1)}/s";
            info.valueText.text = value != 0 ? $"{ArchString.FloatToSimple(difference / secondsInCombat)}/s" : "";
            
        }

        public void HandleValueDungeon()
        {
            if (meterManager.recordingMode != MeterRecordingMode.Dungeon) return;

            value = (float)meterManager.currentField.GetValue(combatLogs.values);


            info.valueText.text = value != 0 ? $"{ArchString.FloatToSimple(value)}" : "";
        }

        public void HandleHeal()
        {
        }
    }

}