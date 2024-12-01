using DungeonArchitect;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class RangeSlider : MonoActor
    {
        [SerializeField] Slider minSlider, maxSlider;
        [SerializeField] Image fillImage, fillImageParent;
        [SerializeField] TextMeshProUGUI minText, maxText;

        [Header("Configuration")]
        [SerializeField] bool wholeNumbers;
        [SerializeField] FloatRange restrictions;
        [SerializeField] FloatRange currentValue;



        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        private void OnValidate()
        {
            HandleRestrictions();
            UpdateFill();
            UpdateText();
        }

        void HandleRestrictions()
        {
            minSlider.minValue = restrictions.min;
            minSlider.maxValue = restrictions.max;
            maxSlider.maxValue = restrictions.max;
            maxSlider.maxValue = restrictions.max;

            currentValue.ClampValues(restrictions);

            minSlider.value = currentValue.min;
            maxSlider.value = currentValue.max;

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

            var width = fillImageParent.rectTransform.rect.width;

            ArchUI.SetLeftRightMargins(fillImage.rectTransform, width * start, width - (width * end));

        }

        public void HandleMinSliderChange(float newValue)
        {

        }

        public void HandleMaxSliderChange(float newValue)
        {

        }

    }
}
