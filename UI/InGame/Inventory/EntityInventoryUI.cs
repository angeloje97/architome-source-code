using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System.Linq;

public class EntityInventoryUI : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public Inventory entityInventory;
    public InventoryManager inventoryManager;
    public List<InventorySlot> inventorySlots;
    public List<Item> items;
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

        if (GameManager.active)
        {
            GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;
        }
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
                UpdateSlots();
                entityInfo.Inventory().entityInventoryUI = this;
                items = entityInfo.Inventory().items;
                HandleExistingItems();
                currentEntity = entityInfo;
            }


            void HandleExistingItems()
            {
                for (int i = 0; i < items.Count; i++)
                {
                    if(i >= inventorySlots.Count) { break; }
                    if (items[i] == null) { continue; }
                    if (inventorySlots[i].item != null) { continue; }

                    var clone = Instantiate(items[i]);

                    CreateItem(clone, inventorySlots[i]);
                   
                }
            }
        }
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {

    }
    public void UpdateInventory()
    {
        for(int i = 0; i < inventorySlots.Count; i++)
        {
            items[i] = inventorySlots[i].item;
        }
    }
    void UpdateSlots()
    {
        foreach(InventorySlot i in inventorySlots)
        {
            i.entityInfo = entityInfo;
            i.inventory = entityInfo.Inventory();
        }
    }
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
