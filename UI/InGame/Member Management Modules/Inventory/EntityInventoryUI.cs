using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System.Linq;

[RequireComponent(typeof(ItemSlotHandler))]
public class EntityInventoryUI : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public Inventory entityInventory;
    public InventoryManager inventoryManager;
    public List<InventorySlot> inventorySlots;
    //public List<Item> items;
    public List<ItemData> inventoryItems;
    public ModuleInfo module;

    private EntityInfo currentEntity;

    //Update Handlers
    private int itemCount;

    void GetDependencies()
    {
        inventorySlots = GetComponentsInChildren<InventorySlot>().ToList();

        if(GetComponentInParent<InventoryManager>())
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

    }
    void HandleUpdateTriggers()
    {
        HandleNewEntity();



        void HandleNewEntity()
        {
            if(currentEntity != entityInfo)
            {
                //UpdateSlots();
                entityInfo.Inventory().entityInventoryUI = this;
                //items = entityInfo.Inventory().items;
                inventoryItems = entityInfo.Inventory().inventoryItems;
                HandleExistingItems();
                currentEntity = entityInfo;
            }


            void HandleExistingItems()
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    if(i >= inventorySlots.Count) { break; }
                    if (inventoryItems[i].item == null) { continue; }
                    if (inventorySlots[i].item != null) { continue; }

                    var clone = Instantiate(inventoryItems[i].item);

                    CreateItem(clone, inventorySlots[i]);
                   
                }
            }
        }
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {

    }

    void OnChangeItem(ItemEventData eventData)
    {
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
    }

    public void UpdateInventory()
    {
        for(int i = 0; i < inventorySlots.Count; i++)
        {
            inventoryItems[i].item = inventorySlots[i].item;

            if (inventorySlots[i].currentItemInfo)
            {
                inventoryItems[i].amount = inventorySlots[i].currentItemInfo.currentStacks;
            }
            //items[i] = inventorySlots[i].item;
        }
    }
    //void UpdateSlots()
    //{
    //    foreach(InventorySlot i in inventorySlots)
    //    {
    //        i.entityInfo = entityInfo;
    //        i.inventory = entityInfo.Inventory();
    //    }
    //}
    public InventorySlot FirstAvailableSlot()
    {
        foreach(InventorySlot i in inventorySlots)
        {
            if(i.item == null)
            {
                return i;
            }
        }

        return null;
    }
    public GameObject CreateItem(Item item, InventorySlot slot)
    {
        var itemTemplate = inventoryManager.itemTemplate;

        //var newItem = module.CreateItem(item, true);

        if(inventoryManager && inventoryManager.itemBin)
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
    
    public bool Contains(Item item)
    {
        foreach(InventorySlot slot in inventorySlots)
        {
            if (slot.item == item)
            {
                return true;
            }
        }

        return false;
    }
}
