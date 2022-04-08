using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Architome;

public class GearStatsUI : MonoBehaviour
{
    public GearModuleManager moduleManager;
    public ModuleInfo module;
    public EntityInfo entityInfo;

    public TextMeshProUGUI entityName;

    [Header("Health and Mana")]
    public TextMeshProUGUI healthManaValue;

    [Header("Core Stats")]
    public TextMeshProUGUI vitalityValue;
    public TextMeshProUGUI strengthValue;
    public TextMeshProUGUI dexterityValue;
    public TextMeshProUGUI wisdomValue;

    [Header("Secondary Stats")]
    public TextMeshProUGUI attackSpeedValue;
    public TextMeshProUGUI attackDamageValue;
    public TextMeshProUGUI hasteValue;
    public TextMeshProUGUI criticalChanceValue;
    public TextMeshProUGUI criticalMultiplierValue;
    public TextMeshProUGUI armorValue;
    public TextMeshProUGUI magicResistValue;
    public TextMeshProUGUI manaRegenValue;
    public TextMeshProUGUI healthRegenValue;
    public TextMeshProUGUI movementSpeedValue;


    void GetDependenices()
    {
        moduleManager = GetComponentInParent<GearModuleManager>();
        module = GetComponentInParent<ModuleInfo>();
        


        if (module)
        {
            module.OnSelectEntity += OnSelectEntity;
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
        UpdateStats();

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
        UpdateStats();
    }

    

    void UpdateStats()
    {
        UpdateCoreStats();
        UpdateSecondaryStats();

        void UpdateCoreStats()
        {
            vitalityValue.text = $"{entityInfo.stats.Vitality}";
            strengthValue.text = $"{entityInfo.stats.Strength}";
            dexterityValue.text = $"{entityInfo.stats.Dexterity}";
            wisdomValue.text = $"{entityInfo.stats.Wisdom}";

            healthManaValue.text = $"Health: {entityInfo.maxHealth} Mana: {entityInfo.maxMana}";
        }

        void UpdateSecondaryStats()
        {
            attackSpeedValue.text = $"{entityInfo.stats.attackSpeed}/s";
            attackDamageValue.text = $"{entityInfo.stats.attackDamage}";
            hasteValue.text = $"{entityInfo.stats.haste * 100}%";
            criticalChanceValue.text = $"{entityInfo.stats.criticalStrikeChance * 100}%";
            criticalMultiplierValue.text = $"{entityInfo.stats.criticalDamage * 100}%";
            armorValue.text = $"{entityInfo.stats.armor}";
            magicResistValue.text = $"{entityInfo.stats.magicResist}";
            manaRegenValue.text = $"{entityInfo.stats.manaRegen}/s";
            healthRegenValue.text = $"{entityInfo.stats.healthRegen}/s";
            movementSpeedValue.text = $"{entityInfo.stats.movementSpeed * 100}%";

        }
    }
}
