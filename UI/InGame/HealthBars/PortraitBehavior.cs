using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Architome;
using Architome.Enums;

[RequireComponent(typeof(TargetableEntity))]
public class PortraitBehavior : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // Start is called before the first frame update
    public ContainerTargetables targetManager;
    
    public bool isActive;


    [Serializable]
    public class HealthBar
    {
        public EntityInfo entity;
        public Image healthBar;
        public Image secondaryHealth;
        public TextMeshProUGUI healthText;

        public MetricType textType;

        public Action OnChangeEntity;

        bool showNormalHealth;

        void Clear()
        {
            if (entity == null) return;
            entity.OnHealthChange -= OnHealthChange;
        }

        public void SetEntity(EntityInfo entity)
        {
            if (healthBar == null) return;


            Clear();

            this.entity = entity;
               
            if (this.entity)
            {
                this.entity.OnHealthChange += OnHealthChange;


                OnHealthChange(entity.health, entity.shield, entity.maxHealth);
            }
        }

        public void OnHover(bool hover)
        {
            showNormalHealth = hover;
            OnHealthChange(entity.health, entity.shield, entity.maxHealth);
        }


        void OnHealthChange(float health, float shield, float maxHealth)
        {
            try
            {
                healthBar.fillAmount = health / maxHealth;
                var shieldText = shield > 0 ? $"+ {shield}" : "";
                //if (health + shield > maxHealth && (health / maxHealth) > .95f)
                //{
                //    healthBar.fillAmount = .95f;
                //}

                secondaryHealth.fillAmount = shield / maxHealth;

                if (!showNormalHealth)
                {
                    //healthText.text = $"{(int)health}{shieldText}/{(int)maxHealth}"; 
                    healthText.text = $" ({(int) ((health/maxHealth)*100)})% HP";
                }
                else
                {
                    healthText.text = $"{ArchString.FloatToSimple(health)}/{ArchString.FloatToSimple(maxHealth)} HP";
                }

                if(health == 0f) { healthText.text = $"Dead"; }

            }
            catch
            {

            }

        }


    }

    [Serializable]
    public class StateIcon
    {
        EntityInfo entity;
        public Image stateIcon;
        public Sprite combatSprite;
        public void Clear()
        {
            if (entity == null) return;
            entity.OnCombatChange -= OnCombatChange;
        }
        public void SetEntity(EntityInfo entity)
        {
            if (stateIcon == null) return;
            Clear();
            this.entity = entity;

            if (this.entity)
            {
                this.entity.OnCombatChange += OnCombatChange;
            }
        }

        void OnCombatChange(bool isInCombat)
        {
            if (combatSprite == null) return;

            stateIcon.sprite = combatSprite;

            stateIcon.GetComponent<CanvasGroup>().alpha = isInCombat ? 1 : 0;

        }

        
    }

    public ToolTip currentToolTip;

    public HealthBar healthUI;
    public StateIcon stateIcon;

    [Header("Entity Info")]
    public EntityInfo entity;
    public Image icon;
    public Image iconBorder;
    public TextMeshProUGUI entityName;

    [Header("Buff Icon Manager")]
    public BuffUIManager buffUI;
    

    [Header("Resource Bar")]
    public Image resourceBar;
    public TextMeshProUGUI resourceText;

    [Header("Cast Bar")]
    public Image castBar;
    public TextMeshProUGUI castBarName;
    public TextMeshProUGUI castBarTimer;


    [Header("Experience Bar and Level")]
    public Image experienceBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceBarText;
    

    public float mana;
    public float maxMana;

    public int level;
    public float currentExperience;
    public float experienceRequired;

    public Action<EntityInfo, EntityInfo> OnEntityChange;

    public EntityInfo entityInfoCheck;

    TargetableEntity targetableEntity;

    public void GetDependencies()
    {
        targetManager = ContainerTargetables.active;
        
        if (targetManager)
        {
            targetManager.OnNewHoverTarget += OnNewHoverTarget;
        }

        targetableEntity = GetComponent<TargetableEntity>();

        
    }

    public void OnNewHoverTarget(GameObject previous, GameObject newTarget)
    {
        if (entity == null) return;
        if (newTarget == null)
        {
            healthUI.OnHover(false);
            return;
        }

        var info = newTarget.GetComponent<EntityInfo>();

        if (info == null) return;
        if (info != entity) return;


        healthUI.OnHover(true);
    }

    public void OnNewHoverTarget(EntityInfo previous, EntityInfo newTarget)
    {
        if (entity == null) return;
        if (newTarget == null)
        {
            healthUI.OnHover(false);
            return;
        }


        if (newTarget == null) return;
        if (newTarget != entity) return;


        healthUI.OnHover(true);
    }

    void Start()
    {
        GetDependencies();
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
            HandleListeners();
            OnEntityChange?.Invoke(entityInfoCheck, entity);
            entityInfoCheck = entity;
        }

        void HandleListeners()
        {
            if (entityInfoCheck)
            {
                var combatBehavior = entityInfoCheck.CombatBehavior();
                var abilityManager = entityInfoCheck.AbilityManager();

                if (combatBehavior)
                {
                    combatBehavior.CombatInfo().OnNewTargetedBy -= OnNewTargetedBy;
                    combatBehavior.CombatInfo().OnTargetedByRemove -= OnTargetedByRemove;
                    entityInfoCheck.OnExperienceGain -= OnExperienceGain;
                }

                if (abilityManager)
                {
                    abilityManager.OnAbilityStart -= OnAbilityStart;
                    abilityManager.OnAbilityEnd -= OnAbilityEnd;
                    abilityManager.WhileCasting -= WhileCasting;
                }
            }

            targetableEntity.HandleExit();
            targetableEntity.SetEntity(entity);

            if (entity)
            {
                var combatBehavior = entity.CombatBehavior();
                

                if (combatBehavior)
                {
                    combatBehavior.CombatInfo().OnNewTargetedBy += OnNewTargetedBy;
                    combatBehavior.CombatInfo().OnTargetedByRemove += OnTargetedByRemove;

                }
                entity.OnExperienceGain += OnExperienceGain;

                var abilityManager = entity.AbilityManager();

                if (abilityManager)
                {
                    abilityManager.OnAbilityStart += OnAbilityStart;
                    abilityManager.OnAbilityEnd += OnAbilityEnd;
                    abilityManager.WhileCasting += WhileCasting;

                    if (entity.AbilityManager().currentlyCasting)
                    {
                        if (entity.AbilityManager().currentlyCasting.vfx.showCastBar)
                        {
                            OnAbilityStart(entity.AbilityManager().currentlyCasting);
                            return;
                        }
                    }
                }

                SetCastBar(false);
            }
        }
    }
    void UpdateBars()
    {
        UpdateResourceBar();
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (entity == null) return;
        return;
        entity.infoEvents.OnMouseHover?.Invoke(entity, true, gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (entity == null) return;
        return;
        entity.infoEvents.OnMouseHover?.Invoke(entity, false, gameObject);

    }
    public void OnNewTargetedBy(EntityInfo newTarget, ActionHashList<EntityInfo> targetedBy)
    {

    }
    public void OnTargetedByRemove(EntityInfo newTarget, ActionHashList<EntityInfo> targetedBy)
    {

    }
    public void SetEntity(EntityInfo entity)
    {
        this.entity = entity;
        healthUI.SetEntity(entity);
        stateIcon.SetEntity(entity);
        buffUI?.SetEntity(entity);
        UpdateEntity();
        isActive = true;
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

        UpdateIconColor();

        var portraitIcon = entity.PortraitIcon();

        if(portraitIcon == null)
        {
            icon.gameObject.SetActive(false); 
        }
        else 
        {
            icon.gameObject.SetActive(true); icon.sprite = portraitIcon; 
        }

        if (experienceBar)
        {
            experienceBar.fillAmount = entity.entityStats.experience / entity.entityStats.experienceReq;
        }

        icon.sprite = portraitIcon;

        mana = 1;
    }
    public void OnAbilityStart(AbilityInfo ability)
    {
        if (ability == null) return;
        if (castBar == null) return;
        if (!ability.vfx.showCastBar) return;
        var hasName = false;
        if (castBarName)
        {
            if (ability.abilityName.Length > 0)
            {
                castBarName.text = ability.abilityName;
                hasName = true;
            }
        }

        SetCastBar(true, hasName);
    }
    public void SetCastBar(bool enable, bool enableAbilityName = false)
    {
        if (castBar == null || castBarName == null) return;
        castBar.transform.parent.gameObject.SetActive(enable);

        castBarName.gameObject.SetActive(enableAbilityName);
    }

    public void WhileCasting(AbilityInfo ability)
    {
        if (castBar == null) return;
        if (ability.isAttack) return;
        if (!ability.vfx.showCastBar) return;
        castBar.fillAmount = ability.progress;
        castBarTimer.text = $"{Mathg.Round(ability.progressTimer, 1)}s";
    }

    public void OnAbilityEnd(AbilityInfo ability)
    {
        if (castBar == null) return;
        if (!ability.vfx.showCastBar) return;

        SetCastBar(false, true);
    }

    public void UpdateIconColor()
    {
        if (iconBorder == null) return;

        var archClass = entity.archClass;

        if (archClass == null) return;

        iconBorder.color = archClass.classColor;
    }




}
