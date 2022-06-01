using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

[RequireComponent(typeof(ItemSlotHandler))]
public class GearSlotManager : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public GearModuleManager moduleManager;
    public ModuleInfo module;
    public PartyManager partyManager;

    [Header("Gear Slot Manager Properties")]
    public Transform equipmentBin;
    public List<GearSlot> gearSlots;



    //Update Triggers
    private EntityInfo currentEntity;

    void GetDependencies()
    {
        module = GetComponentInParent<ModuleInfo>();
        partyManager = GetComponentInParent<PartyManager>();
        if (GetComponentInParent<GearModuleManager>())
        {
            moduleManager = GetComponentInParent<GearModuleManager>();
            
        }

        if (module)
        {
            module.OnSelectEntity += OnSelectEntity;
        }

        if (partyManager)
        {
            partyManager.OnSelectEntity += OnSelectEntity;
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
        
    }

    public GearModuleManager GearManager()
    {
        return GetComponentInParent<GearModuleManager>() ? GetComponentInParent<GearModuleManager>() : null;
    }

    
    void OnSelectEntity(EntityInfo entity)
    {
        if (entity == null) return;

        entityInfo = entity;
        SetGearSlots();
        DestroyItems();
        CreateItems();
    }


    void SetGearSlots()
    {
        if (entityInfo == null || entityInfo.CharacterInfo() == null) { return; }

        foreach (GearSlot slot in gearSlots)
        {
            if (slot.equipmentSlot != null)
            {
                slot.equipmentSlot.OnLoadEquipment -= OnLoadEquipment;
            }

            slot.entityInfo = entityInfo;
            slot.equipmentSlot = entityInfo.CharacterInfo().EquipmentSlot(slot.slotType);
            slot.characterInfo = entityInfo.CharacterInfo();
            slot.events.OnSetSlot?.Invoke(slot);

            slot.equipmentSlot.OnLoadEquipment += OnLoadEquipment;
        }
    }

    void OnLoadEquipment(Equipment equip)
    {
        DestroyItems();
        CreateItems();
    }

    void DestroyItems()
    {
        foreach (var itemInfo in module.GetComponentsInChildren<ItemInfo>())
        {
            Destroy(itemInfo.gameObject);
        }
    }

    void OnChangeItem(ItemEventData eventData)
    {
        var gearSlot = (GearSlot)eventData.itemSlot;

        gearSlot.equipmentSlot.equipment = (Equipment)gearSlot.item ? (Equipment)gearSlot.item : null;
    }

    
    void CreateItems()
    {
        for(int i = 0; i < gearSlots.Count; i++)
        {
            var current = gearSlots[i];

            if(current.equipmentSlot == null ||
                current.equipmentSlot.equipment == null) { continue; }

            CreateEquipment(current.equipmentSlot.equipment, current);
        }
    }

    public GameObject CreateEquipment(Equipment equipment, GearSlot slot)
    {
        var itemTemplate = module.prefabs.item;

        if(equipmentBin != null)
        {
            var newEquipment = Instantiate(itemTemplate, equipmentBin);

            var itemInfo = newEquipment.GetComponent<ItemInfo>();

            itemInfo.item = equipment;
            itemInfo.UpdateItemInfo();
            itemInfo.isInInventory = true;
            itemInfo.HandleNewSlot(slot);

            return itemInfo.gameObject;
        }

        return null;
        
    }

}
