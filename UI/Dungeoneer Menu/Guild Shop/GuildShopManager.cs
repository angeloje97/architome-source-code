using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class GuildShopManager : MonoBehaviour
    {
        #region Fields
        public GuildVault guildVault;
        public GuildManager guildManager;
        public List<Shop> targetShops;
        public WorldActions worldActions;
        public PromptHandler promptHandler;

        ContextMenu menu;
        #endregion
        void Start()
        {
            GetDependencies();
            HandleShops();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        void GetDependencies()
        {
            menu = ContextMenu.current;
            targetShops = GetComponentsInChildren<Shop>().ToList();
            guildManager = GuildManager.active;
            guildVault = GuildVault.active;
            worldActions = WorldActions.active;
            promptHandler = PromptHandler.active;
        }

        #region Handling Items
        void HandleShops()
        {
            foreach(var shop in targetShops)
            {
                shop.OnMerchAction += HandleMerchAction;
            }
        }

        async void HandleMerchAction(MerchData merchandise)
        {
            Debugger.UI(6951, $"Used item action");

            var options = new List<ContextMenu.OptionData>()
            {
                new("Buy", () => { HandleBuy(merchandise); })
            };

            if(merchandise.item.MaxStacks > 1)
            {
                options.Add(new("Buy X Amount", () => { HandleBuyAmount(merchandise); }));
                options.Add(new("Buy Max", () => { HandleBuyMax(merchandise); }));
            }

            

            await menu.UserChoice(new()
            {
                title = $"{merchandise.item}",
                options = options,
            });
        }

        void HandleBuy(MerchData data)
        {
            SendToVault(data, data.amount);
        }

        void HandleBuyAmount(MerchData data)
        {
            var amount = 1;
            var spriteAssetSet = false;

            var response = promptHandler.InputPrompt(new() {
                title = $"{data.item}",
                icon = data.item.itemIcon,
                blocksScreen = true,
                OnStart = OnStart,
                OnInputFieldChange = OnInputFieldChange,
                options = new()
                {
                    new("Cancel") { isEscape = true },
                    new("Buy"),
                }
            });

            SendToVault(data, amount);

            void OnInputFieldChange(PromptInfo prompt, TMP_InputField inputField) 
            {
                amount = int.Parse(inputField.text);

                amount = Mathf.Clamp(amount, 1, data.item.MaxStacks);
                inputField.text = $"{amount}";

                Debugger.UI(7414, $"Entered amount {amount}");
                UpdateText(prompt.info.question);
            }

            void OnStart(PromptInfo prompt)
            {
                UpdateText(prompt.info.question);
                prompt.input.inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                prompt.input.inputField.text = $"{amount}";
            }

            void UpdateText(TextMeshProUGUI tmp)
            {
                var newText = $"Buy {amount} {data.item} for {data.price * amount} <sprite={data.currency.spriteIndex}>";
                if (spriteAssetSet)
                {
                    tmp.text = newText;
                }
                else
                {
                    ArchUI.SetText(tmp, newText, SpriteAssetType.General);
                    spriteAssetSet = true;
                }
            }
        }

        void HandleBuyMax(MerchData data)
        {
            SendToVault(data, data.item.MaxStacks);
        }

        void SendToVault(MerchData merchData, int amount)
        {
            var price = merchData.price;
            var currency = merchData.currency;
            var data = new ItemData(merchData, amount);
            var total = price * amount;

            var canSpend = guildManager.CanSpend(currency, total);
            if (!canSpend) return;
            var newItem = worldActions.CreateItemUI(data, transform);

            var vaulted = guildVault.VaultItem(newItem);

            if (!vaulted)
            {
                newItem.DestroySelf(false);
                return;
            }

            guildManager.SpendCurrency(currency, price);

        }
        #endregion

    }
}
