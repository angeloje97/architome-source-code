using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(ItemSlotHandler))]
    public class GearSlotManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public EntityInfo entityInfo;
        public GearModuleManager moduleManager;
        public ModuleInfo module;
        public GuildManager guildManager;

        [Header("Gear Slot Manager Properties")]
        public Transform equipmentBin;
        public List<GearSlot> gearSlots;
        ItemSlotHandler itemSlotHandler;



        //Update Triggers
        private EntityInfo currentEntity;

        Action<GearSlotManager> OnDestroyEvent;


        public void GetDependencies()
        {
            module = GetComponentInParent<ModuleInfo>();
            guildManager = GetComponentInParent<GuildManager>();

            moduleManager = GetComponentInParent<GearModuleManager>();

            if (moduleManager)
            {
                moduleManager.OnEquipItem += OnTryEquip;
            }

            if (module && !moduleManager.seperateEntities)
            {
                module.OnSelectEntity += OnSelectEntity;
            }

            if (guildManager)
            {
                guildManager.OnSelectEntity += OnSelectEntity;
            }

            itemSlotHandler = GetComponent<ItemSlotHandler>();

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem += OnChangeItem;

            }
        }

        private void OnDestroy()
        {
            OnDestroyEvent?.Invoke(this);
            if (moduleManager)
            {
                moduleManager.OnEquipItem -= OnTryEquip;
            }
            if (module)
            {
                module.OnSelectEntity -= OnSelectEntity;
            }

            if (guildManager)
            {
                guildManager.OnSelectEntity -= OnSelectEntity;
            }

            if (itemSlotHandler)
            {
                itemSlotHandler.OnChangeItem -= OnChangeItem;

            }
        }

        void Start()
        {
            GetDependencies();
        }


        void OnSelectEntity(EntityInfo entity)
        {
            SetEntity(entity);
        }

        public void SetEntity(EntityInfo entity)
        {
            if (entity == null) return;
            if (entityInfo && moduleManager.seperateEntities) return;

            entityInfo = entity;
            SetGearSlots();
            DestroyItems();
            CreateItems();
        }

        public void SetEntitySeperate(EntityInfo entity)
        {
            entity.infoEvents.OnTryEquip += OnTryEquip;

            OnDestroyEvent += (obj) =>
            {
                entity.infoEvents.OnTryEquip -= OnTryEquip;
            };

        }

        

        public void OnTryEquip(ItemInfo info, EntityInfo entity)
        {
            if (entityInfo != entity)
            {
                module.SelectEntity(entity);
            }

            var equipment = (Equipment)info.item;


            var newSlot = Slot(equipment.equipmentSlotType);

            if (!newSlot.CanInsert(info)) return;

            var currentlyEquipped = newSlot.currentItemInfo;

            if (currentlyEquipped)
            {
                info.HandleItem(currentlyEquipped);
                return;
            }

            info.HandleNewSlot(newSlot);
        }

        void HandleWeaponTypeConflicts(GearSlot newSlot)
        {
            if (newSlot.slotType != EquipmentSlotType.OffHand && newSlot.slotType != EquipmentSlotType.MainHand) return;

            var otherSlot = newSlot.slotType == EquipmentSlotType.MainHand ? Slot(EquipmentSlotType.OffHand) : Slot(EquipmentSlotType.MainHand);

            if (otherSlot.item == null || newSlot.item == null) return;

            var newWeapon = (Weapon)newSlot.item;
            var otherWeapon = (Weapon)otherSlot.item;

            var twoHanders = Weapon.TwoHanders;

            if (twoHanders.Contains(newWeapon.weaponType) || twoHanders.Contains(otherWeapon.weaponType))
            {
                entityInfo.LootItem(otherSlot.currentItemInfo);
            }


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
            if (module == null)
            {
                module = GetComponentInParent<ModuleInfo>();
            }
            module.ReturnAllItemsFromBin();

            foreach (var itemInfo in GetComponentsInChildren<ItemInfo>())
            {
                Destroy(itemInfo.gameObject);
            }
        }

        public GearSlot Slot(EquipmentSlotType slotType)
        {
            foreach (var slot in gearSlots)
            {
                if (slot.slotType != slotType) continue;

                return slot;
            }

            return null;
        }

        void OnChangeItem(ItemEventData eventData)
        {
            var gearSlot = (GearSlot)eventData.itemSlot;

            gearSlot.equipmentSlot.equipment = (Equipment)gearSlot.item ? (Equipment)gearSlot.item : null;

            HandleWeaponTypeConflicts(gearSlot);

            if (eventData.previousItem)
            {
                eventData.previousItem.OnItemAction -= OnItemAction;
            }

            if (eventData.newItem)
            {
                eventData.newItem.OnItemAction += OnItemAction;
            }
        }

        async void OnItemAction(ItemInfo info)
        {
            var contextMenu = ContextMenu.current;
            if (contextMenu == null) return;

            var options = new List<ContextMenu.OptionData>()
            {
                new("Unequip", (data) => {
                    entityInfo.LootItem(info);
                }),
                new("Destroy", async(data) => {
                    await info.SafeDestroy();
                })
            };

            

            var response = await contextMenu.UserChoice(new()
            {
                title = info.item.itemName,
                options = options
            });

            var choice = response.index;

        }


        void CreateItems()
        {
            for (int i = 0; i < gearSlots.Count; i++)
            {
                var current = gearSlots[i];

                if (current.equipmentSlot == null ||
                    current.equipmentSlot.equipment == null) { continue; }

                CreateEquipment(current.equipmentSlot.equipment, current);
            }
        }

        public GameObject CreateEquipment(Equipment equipment, GearSlot slot)
        {
            var itemTemplate = module.prefabs.item;

            if (equipmentBin == null)
            {
                equipmentBin = GetComponentInParent<ModuleInfo>().itemBin;
            }

            if (equipmentBin != null)
            {
                var newEquipment = Instantiate(itemTemplate, equipmentBin);

                var itemInfo = newEquipment.GetComponent<ItemInfo>();

                itemInfo.ManifestItem(new() { item = equipment, amount = 1 }, true);

                //itemInfo.item = equipment;
                //itemInfo.UpdateItemInfo();
                //itemInfo.isInInventory = true;
                itemInfo.HandleNewSlot(slot, true);

                return itemInfo.gameObject;
            }

            return null;

        }

    }

}