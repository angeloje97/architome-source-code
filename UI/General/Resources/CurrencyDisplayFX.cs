using DungeonArchitect.Builders.Grid.SpatialConstraints;
using Mono.Cecil;
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

            var popUp = popupManager.GeneralPopUp(transform, text, Color.white, new()
            {
                screenPosition = true,
                currencyTop = true,
            });


        }


    }
}