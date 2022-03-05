using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;

public class GearSlotManager : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    

    [Header("Gear Slot Manager Properties")]
    public Transform equipmentBin;
    public GameObject itemTemplate;
    public List<GearSlot> gearSlots;

    //Update Triggers
    private EntityInfo currentEntity;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!GetComponentInParent<ModuleInfo>().isActive){return;}
        HandleNewEntity();
    }

    public GearModuleManager GearManager()
    {
        return GetComponentInParent<GearModuleManager>() ? GetComponentInParent<GearModuleManager>() : null;
    }

    

    void HandleNewEntity()
    {
        if(currentEntity != entityInfo)
        {
            currentEntity = entityInfo;
            SetGearSlots();
            DestroyItems();
            CreateItems();
        }

        void SetGearSlots()
        {
            if(entityInfo == null || entityInfo.CharacterInfo() == null) { return; }
            
            foreach(GearSlot slot in gearSlots)
            {
                slot.equipmentSlot = entityInfo.CharacterInfo().EquipmentSlot(slot.slotType);
                slot.characterInfo = entityInfo.CharacterInfo();
                slot.entityInfo = entityInfo;
            }
        }
    }

    void DestroyItems()
    {
        foreach(Transform child in equipmentBin)
        {
            Destroy(child.gameObject);
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
