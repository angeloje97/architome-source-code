using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Architome;

public class GearStatsUI : MonoBehaviour
{
    public EntityInfo entityInfo;

    public TextMeshProUGUI entityName;

    [Header("Health and Mana")]
    public TextMeshProUGUI healthManaValue;

    [Header("Core Stats")]
    public TextMeshProUGUI vitalityValue;
    public TextMeshProUGUI strengthValue;
    public TextMeshProUGUI dexterityValue;
    public TextMeshProUGUI wisdomValue;

    //Update Triggers
    private EntityInfo currentEntity;

    private void Update()
    {
        HandleUpdateTriggers();
    }

    void HandleUpdateTriggers()
    {
        HandleNewEntity();
    }
    void HandleNewEntity()
    {
        if (entityInfo != null &&
            currentEntity != entityInfo)
        {
            currentEntity = entityInfo;
            entityName.text = entityInfo.entityName;
            UpdateStats();
        }
    }

    void UpdateStats()
    {
        UpdateCoreStats();

        void UpdateCoreStats()
        {
            vitalityValue.text = $"{entityInfo.stats.Vitality}";
            strengthValue.text = $"{entityInfo.stats.Strength}";
            dexterityValue.text = $"{entityInfo.stats.Dexterity}";
            wisdomValue.text = $"{entityInfo.stats.Wisdom}";

            healthManaValue.text = $"Health: {entityInfo.maxHealth} Mana: {entityInfo.maxMana}";
        }
    }
}
