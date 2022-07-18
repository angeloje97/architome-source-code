using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class GuildVault : MonoBehaviour
    {
        [SerializeField] GuildManager manager;
        [SerializeField] Transform vaultParent;
        [SerializeField] Transform inventorySlots;
        [SerializeField] ArchButton newSlotButton;
        [SerializeField] TextMeshProUGUI newSlotText;

        List<InventorySlot> slots;
        List<ItemData> items;

        private void Start()
        {
            var itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;
            }
        }

        public void InitializeModule()
        {
            items = manager.guildInfo.vault;

            CreateInventorySlots(manager.guildInfo.maxSlots);
            UpdateBuySlotButton();
            CreateItems();
        }

        void CreateInventorySlots(int amount)
        {
            if (vaultParent == null) return;
            var dungeoneering = DungeoneerManager.active;

            var slotPrefab = dungeoneering.prefabs.inventorySlot;
            if (slotPrefab == null) return;

            slots = new();

            for (int i = 0; i < amount; i++)
            {
                slots.Add(Instantiate(slotPrefab, inventorySlots).GetComponent<InventorySlot>());
            }

        }

        void CreateItems()
        {
            var itemInfoPrefab = DungeoneerManager.active.prefabs.itemTemplate;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].item == null) continue;
                if (i < 0 || i >= slots.Count) continue;

                var slot = slots[i];
                var newItemInfo = Instantiate(itemInfoPrefab, vaultParent).GetComponent<ItemInfo>();

                newItemInfo.ManifestItem(items[i], true);

                newItemInfo.HandleNewSlot(slot);
            }
        }

        private void OnChangeItem(ItemEventData eventData)
        {
            var slot = eventData.itemSlot;
            if (!slots.Contains(slot)) return;
            var index = slots.IndexOf(slot);
            items[index] = new(eventData.newItem);
        }



        void CreateItem(ItemData item, InventorySlot slot)
        {
        }

        void UpdateInventorySlots()
        {
            var slotPrefab = DungeoneerManager.active.prefabs.inventorySlot;
            while (slots.Count < manager.guildInfo.maxSlots)
            {
                slots.Add(Instantiate(slotPrefab, inventorySlots).GetComponent<InventorySlot>());
            }
        }

        public void BuyNewSlot()
        {
            var price = SlotPrice();
            if (manager.guildInfo.gold < price) return;

            var success = manager.SpendGold(price);
            if (!success) return;


            manager.guildInfo.maxSlots++;

            UpdateInventorySlots();
            UpdateBuySlotButton();
        }

        public int SlotPrice()
        {
            if (slots == null) return 0;

            return 12 * slots.Count;
        }

        void UpdateBuySlotButton()
        {
            var price = SlotPrice();
            newSlotButton.interactable = price <= manager.guildInfo.gold;
            newSlotText.text = $"Buy Slot ({price}g)";
        }
    }
}
