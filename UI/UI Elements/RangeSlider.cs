using DungeonArchitect;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class RangeSlider : MonoActor
    {
        [SerializeField] Slider minSlider, maxSlider;
        [SerializeField] Image fillImage;
        [SerializeField] TextMeshProUGUI minText, maxText;

        [SerializeField] IntRange intRestrictions;
        [SerializeField] IntRange currentIntValue;
        

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
            minSlider.minValue = intRestrictions.min;
            minSlider.maxValue = intRestrictions.max;
            maxSlider.maxValue = intRestrictions.max;
            maxSlider.maxValue = intRestrictions.max;

            //currentIntValue.min = (int) minSlider.value;
            //currentIntValue.max = (int) maxSlider.value;

            currentIntValue.ClampValues(intRestrictions);

            minSlider.value = (float) currentIntValue.min;
            maxSlider.value = (float) currentIntValue.max;

        }

        void UpdateText()
        {

        }

        void UpdateFill()
        {

        }
    }
}
