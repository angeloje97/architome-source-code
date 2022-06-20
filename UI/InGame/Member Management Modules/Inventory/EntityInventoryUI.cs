using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class EntityInventoryUI : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        public Inventory entityInventory;
        public CharacterInfo entityCharacter;
        public InventoryManager inventoryManager;
        public List<InventorySlot> inventorySlots;
        public List<ItemData> inventoryItems;
        public ModuleInfo module;

        private EntityInfo currentEntity;

        //Update Handlers
        private int itemCount;

        void GetDependencies()
        {
            inventorySlots = GetComponentsInChildren<InventorySlot>().ToList();

            if (GetComponentInParent<InventoryManager>())
            {
                inventoryManager = GetComponentInParent<InventoryManager>();
            }
            module = GetComponentInParent<ModuleInfo>();

            if (GameManager.active)
            {
                GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
            }


            GetComponent<ItemSlotHandler>().OnChangeItem += OnChangeItem;
        }

        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            HandleUpdateTriggers();
        }

        public void SetEntity(EntityInfo entity)
        {

            entityInfo = entity;
            entityInventory = entityInfo.Inventory();
            entityInventory.entityInventoryUI = this;
            inventoryItems = entityInfo.Inventory().inventoryItems;
            entityCharacter = entity.GetComponentInChildren<CharacterInfo>();
            entityInventory.OnLoadInventory += OnLoadInventory;

            HandleExistingItems();
        }
        void HandleUpdateTriggers()
        {
            //HandleNewEntity();



            void HandleNewEntity()
            {
                if (currentEntity != entityInfo)
                {
                    //UpdateSlots();
                    entityInfo.Inventory().entityInventoryUI = this;
                    //items = entityInfo.Inventory().items;
                    inventoryItems = entityInfo.Inventory().inventoryItems;
                    currentEntity = entityInfo;
                }



            }
        }
        void HandleExistingItems()
        {
            if (entityInfo == null) return;
            for (int i = 0; i < inventoryItems.Count; i++)
            {
                if (i >= inventorySlots.Count) { break; }
                if (inventoryItems[i].item == null) { continue; }
                if (inventorySlots[i].item != null) { continue; }

                var clone = Instantiate(inventoryItems[i].item);

                CreateItem(clone, inventorySlots[i]);

            }
        }

        public void RedrawInventory()
        {
            ClearItems();
            ArchAction.Yield(() => HandleExistingItems());
        }

        void OnLoadInventory(Inventory inventory)
        {
            ClearItems();
            ArchAction.Yield(() => HandleExistingItems());


        }

        void ClearItems()
        {
            var items = GetComponentsInChildren<ItemInfo>();

            if (items.Length == 0) return;

            for (int i = 0; i < items.Length; i++)
            {
                var item = items[i].gameObject;
                Destroy(item);

            }
        }

        public void OnNewPlayableEntity(EntityInfo newEntity, int index)
        {

        }

        void OnChangeItem(ItemEventData eventData)
        {

            HandlePreviousItem();
            HandleNewItem();
            var index = inventorySlots.IndexOf(eventData.itemSlot);

            //items[index] = eventData.itemSlot.item;
            inventoryItems[index].item = eventData.itemSlot.item;

            if (eventData.newItem)
            {
                inventoryItems[index].amount = eventData.newItem.currentStacks;
            }
            else
            {
                inventoryItems[index].amount = 0;
            }

            void HandlePreviousItem()
            {
                if (eventData.previousItem == null) return;
                eventData.previousItem.OnItemAction -= OnItemAction;
            }

            void HandleNewItem()
            {
                if (eventData.newItem == null) return;

                eventData.newItem.OnItemAction += OnItemAction;
            }
        }

        public void UpdateInventory()
        {
            for (int i = 0; i < inventorySlots.Count; i++)
            {
                inventoryItems[i].item = inventorySlots[i].item;

                if (inventorySlots[i].currentItemInfo)
                {
                    inventoryItems[i].amount = inventorySlots[i].currentItemInfo.currentStacks;
                }
                //items[i] = inventorySlots[i].item;
            }
        }

        public InventorySlot FirstAvailableSlot()
        {
            foreach (InventorySlot i in inventorySlots)
            {
                if (i.item == null)
                {
                    return i;
                }
            }

            return null;
        }
        public GameObject CreateItem(Item item, InventorySlot slot)
        {
            var itemTemplate = module.prefabs.item;

            //var newItem = module.CreateItem(item, true);

            if (inventoryManager && inventoryManager.itemBin)
            {
                var newItem = Instantiate(itemTemplate, inventoryManager.itemBin);
                var itemInfo = newItem.GetComponent<ItemInfo>();

                itemInfo.item = item;
                itemInfo.UpdateItemInfo();
                itemInfo.isInInventory = true;


                itemInfo.HandleNewSlot(slot);
                return itemInfo.gameObject;
            }

            return null;
        }

        async public void OnItemAction(ItemInfo info)
        {
            var contextMenu = ContextMenu.current;

            if (contextMenu == null) return;

            var options = new List<string>()
            {
                "Destroy"
            };
            UpdateOptions();

            var name = info.item.itemName;

            var choice = await contextMenu.UserChoice(new()
            {
                options = options,
                title = name,
            });

            if (choice < 0 || choice >= options.Count) return;


            HandleDestroy();
            HandleEquip();

            void UpdateOptions()
            {
                if (Item.Equipable(info.item))
                {
                    options.Insert(0, "Equip");
                }
            }

            async void HandleDestroy()
            {
                if (options[choice] != "Destroy") return;

                var optionPicked = await PromptHandler.active.GeneralPrompt(new() {
                    icon = info.item.itemIcon,
                    title = $"{info.item.itemName}",
                    question = $"Are you sure you want to destroy {info.item.itemName}?",
                    option1 = "Destroy",
                    option2 = "Cancel"
                });

                if (optionPicked == 0)
                {
                    Destroy(info.gameObject);
                }

            }

            void HandleEquip()
            {
                if (options[choice] != "Equip") return;
                if (entityCharacter == null) return;

                entityCharacter.modules.gearModule.EquipItem(info, entityInfo);
            }
        }

        public bool Contains(Item item)
        {
            foreach (InventorySlot slot in inventorySlots)
            {
                if (slot.item == item)
                {
                    return true;
                }
            }

            return false;
        }
    }

}