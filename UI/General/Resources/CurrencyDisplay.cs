using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Architome
{
    [RequireComponent(typeof(CurrencyDisplayFX))]
    public class CurrencyDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Components")]
        public Image currencyImage;
        public TextMeshProUGUI amount;
        public Currency currentCurrency;



        ToolTip currentLabel;
        ToolTipManager toolTipManager;

        string currencyName;
        float currentAmount;

        public Action<float, float> OnAmountChange;


        public float mostRecentChange;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            toolTipManager = ToolTipManager.active;
        }


        public void SetCurrencyDisplay(Currency currency, float amount)
        {
            currencyImage.sprite = currency.itemIcon;
            currentCurrency = currency;
            this.amount.text = ArchString.FloatToSimple(amount);
            currencyName = currency.itemName;
            currentAmount = amount;
        }

        public void UpdateCurrencyDisplay(float amount)
        {
            if (amount == currentAmount) return;

            var simple = ArchString.FloatToSimple(amount);

            mostRecentChange = amount - currentAmount;

            OnAmountChange?.Invoke(currentAmount, amount);
            currentAmount = amount;
            this.amount.text = simple;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (currentLabel != null) return;
            if (toolTipManager == null) return;
            currentLabel = toolTipManager.Label();
            currentLabel.followMouse = true;
            currentLabel.SetToolTip(new() { name = currencyName });
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (currentLabel == null) return;

            currentLabel.DestroySelf();
            currentLabel = null;
        }
    }
}
