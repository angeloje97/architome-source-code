using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System;
public class GearModuleManager : MonoBehaviour
{
    public ModuleInfo module;
    public List<EntityInfo> playableEntities;
    public List<Image> entityButtons;

    public GearStatsUI stats;
    public GearSlotManager gearSlotManager;
    public Action<EntityInfo> OnSetEntity;
    public Action<ItemInfo, EntityInfo> OnEquipItem;



    void GetDependencies()
    {
        GameManager.active.OnNewPlayableEntity += OnNewPlayableEntity;

        module = GetComponentInParent<ModuleInfo>();

    }
    void Start()
    {
        GetDependencies();
        
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        var characterInfo = newEntity.GetComponentInChildren<CharacterInfo>();

        if (characterInfo)
        {
            characterInfo.modules.gearModule = this;
        }
        if (index == 0)
        {
            module.OnSelectEntity?.Invoke(newEntity);
        }
    }

    public void EquipItem(ItemInfo info, EntityInfo entity)
    {
        if (!Item.Equipable(info.item)) return;

        OnEquipItem?.Invoke(info, entity);
    }

    private void Update()
    {
    }
    // Update is called once per frame


}
