using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Architome;
using System;
public class PortraitBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public ContainerTargetables targetManager;
    
    public bool isActive;

    [Header("Entity Info")]
    public EntityInfo entity;
    public Image icon;
    public TextMeshProUGUI entityName;

    [Header("Buff Icon Manager")]
    public BuffUIManager buffUI;
    
    [Header("Health Bar")]
    public Image healthBar;
    public Image secondaryHealth;
    public TextMeshProUGUI healthText;

    [Header("Resource Bar")]
    public Image resourceBar;
    public TextMeshProUGUI resourceText;

    [Header("Cast Bar")]
    public Image castBar;
    public TextMeshProUGUI castBarName;
    public TextMeshProUGUI castBarTimer;

    [Header("StatusBar")]
    public Image statusBar;
    public TextMeshProUGUI statusTextName;
    public TextMeshProUGUI statusTextTimer;

    [Header("Experience Bar and Level")]
    public Image experienceBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceBarText;
    
    [Header("Portrait Properties")]
    public float shield;
    public float health;
    public float maxHealth;

    public float mana;
    public float maxMana;

    public int level;
    public float currentExperience;
    public float experienceRequired;

    public Action<EntityInfo, EntityInfo> OnEntityChange;

    public EntityInfo entityInfoCheck;

    public void GetDependencies()
    {
        if(targetManager == null)
        {
            if(GMHelper.TargetManager())
            {
                targetManager = GMHelper.TargetManager();
            }
        }
    }
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        HandleEvents();
        if (!isActive || entity == null) { return; }
        UpdateBars();
        UpdateLevel();

    }

    void HandleEvents()
    {
        if(entityInfoCheck != entity)
        {
            OnEntityChange?.Invoke(entityInfoCheck, entity);
            entityInfoCheck = entity;
        }
    }


    void UpdateBars()
    {
        UpdateHealth();
        UpdateResourceBar();
        UpdateCastBar();

        void UpdateCastBar()
        {
            if(castBar == null) { return; }

            bool currentlyCasting = entity.AbilityManager() && entity.AbilityManager().currentlyCasting && !entity.AbilityManager().currentlyCasting.isAttack;

            castBar.transform.parent.gameObject.SetActive(currentlyCasting);

            if (!currentlyCasting) { return; }
            
            
            var ability = entity.AbilityManager().currentlyCasting;
            var abilityName = ability.abilityName;
            var castTime = ability.castTime;
            var castTimer = ability.castTimer;


            UpdateFill();
            UpdateCastBarTextTime();
            UpdateAbilityName();

            void UpdateFill()
            {
                castBar.fillAmount = castTimer / castTime;
            }
            void UpdateCastBarTextTime()
            {
                if (castBarTimer == null) { return; }
                var timeLeft = 0f;
                if(ability.isCasting && !ability.isChanneling)
                {
                    timeLeft = castTime - castTimer;
                }
                else if (ability.isChanneling)
                {
                    timeLeft = castTimer;
                }
                timeLeft = Mathg.Round(timeLeft, 1);

                castBarTimer.text = $"{timeLeft}s";
            }
            void UpdateAbilityName()
            {
                bool hasName = abilityName.Length > 0;

                if(castBarName.gameObject.activeSelf != hasName)
                {
                    castBarName.gameObject.SetActive(hasName);
                    
                }
                if (castBarName.text != abilityName)
                {
                    castBarName.text = abilityName;
                }
            }
        }
        void UpdateHealth()
        {
            if(!healthText || !healthBar || !secondaryHealth) { return; }
            if (health == entity.health && shield == entity.shield) { return; }

            health = entity.health;
            maxHealth = entity.maxHealth;
            shield = entity.shield;

            var shieldText = shield > 0 ? $"+ {shield}" : "";

            healthBar.fillAmount = (health) / (maxHealth);

            if(health/maxHealth > .95f &&
                health + shield > maxHealth)
            {
                healthBar.fillAmount = .95f;
            }

            secondaryHealth.fillAmount = (health + shield) / (maxHealth);
            if (health != 0) { healthText.text = $"{(int)health}{shieldText}/{(int)maxHealth}"; }
            else { healthText.text = $"Dead"; }

        }
        void UpdateResourceBar()
        {
            if (!resourceBar || !resourceText) { return; }
            if(mana == entity.mana && maxMana == entity.maxMana) { return; }

            mana = entity.mana;
            maxMana = entity.maxMana;

            resourceBar.fillAmount = mana / maxMana;

            resourceText.text = $"{mana}/{maxMana}";
        }
    }

    void OnExperienceGain(float value)
    {
        HandleExperienceBar();
        HandleExperienceText();

        void HandleExperienceBar()
        {
            if (experienceBar == null) return;

            experienceBar.fillAmount = entity.entityStats.experience / entity.entityStats.experienceReq;
        }

        void HandleExperienceText()
        {
            if (experienceBarText == null) { return; }

            experienceBarText.text = $"{(int)entity.entityStats.experience}/{(int)entity.entityStats.experienceReq}";
        }
    }

    void UpdateLevel()
    {
        if(!levelText) { return; }
        if (level == entity.stats.Level) return;
        level = entity.stats.Level;

        levelText.text = $"{level}";
    }

    public void OnNewTargetedBy(GameObject newTarget, List<GameObject> targetedBy)
    {

    }

    public void OnTargetedByRemove(GameObject newTarget, List<GameObject> targetedBy)
    {

    }
    public void SetEntity(EntityInfo entity)
    {
        HandleEvents();

        this.entity = entity;
        UpdateEntity();
        isActive = true;

        buffUI?.SetEntity(entity);

        void HandleEvents()
        {
            if (this.entity != null)
            {
                this.entity.CombatBehavior().CombatInfo().OnNewTargetedBy -= OnNewTargetedBy;
                this.entity.CombatBehavior().CombatInfo().OnTargetedByRemove -= OnTargetedByRemove;
                this.entity.OnExperienceGain -= OnExperienceGain;
            }
        }
    }
    public void ResetEntity()
    {
        entity = null;
        isActive = false;
    }
    public void UpdateEntity()
    {
        if(entity == null || icon==null) { return; }
        entityName.text = $"{entity.entityName}";
        levelText.text = $"{entity.stats.Level}";

        if(entity.entityPortrait == null)
        {
            icon.gameObject.SetActive(false); 
        }
        else 
        {
            icon.gameObject.SetActive(true); icon.sprite = entity.entityPortrait; 
        }

        if (experienceBar)
        {
            experienceBar.fillAmount = entity.entityStats.experience / entity.entityStats.experienceReq;
        }

        

        icon.sprite = entity.entityPortrait;

        HandleEvents();

        void HandleEvents()
        {
            entity.CombatBehavior().CombatInfo().OnNewTargetedBy += OnNewTargetedBy;
            entity.CombatBehavior().CombatInfo().OnTargetedByRemove += OnTargetedByRemove;
            entity.OnExperienceGain += OnExperienceGain;
        }
    }


}
