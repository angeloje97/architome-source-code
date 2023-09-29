using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using System;
using TMPro;
using Architome.Enums;


namespace Architome
{
    public class CombatMeterManager : MonoBehaviour
    {
        // Start is called before the first frame update
        [Serializable]
        public struct Info
        {
            public TMP_Dropdown meterModeDropDown;
            public TMP_Dropdown recordingModeDropDown;
        }

        public Info info;

        public GameObject combatMeterPrefab;
        public WidgetInfo widget;

        public List<CombatMeter> combatMeters;

        float highestValue;
        public bool isUpdating;

        public List<string> meterMode;

        public FieldInfo currentField;
        public MeterRecordingMode recordingMode;

        public Action<float> OnMaxValue;
        public Action<FieldInfo> OnChangeField;
        public Action<MeterRecordingMode> OnChangeMode;

        [SerializeField] bool updateSelf;


        void GetDependencies()
        {
            GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
            GameManager.active.OnNewPlayableParty += OnNewPlayableParty;
            currentField = typeof(CombatInfo.CombatLogs.Values).GetFields()[0];

            widget = GetComponentInParent<WidgetInfo>();

            widget.OnActiveChange += OnActiveChange;
        }

        void Start()
        {
            GetDependencies();

        }

        private void OnValidate()
        {
            if (!updateSelf) return;
            updateSelf = false;
            meterMode = typeof(CombatInfo.CombatLogs.Values).GetFields().Select(field => ArchString.CamelToTitle(field.Name)).ToList();

            if (info.meterModeDropDown)
            {
                info.meterModeDropDown.ClearOptions();

                foreach (var mode in meterMode)
                {
                    info.meterModeDropDown.options.Add(new TMP_Dropdown.OptionData() { text = mode });
                }


                info.meterModeDropDown.enabled = false;
                info.meterModeDropDown.enabled = true;
            }

            if (info.recordingModeDropDown)
            {
                //info.recordingModeDropDown.ClearOptions();

                //foreach (MeterRecordingMode mode in Enum.GetValues(typeof(MeterRecordingMode)))
                //{
                //    var newText = ArchString.CamelToTitle(mode.ToString());
                //    info.recordingModeDropDown.options.Add(new() { text = newText });
                //}

                ArchUI.SetDropDown<MeterRecordingMode>(info.recordingModeDropDown);

                info.recordingModeDropDown.enabled = false;
                info.recordingModeDropDown.enabled = true;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void ChangeField(TMP_Dropdown meterMode)
        {
            var value = meterMode.value;
            if (typeof(CombatInfo.CombatLogs.Values).GetFields().Length <= value) return;
            currentField = typeof(CombatInfo.CombatLogs.Values).GetFields()[value];

            OnChangeField?.Invoke(currentField);

            SortMeters();
        }

        public void ChangeMode(TMP_Dropdown recordingMode)
        {
            var index = recordingMode.value;

            var enums = Enum.GetValues(typeof(MeterRecordingMode));

            if (enums.Length <= index) return;

            this.recordingMode = (MeterRecordingMode) enums.GetValue(index);

            OnChangeMode?.Invoke(this.recordingMode);


            SortMeters();
        }

        void OnActiveChange(bool active)
        {
            ArchAction.Yield(() => SortMeters());
            
        }

        public void OnNewPlayableEntity(EntityInfo entity, int index)
        {
            if (combatMeterPrefab == null) return;
            if (entity == null) return;

            var combatMeter = Instantiate(combatMeterPrefab, transform).GetComponent<CombatMeter>();

            if (combatMeter != null)
            {
                combatMeters.Add(combatMeter);
                combatMeter.SetEntity(entity);
            }

            SortMeters();
        }

        public void OnNewPlayableParty(PartyInfo party, int index)
        {
            if (index != 0) return;
            party.events.OnCombatChange += OnCombatChange;
        }

        async public void UpdateMeters(bool ignoreActiveMeters = false)
        {
            if (isUpdating)
            {
                return;
            }

            isUpdating = true;
            
            SortMeters(ignoreActiveMeters);

            await Task.Delay(250);

            isUpdating = false;
        }

        void SortMeters(bool ignoreActiveMembers = false)
        {
            CalculateMax();
            if (ignoreActiveMembers)
            {
                combatMeters = combatMeters.OrderBy(meter => meter.value).ToList();
            }
            else
            {
                combatMeters = combatMeters.OrderBy(meter => meter.value).ThenBy(meter => meter.isActive).ToList();
            }

            foreach (var meter in combatMeters)
            {
                meter.transform.SetAsFirstSibling();
            }
        }

        public void OnCombatChange(bool isInCombat)
        {
            if (!isInCombat)
            {
                ArchAction.Delay(() => UpdateMeters(true), .5f);
                return;
            }
            foreach (var meter in combatMeters)
            {
                if (meter.entity.isInCombat) continue;
                meter.ResetMeter();
            }
        }

        public void CalculateMax()
        {
            highestValue = combatMeters.Max(meter => meter.value);
            OnMaxValue?.Invoke(highestValue);
        }

    }

}