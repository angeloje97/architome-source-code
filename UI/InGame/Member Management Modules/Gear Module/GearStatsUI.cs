using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Architome;
using System;
using System.Reflection;

public class GearStatsUI : MonoBehaviour
{
    public GearModuleManager moduleManager;
    public ModuleInfo module;
    public EntityInfo entityInfo;
    public PartyManager partyManager;
    public TextMeshProUGUI entityName;

    [Serializable]
    public struct Info
    {
        public Transform coreGroup;
        public Transform secondaryGroup;
    }

    [Serializable]
    public struct Prefabs
    {
        public GameObject singleCore;
        public GameObject singleSecondary;
        
    }

    public Prefabs prefabs;
    public Info info;
    

    public List<GearStatSingle> singlesCore;
    public List<GearStatSingle> singlesSecondary;

    public Dictionary<string, GearStatSingle> statMaps;

    [Header("Health and Mana")]
    public TextMeshProUGUI healthManaValue;

    //[Header("Core Stats")]
    //public TextMeshProUGUI vitalityValue;
    //public TextMeshProUGUI strengthValue;
    //public TextMeshProUGUI dexterityValue;
    //public TextMeshProUGUI wisdomValue;

    //[Header("Secondary Stats")]
    //public TextMeshProUGUI attackSpeedValue;
    //public TextMeshProUGUI attackDamageValue;
    //public TextMeshProUGUI hasteValue;
    //public TextMeshProUGUI criticalChanceValue;
    //public TextMeshProUGUI criticalMultiplierValue;
    //public TextMeshProUGUI armorValue;
    //public TextMeshProUGUI magicResistValue;
    //public TextMeshProUGUI manaRegenValue;
    //public TextMeshProUGUI healthRegenValue;
    //public TextMeshProUGUI movementSpeedValue;


    void GetDependenices()
    {
        moduleManager = GetComponentInParent<GearModuleManager>();
        module = GetComponentInParent<ModuleInfo>();

        partyManager = GetComponentInParent<PartyManager>();


        if (partyManager)
        {
            partyManager.OnSelectEntity += OnSelectEntity;
        }


        if (module)
        {
            module.OnSelectEntity += OnSelectEntity;
        }

        CreateSingleStats();
    }

    private void OnDestroy()
    {
        if (entityInfo)
        {
            entityInfo.OnChangeStats -= OnChangeStats;
        }
    }

    private void Start()
    {
        GetDependenices();
    }


    void OnSelectEntity(EntityInfo entity)
    {
        HandleOldEntity();
        entityInfo = entity;
        entityName.text = entity.entityName;
        HandleNewEntity();
        UpdateNewStats();

        void HandleOldEntity()
        {
            if (entityInfo == null) return;
            entityInfo.OnChangeStats -= OnChangeStats;

        }

        void HandleNewEntity()
        {
            entityInfo.OnChangeStats += OnChangeStats;
        }
    }

    void OnChangeStats(EntityInfo entityInfo)
    {
        if (gameObject == null)
        {
            entityInfo.OnChangeStats -= OnChangeStats;
            return;
        }
        UpdateNewStats();
    }

    void CreateSingleStats()
    {
        if (prefabs.singleCore == null || prefabs.singleSecondary == null) return;
        if (info.secondaryGroup == null) return;
        if (info.coreGroup == null) return;

        singlesCore = new();
        singlesSecondary = new();
        statMaps = new();

        Stats templateStats = new();

        foreach (var field in typeof(Stats).GetFields())
        {
            if (Stats.HiddenFields.Contains(field.Name)) continue;

            bool isInt = field.GetValue(templateStats).GetType() == typeof(int);

            var prefab = isInt ? prefabs.singleCore : prefabs.singleSecondary;

            var newSingle = Instantiate(prefab, transform).GetComponent<GearStatSingle>();

            newSingle.SetSingle(ArchString.CamelToTitle(field.Name));

            if (isInt)
            {
                newSingle.transform.SetParent(info.coreGroup);
                singlesCore.Add(newSingle);
            }
            else
            {
                newSingle.transform.SetParent(info.secondaryGroup);
                singlesSecondary.Add(newSingle);
            }

            statMaps.Add(field.Name, newSingle);
        }
    }
        
    void UpdateNewStats()
    {
        if (statMaps == null) return;
        if (entityInfo == null) return;

        if (healthManaValue)
        {
            healthManaValue.text = $"Health: {entityInfo.maxHealth} Mana: {entityInfo.maxMana}";
        }

        foreach (var field in typeof(Stats).GetFields())
        {
            if (!statMaps.ContainsKey(field.Name)) continue;

            var single = statMaps[field.Name];
            var value = field.FieldType == typeof(int) ? (int)field.GetValue(entityInfo.stats) : (float)field.GetValue(entityInfo.stats);

            var significant = value > 0;

            if (single.gameObject.activeSelf != significant)
            {
                single.gameObject.SetActive(significant);
            }

            if (!significant)
            {
                continue;
            }

            var multiplier = 1;
            var extraString = "";

            if (Stats.PercentageFields.Contains(field.Name))
            {
                multiplier = 100;
                extraString = "%";
            }

            var newValue = $"{ArchString.FloatToSimple(value*multiplier)}{extraString}";

            single.UpdateSingle(newValue);

        }


    }
}
