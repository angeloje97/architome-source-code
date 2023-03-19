using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class ShopItemHandler : MonoBehaviour
    {
        [SerializeField] Shop shop;
        [SerializeField] Transform slotParent;

        WorldActions worldActions;
        ItemSlotHandler slotHandler;

        void Start()
        {
            GetDependencies();
            CreateItems();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            shop = GetComponent<Shop>();
            worldActions = WorldActions.active;
            slotHandler = GetComponent<ItemSlotHandler>();
        }

        void AddMerch(MerchData merchData)
        {
            //var slot = Instantiate(slotTemplate, slotParent);
            var slot = worldActions.CreateInventorySlot(slotParent);
            var newItemInfo = worldActions.CreateItemUI(merchData, slot.transform, false);
            newItemInfo.ManifestItem(merchData, true);
            newItemInfo.HandleNewSlot(slot);

            newItemInfo.BeforeShowItemToolTip += (ItemInfo item, ToolTipElement element) => {
                var currencyIndex = merchData.currency.spriteIndex;
                element.data.value = $"{merchData.price} <sprite={currencyIndex}>";
            };



            newItemInfo.OnItemAction += (ItemInfo item) => {
                shop.OnMerchAction?.Invoke(merchData);
            };
        }

        

        void CreateItems()
        {
            foreach(var merch in shop.merchandise)
            {
                AddMerch(merch);
            }
        }



    }
}
