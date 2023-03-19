using System.Collections;
using System.Collections.Generic;
using Architome;
using UnityEngine;
using System;
using UnityEngine.UI;
using Architome.Enums;

[RequireComponent(typeof(CanvasGroup))]
public class EntitySpellBook : MonoBehaviour
{
    // Start is called before the first frame update

    public List<SpellBookSlot> spellSlots;

    public Transform bin;
    public GameObject abilityTemplate;
    public ModuleInfo module;


    void Start()
    {
        //Invoke("GetDependencies", .5f);
        GetDependendencies();
    }

    void GetDependendencies()
    {
        module = GetComponentInParent<ModuleInfo>();

        var itemSlotHandler = GetComponent<ItemSlotHandler>();


        if (itemSlotHandler)
        {
            itemSlotHandler.OnChangeItem += OnChangeItem;
        }
    }

    private void OnChangeItem(ItemEventData eventData)
    {
        var augmentSlot = (AugmentSlot)eventData.itemSlot;
        var ability = augmentSlot.ability;

        HandleAugmentDatas();

        if (eventData.newItem)
        {
            var augmentItem = (AugmentItem) eventData.newItem.item;

            ability.AddAugment(augmentItem);
        }
        if (eventData.previousItem)
        {
            var augmentItem = (AugmentItem) eventData.previousItem.item;

            ability.RemoveAugment(augmentItem);
        }


        void HandleAugmentDatas()
        {
            var index = augmentSlot.Index();

            if (ability.augmentsData == null)
            {
                ability.augmentsData = new();
            }

            while (ability.augmentsData.Count < augmentSlot.GroupSize())
            {
                ability.augmentsData.Add(ItemData.Empty);
            }

            ability.augmentsData[index] = new() { item = augmentSlot.item, amount = augmentSlot.item != null ? 1 : 0 };
        }
    }



    public void SetEntity(EntityInfo entity)
    {
        
        //ClearBin();
        CreateItems(entity);
        
    }

    public void ClearBin()
    {
        GetComponentInParent<ModuleInfo>()?.DestroyBin();

        var abilityUIs = GetComponentsInChildren<AbilityInfoUI>();
        var augments = GetComponentsInChildren<ItemInfo>();
        
        for (int i = 0; i < augments.Length; i++)
        {
            Destroy(augments[i].gameObject);
        }

        for (int i = 0; i < abilityUIs.Length; i++)
        {
            Destroy(abilityUIs[i].gameObject);
        }

        foreach(var slot in spellSlots)
        {
            if(slot.GetComponentInChildren<AbilityInfoUI>())
            {
                Destroy(slot.GetComponentInChildren<AbilityInfoUI>().gameObject);
            }
        }
    }

    public void CreateItems(EntityInfo entity)
    {
        var abilities = entity.GetComponentInChildren<AbilityManager>();

        if(abilities == null) { return; }
        var world = World.active;

        var itemTemplate = world.prefabsUI.item;


        foreach(var slot in spellSlots)
        {
            var ability = abilities.Ability(slot.slotType);

            if(ability == null) { continue; }
            var newUI = Instantiate(abilityTemplate);

            var abilityUI = newUI.GetComponent<AbilityInfoUI>();
            abilityUI.SetAbility(ability);


            slot.SetAbilityUI(abilityUI);

            HandleAugments(slot);
        }

        HandleNewAbility();

        void HandleNewAbility()
        {
            abilities.OnNewAbility += delegate (AbilityInfo ability)
            {
                
                var slot = Slot(ability.abilityType2);
                if (slot == null) return;
                var newUI = Instantiate(abilityTemplate);

                var abilityUI = newUI.GetComponent<AbilityInfoUI>();
                abilityUI.SetAbility(ability);

                slot.SetAbilityUI(abilityUI);
                HandleAugments(slot);
            };
        }

        void HandleAugments(SpellBookSlot slot)
        {
            var augmentSlots = slot.transform.parent.GetComponentsInChildren<AugmentSlot>();
            if (augmentSlots.Length == 0) return;
            if (slot.ability.augmentsData == null) return;
        
            for (int i = 0; i < slot.ability.augmentsData.Count; i++)
            {
                if (i >= augmentSlots.Length) continue;
                var augmentData = slot.ability.augmentsData[i];
                if (augmentData.item == null) continue;
                var newItem = Instantiate(itemTemplate, augmentSlots[i].transform);

                newItem.ManifestItem(augmentData, true);

                newItem.HandleNewSlot(augmentSlots[i]);

                ArchAction.YieldFor(() => { newItem.ReturnToSlot(); }, 2);
            }

        }

        SpellBookSlot Slot(AbilityType2 slotType)
        {
            foreach(var slot in spellSlots)
            {
                if (slot.slotType != slotType) continue;
                return slot;
            }

            return null;
        }

    }
}
