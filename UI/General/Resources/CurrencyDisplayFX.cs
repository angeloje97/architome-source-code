using DungeonArchitect.Builders.Grid.SpatialConstraints;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CurrencyDisplayFX : MonoBehaviour
    {
        CurrencyDisplay currencyDisplay;
        AudioManager audioManager;
        PopupTextManager popupManager;
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            currencyDisplay = GetComponent<CurrencyDisplay>();
            if (!currencyDisplay) return;

            audioManager = GetComponentInParent<AudioManager>();
            popupManager = PopupTextManager.active;
            currencyDisplay.OnAmountChange += OnAmountChange;
        }

        public void OnAmountChange(float before, float after)
        {
            var difference = after - before;

            var text = difference < 0 ? "-" : "+";

            text += $" {ArchString.FloatToSimple(Mathf.Abs(difference))}";

            var popUp = popupManager.GeneralPopUp(new(transform, text) {color =  Color.white, screenPosition = true, boolean = PopupText.eAnimatorBool.CurrencyTop, });
        }


    }
}
