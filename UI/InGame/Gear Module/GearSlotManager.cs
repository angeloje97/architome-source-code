using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class GearSlotManager : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public GearModuleManager moduleManager;
    public ModuleInfo module;

    [Header("Gear Slot Manager Properties")]
    public Transform equipmentBin;
    public GameObject itemTemplate;
    public List<GearSlot> gearSlots;



    //Update Triggers
    private EntityInfo currentEntity;

    void GetDependencies()
    {
        module = GetComponentInParent<ModuleInfo>();
        if (GetComponentInParent<GearModuleManager>())
        {
            moduleManager = GetComponentInParent<GearModuleManager>();
            
        }

        if (module)
        {
            module.OnSelectEntity += OnSelectEntity;
        }
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
            slot.equipmentSlot = entityInfo.CharacterInfo().EquipmentSlot(slot.slotType);
            slot.characterInfo = entityInfo.CharacterInfo();
            slot.entityInfo = entityInfo;
            slot.events.OnSetSlot?.Invoke(slot);
        }
    }

    void DestroyItems()
    {
        foreach(Transform child in equipmentBin)
        {
            Destroy(child.gameObject);
        }

        foreach (var slot in gearSlots)
        {
            slot.item = null;
        }
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
        var itemTemplate = this.itemTemplate;

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
