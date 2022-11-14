using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System;
using JetBrains.Annotations;

public class GearModuleManager : MonoBehaviour
{
    public ModuleInfo module;
    public GameManager gameManager;
    public List<EntityInfo> playableEntities;
    public List<Image> entityButtons;

    public GearStatsUI stats;
    public GearSlotManager gearSlotManager;
    public Action<EntityInfo> OnSetEntity;
    public Action<ItemInfo, EntityInfo> OnEquipItem { get; set; }

    //public Dictionary<EntityInfo, List<CanvasGroup>> entityDict;



    [Header("Prefabs")]
    public GearStatsUI statsPrefab;
    public GearSlotManager slotManager;

    [Header("States")]
    public Transform statsParent;
    public Transform slotsParent;
    public bool seperateEntities;
    public bool defaultModule;


    void GetDependencies()
    {
        gameManager = GameManager.active;
        module = GetComponentInParent<ModuleInfo>();
        gameManager.OnNewPlayableEntity += OnNewPlayableEntity;

        if (!seperateEntities)
        {
            defaultModule = true;
        }

        if (defaultModule)
        {
            seperateEntities = false;
            module.OnSelectEntity += SelectEntity;
        }



    }
    void Start()
    {
        GetDependencies();
        HandleSeperateEntity();
        
    }

    void HandleSeperateEntity()
    {
        if (!seperateEntities) return;

        EntityInfo currentEntity = null;
        Dictionary<EntityInfo, List<CanvasGroup>> entityDict = new();
        module.OnSelectEntity += HandleSelectEntity;
        gameManager.OnNewPlayableEntity += HandleNewPlayableEntity;

        Destroy(stats.gameObject);
        Destroy(gearSlotManager.gameObject);


        void HandleNewPlayableEntity(EntityInfo entity, int index)
        {
            var newStats = Instantiate(statsPrefab.gameObject, statsParent).GetComponent<GearStatsUI>();
            var newSlotManager = Instantiate(slotManager, slotsParent).GetComponent<GearSlotManager>();

            newStats.SetEntity(entity);
            newSlotManager.SetEntity(entity);

            var statCanvas = newStats.GetComponent<CanvasGroup>();
            var slotCanvas = newSlotManager.GetComponent<CanvasGroup>();


            entityDict.Add(entity, new() {
            statCanvas,
            slotCanvas
            });

            ArchUI.SetCanvases(entityDict[entity], false);

            if (index == 0)
            {
                HandleSelectEntity(entity);
            }
        }


        void HandleSelectEntity(EntityInfo entity)
        {
            if (!entityDict.ContainsKey(entity)) return;
            if (entityDict == null) return;

            if (currentEntity)
            {
                ArchUI.SetCanvases(entityDict[currentEntity], false);
            }

            currentEntity = entity;

            ArchUI.SetCanvases(entityDict[entity], true);
        }
    }

    public void SelectEntity(EntityInfo entity)
    {
        OnSetEntity?.Invoke(entity);
    }

    public void OnNewPlayableEntity(EntityInfo newEntity, int index)
    {
        var characterInfo = newEntity.GetComponentInChildren<CharacterInfo>();

        if (characterInfo)
        {
            characterInfo.modules.gearModule = this;
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
