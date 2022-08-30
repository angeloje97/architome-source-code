using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Architome
{
    public class NewVaultSlotButton : MonoBehaviour
    {
        [SerializeField] GuildVault vault;
        [SerializeField] GuildManager manager;
        [SerializeField] ArchButton buyButton;
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] Image currencyIcon;
        [SerializeField] Currency currencyUsed;


        [Header("Test Cases")]
        [SerializeField] bool testCurrency;
        [SerializeField] int amount;

        void Start()
        {
            ArchAction.Delay(() => {
                GetDependencies();
                UpdateBuyButton();
            }, .25f);
            
        }
        void Update()
        {
        
        }

        private void OnValidate()
        {
            if (!testCurrency) return;
            testCurrency = false;

            currencyIcon.sprite = currencyUsed.itemIcon;
            amountText.text = $"{ArchString.FloatToSimple(amount, 0)}";
        }
        void GetDependencies()
        {
            manager.OnCurrenciesChange += (List<ItemData> itemDatas) => {
                UpdateBuyButton();
            };
        }

        // Update is called once per frame

        public void BuySlot()
        {
            var slotPrice = vault.SlotPrice();

            if (!manager.SpendCurrency(currencyUsed, slotPrice)) return;

            vault.AddSlot();

        }

        public void UpdateBuyButton()
        {
            var price = vault.SlotPrice();

            amountText.text = $"{ArchString.FloatToSimple(price, 0)}";

            currencyIcon.sprite = currencyUsed.itemIcon;

            buyButton.SetButton(manager.HasCurrency(currencyUsed, price));
        }
    }
}
