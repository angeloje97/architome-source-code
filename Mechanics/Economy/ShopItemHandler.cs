using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            if (slotHandler)
            {
                slotHandler.SetLockItems(true);
            }
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

        async Task AddMerch(MerchData merchData)
        {
            var slot = worldActions.CreateInventorySlot(slotParent);
            await Task.Delay(0625);

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

        

        async void CreateItems()
        {
            var tasks = new List<Task>();
            foreach(var merch in shop.merchandise)
            {
                tasks.Add(AddMerch(merch));
            }

            await Task.WhenAll(tasks);
        }



    }
}
