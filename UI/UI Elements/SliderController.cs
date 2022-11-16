using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class SliderController : MonoBehaviour
    {
        public Slider slider;
        public TextMeshProUGUI value, label;

        public Action<SliderController, float> OnValueChange;
        float currentValue;

        public bool percentValue;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnValidate()
        {
            HandleSliderChange(slider);
        }

        public void HandleSliderChange(Slider slider)
        {
            value.text = $"{slider.value}";

            if (percentValue)
            {
                value.text = ArchString.FloatToSimple(slider.value * 100);
                value.text += "%";
            }

            currentValue = slider.value;

            OnValueChange?.Invoke(this, currentValue);
        }

        public string Label()
        {
            if (label)
            {
                return label.text;
            }

            return "";
        }
        public void SetValue(float newValue)
        {
            if (slider.minValue > newValue) return;
            if (slider.maxValue < newValue) return;

            slider.value = newValue;
        }

        public void SetLabel(string labelName)
        {
            if (label == null) return;
            label.text = labelName;
        }
    }
}
