using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Architome.Enums;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

        public Transform inventorySlotParent;
        public List<InventorySlot> inventorySlots;
        public List<ItemData> inventoryItems;
        public ModuleInfo module;

        [Serializable]
        public struct Components
        {
            public Icon entityIcon;
            public TextMeshProUGUI entityName;
        }



        public Components comps;

        private EntityInfo currentEntity;

        public bool dynamicInventory;
        [SerializeField] bool update;

        //Update Handlers
        private int itemCount;

        void GetDependencies()
        {
            inventorySlots = GetComponentsInChildren<InventorySlot>().ToList();

            inventoryManager = GetComponentInParent<InventoryManager>();

            
            module = GetComponentInParent<ModuleInfo>();

            if (GameManager.active)
            {
                GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
            }

            if (module)
            {
                if (dynamicInventory)
                {
                    module.OnSelectEntity += SetEntity;
                }
            }
            var itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;
                itemSlotHandler.OnItemAction += OnItemAction;
                itemSlotHandler.OnNullHover += OnNullHover;
            }
        }

        void Start()
        {
            GetDependencies();
        }

        private void OnValidate()
        {
            if (!update) return; update = false;

            inventorySlots = GetComponentsInChildren<InventorySlot>().ToList();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void SetEntity(EntityInfo entity)
        {

            entityInfo = entity;
            entityInventory = entityInfo.Inventory();
            entityInventory.entityInventoryUI = this;
            inventoryItems = entityInfo.Inventory().inventoryItems;
            entityCharacter = entity.GetComponentInChildren<CharacterInfo>();
            //entityInventory.OnLoadInventory += OnLoadInventory;

            HandleInventorySlots();
            UpdateComponents();
            HandleExistingItems();
        }

        public void UpdateComponents()
        {
            if (entityInfo == null) return;
            
            if (comps.entityIcon)
            {
                comps.entityIcon.SetIcon(new()
                {
                    sprite = entityInfo.PortraitIcon()
                });
            }

            if (comps.entityName)
            {
                comps.entityName.text = entityInfo.entityName;
            }
        }

        void HandleInventorySlots()
        {
            if (!dynamicInventory) return;
            if (inventorySlotParent == null) return;
            if (module == null) return;

            var slotPrefab = module.prefabs.inventorySlot;

            if (slotPrefab == null) return;

            if (inventorySlots != null)
            {
                for (int i = 0; i < inventorySlots.Count; i++)
                {
                    Destroy(inventorySlots[i].gameObject);
                    inventorySlots.RemoveAt(i);
                    i--;
                }
            }

            inventorySlots = new();

            for (int i = 0; i < inventoryItems.Count; i++)
            {
                inventorySlots.Add(Instantiate(slotPrefab, inventorySlotParent).GetComponent<InventorySlot>());
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

                //var clone = Instantiate(inventoryItems[i].item);

                var newItem = CreateItem(inventoryItems[i], inventorySlots[i]);
                
            }
        }

        public void RedrawInventory()
        {
            ClearItems();
            ArchAction.Yield(() => HandleExistingItems());
        }

        public void OnNullHover(ItemInfo item)
        {
            var gameState = GameManager.active.GameState;

            HandlePlay();
            HandleLobby();

            void HandlePlay()
            {
                if (gameState != GameState.Play) return;
                var worldActions = WorldActions.active;
                if (worldActions == null) return;

                var newItem = worldActions.DropItem(new(item), entityInfo.transform.position, false);

                if (newItem)
                {
                    item.DestroySelf();
                }
            }

            async void HandleLobby()
            {
                if (gameState != GameState.Lobby) return;
                await item.SafeDestroy();

            }
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

            Debugger.UI(5914, $"{eventData.itemSlot} in index {index}");

            inventoryItems[index] = new(eventData.newItem);


            

            void HandlePreviousItem()
            {
                if (eventData.previousItem == null) return;

            }

            void HandleNewItem()
            {
                if (eventData.newItem == null) return;


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

                if (inventorySlots[i].item == null)
                {
                    inventoryItems[i].amount = 0;
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

        public ItemInfo StackableItem(ItemInfo info)
        {
            foreach (var slot in inventorySlots)
            {
                if (slot.currentItemInfo == null) continue;
                if (!info.SameItem(slot.currentItemInfo)) continue;
                if (!slot.currentItemInfo.OpenToStack()) continue;


                return slot.currentItemInfo;
            }
            return null;
        }

        public InventorySlot InventorySlot(int index)
        {
            if (index < 0) return null;
            if (index >= inventorySlots.Count) return null;
            return inventorySlots[index];
        }
        public ItemInfo CreateItem(ItemData data, InventorySlot slot)
        {
            var itemPrefab = World.active.prefabsUI.item;

            var newItem = Instantiate(itemPrefab, transform).GetComponent<ItemInfo>();
            newItem.ManifestItem(data, true);

            //var newItem = module.CreateItem(data, true);
            //Debugger.UI(3914, $"{newItem.item} {newItem.currentStacks}");

            newItem.HandleNewSlot(slot);


            newItem.ReturnToSlot(3);

            return newItem;

            return null;
            var item = data.item;
            var amount = data.amount;

            var itemTemplate = module.prefabs.item;

            //var newItem = module.CreateItem(item, true);

            if (inventoryManager && inventoryManager.itemBin)
            {
                var itemInfo = newItem.GetComponent<ItemInfo>();

                itemInfo.item = item;
                itemInfo.UpdateItemInfo();
                itemInfo.currentStacks = amount;

                itemInfo.HandleNewSlot(slot);
                return itemInfo;
            }

            return null;
        }

        async public void OnItemAction(ItemInfo info)
        {
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return;

            var gameState = GameManager.active ? GameManager.active.GameState : GameState.Lobby;

            var options = new List<string>();
        
            UpdateOptions();
            options.Add("Destroy");
            var name = info.item.itemName;

            var response = await contextMenu.UserChoice(new()
            {
                options = options,
                title = name,
                
            });

            var choice = response.index;

            if (choice < 0 || choice >= options.Count) return;


            HandleDestroy();
            HandleEquip();
            HandleUse();
            HandleSplit();
            HandleSplitInHalf();
            HandleVaultItem();

            void UpdateOptions()
            {
                if (info.currentStacks > 1)
                {
                    options.Insert(0, "Split in Half");
                    options.Insert(0, "Split");
                }

                if (Item.Equipable(info.item))
                {
                    options.Insert(0, "Equip");
                }

                if (Item.Useable(info.item) && gameState == GameState.Play)
                {
                    options.Insert(0, "Use");
                }

                if (gameState == GameState.Lobby)
                {
                    options.Add("Store in Vault");
                }

            }

            async void HandleDestroy()
            {
                if (response.stringValue != "Destroy") return;

                 await info.SafeDestroy();
            }

            void HandleEquip()
            {
                if (options[choice] != "Equip") return;
                if (entityCharacter == null) return;

                entityInfo.infoEvents.OnTryEquip?.Invoke(info, entityInfo);

                //entityCharacter.modules.gearModule.EquipItem(info, entityInfo);
            }

            async void  HandleSplit()
            {
                if (options[choice] != "Split") return;
                var availableSlot = FirstAvailableSlot();

                if(availableSlot == null) return;

                var newItem =  await info.HandleSplit();

                if (newItem == null) return;

                newItem.HandleNewSlot(availableSlot);
            }

            void HandleSplitInHalf()
            {
                if (options[choice] != "Split in Half") return;
                var availableSlot = FirstAvailableSlot();
                if(availableSlot == null) return;

                var newItem = info.SplitHalf();

                if (newItem == null) return;

                newItem.HandleNewSlot(availableSlot);
            }

            void HandleUse()
            {
                if (options[choice] != "Use") return;
                if (entityCharacter == null) return;
                info.Use(new() {
                    itemInfo = info,
                    entityUsed = entityInfo
                });
            }

            void HandleVaultItem()
            {
                if (response.stringValue != "Store in Vault") return;

                Debugger.UI(8718, $"Trying to store {info} in vault");

                var guildVault = GuildVault.active;

                guildVault.VaultItem(info);
            }
        }

        public void OnUpdateItem(ItemInfo info)
        {
            var slot = info.currentSlot;

            if (!inventorySlots.Contains(slot)) return;

            var index = inventorySlots.IndexOf(slot);

            inventoryItems[index] = new() { item = info.item, amount = info.currentStacks };
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