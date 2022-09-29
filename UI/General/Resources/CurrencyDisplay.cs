using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

namespace Architome
{
    public class CurrencyDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Components")]
        public Image currencyImage;
        public TextMeshProUGUI amount;
        

        ToolTip currentLabel;
        ToolTipManager toolTipManager;

        string currencyName;
        float currentAmount;

        public Action<float, float> OnAmountChange;

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
            this.amount.text = ArchString.FloatToSimple(amount);
            currencyName = currency.itemName;
        }

        public void UpdateCurrencyDisplay(float amount)
        {
            if (amount == currentAmount) return;

            var simple = ArchString.FloatToSimple(amount);

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
