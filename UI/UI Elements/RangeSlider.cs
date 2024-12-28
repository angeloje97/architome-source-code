using DungeonArchitect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Architome
{
    public class RangeSlider : MonoActor
    {
        [SerializeField] Slider minSlider, maxSlider;
        [SerializeField] Image fillImage, fillImageParent;
        [SerializeField] TextMeshProUGUI minText, maxText, label;

        [Header("Configuration")]
        [SerializeField] bool wholeNumbers;
        [SerializeField] FloatRange restrictions;
        [SerializeField] public FloatRange currentValue;

        public Action<FloatRange> onValueChange;
        public UnityEvent<FloatRange> onValueChangeEvent;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private async void OnValidate()
        {
            if (wholeNumbers)
            {
                await Task.Delay(250);
            }

            HandleRestrictions();
            UpdateFill();
            UpdateText();
        }

        public void SetLabel(string newLabel)
        {
            label.text = newLabel;
        }

        void HandleRestrictions()
        {

            minSlider.wholeNumbers = wholeNumbers;
            maxSlider.wholeNumbers = wholeNumbers;

            minSlider.minValue = restrictions.min;
            minSlider.maxValue = restrictions.max;
            maxSlider.maxValue = restrictions.max;
            maxSlider.maxValue = restrictions.max;

            if (wholeNumbers)
            {
                currentValue.min = (int)currentValue.min;
                currentValue.max = (int)currentValue.max;
            }

            restrictions.ClampValues();

            currentValue.ClampValues(restrictions);

            UpdateHandles();
        }

        void UpdateHandles(bool withNotify = true)
        {
            if (withNotify)
            {
                minSlider.value = currentValue.min;
                maxSlider.value = currentValue.max;
            }
            else
            {
                minSlider.SetValueWithoutNotify(currentValue.min);
                maxSlider.SetValueWithoutNotify(currentValue.max);
            }
        }

        void UpdateText()
        {
            if (wholeNumbers)
            {
                minText.text = $"{ArchString.FloatToSimple(currentValue.min, 0)}";
                maxText.text = $"{ArchString.FloatToSimple(currentValue.max, 0)}";
                return;
            }
            minText.text = $"{currentValue.min}";
            maxText.text = $"{currentValue.max}";
        }


        void UpdateFill()
        {
            var start = Mathf.InverseLerp(restrictions.min, restrictions.max, currentValue.min);
            var end = Mathf.InverseLerp(restrictions.min, restrictions.max, currentValue.max);

            ArchUI.SetLeftRightMarginsPercent(fillImage.rectTransform, fillImageParent.rectTransform, start, 1-end);
        }

        public void HandleMinSliderChange(float newValue)
        {
            currentValue.min = newValue;
            UpdateValues();
        }

        public void HandleMaxSliderChange(float newValue)
        {

            currentValue.max = newValue;
            UpdateValues();
        }

        public void UpdateValues()
        {
            currentValue.ClampValues(restrictions);
            UpdateText();
            UpdateFill();
            UpdateHandles(false);
            onValueChange?.Invoke(currentValue);
            onValueChangeEvent?.Invoke(currentValue);
        }

        public void SetWholeNumbers(bool wholeNumbers)
        {
            this.wholeNumbers = wholeNumbers;
        }

        public void SetRangeRestrictions(FloatRange newRestrictions)
        {
            restrictions = newRestrictions;
            HandleRestrictions();
        }
    }
}
