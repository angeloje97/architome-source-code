using Architome.Enums;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System;
using System.Threading.Tasks;
using UnityEngine.EventSystems;
[RequireComponent(typeof(TargetableEntity))]
public class ProgressBarsBehavior : EntityProp, IPointerEnterHandler, IPointerExitHandler
{
    public GraphicsInfo graphicsInfo;
    public EntityClusterAgent clusterAgent;
    public AbilityManager abilityManager;

    [Header("Game Objects")]
    public RectTransform healthBarRect;
    public RectTransform resourceBarRect;
    public RectTransform castBarRect;

    [Header("HealthBar")]
    public Image healthBar;
    public Image shieldBar;

    [Header("ResourceBar")]
    public Image resourceBar;

    [Header("CastBar")]
    public AbilityInfo currentAbility;
    public Image castBar;
    public CanvasGroup castBarCanvas;
    public bool castBarActive;

    //Original Properties
    public float originalHealthBarHeight;
    public float originalResourceBarHeight;
    public float originalCastBarHeight;
    public float clusterAgentOffset;
    public Vector3 localPosition;

    

    public TaskProgressBarHandler taskProgressHandler;


    CanvasGroup canvasGroup;
    float targetAlpha;
    float canvasTimer;
    bool activeTimer;
    bool changingAlpha;

    public override void GetDependencies()
    {
        graphicsInfo = GetComponentInParent<GraphicsInfo>();

        if (entityInfo)
        {
            SetHealthBarColor();
            entityInfo.OnHealthChange += OnHealthChange;
            entityInfo.OnManaChange += OnManaChange;
            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnChangeNPCType += OnChangeNPCType;
            entityInfo.OnCombatChange += OnCombatChange;

            combatEvents.AddListenerHealth(eHealthEvent.OnHealingTaken, OnHealingTaken, this);

            entityInfo.OnNewBuff += OnNewBuff;
            OnCombatChange(entityInfo.isInCombat);

            abilityManager = entityInfo.AbilityManager();

            var targetableEntity = GetComponent<TargetableEntity>();
            targetableEntity.SetEntity(entityInfo);
        }

        if(abilityManager)
        {


            abilityManager.OnAbilityStart += OnAbilityStart;


        }

        if(graphicsInfo)
        {
            var clusterAgent = graphicsInfo.EntityClusterAgent();
            if(clusterAgent)
            {
                clusterAgent = graphicsInfo.EntityClusterAgent();
                clusterAgent.OnClusterEnter += OnClusterEnter;
                clusterAgent.OnClusterExit += OnClusterExit;
                clusterAgent.OnClusterChange += OnClusterChange;
            }
        }

        if(healthBarRect.gameObject.activeInHierarchy)
        {
            originalHealthBarHeight = healthBarRect.localPosition.y;
            clusterAgentOffset += healthBarRect.rect.height;
        }

        if(resourceBarRect.gameObject.activeInHierarchy)
        {
            originalResourceBarHeight = resourceBarRect.localPosition.y;
            clusterAgentOffset += resourceBarRect.rect.height;
        }


        var rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, clusterAgentOffset);
        clusterAgentOffset += castBarRect.rect.height;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup && entityInfo)
        {
            canvasGroup.alpha = entityInfo.isInCombat ? 1f : 0f;
        }

        HandleHideMana();
        UpdateCastBar();
        taskProgressHandler.Initialize(this);

    }
    public void OnValidate()
    {
        localPosition = transform.localPosition;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        return;
        entityInfo.infoEvents.OnMouseHover?.Invoke(entityInfo, true, gameObject);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        return;
        entityInfo.infoEvents.OnMouseHover?.Invoke(entityInfo, false, gameObject);
    }

    public void OnClusterEnter(EntityCluster cluster, int index)
    {
        transform.localPosition = localPosition + new Vector3(0, index * 1, 0);
    }
    public void OnClusterExit(EntityCluster cluster, int index)
    {
        transform.localPosition = localPosition;
        
    }

    public void OnClusterChange(EntityCluster cluster, int index)
    {
        transform.localPosition = localPosition + new Vector3(0, index * 1, 0);
    }

    public async void OnCombatChange(bool isInCombat)
    {
        if (!isInCombat) return;
        while (entityInfo.isInCombat)
        {
            UpdateCanvasTimer(4f);
            await Task.Delay(3000);
        }

    }

    async void UpdateCanvas(bool val)
    {
        targetAlpha = val ? 1f : 0f;
        if (canvasGroup == null) return;
        if (canvasGroup.alpha == targetAlpha) return;
        canvasGroup.interactable = targetAlpha == 1f;
        canvasGroup.blocksRaycasts = targetAlpha == 1f;

        if (changingAlpha) return;

        changingAlpha = true;


        while (canvasGroup != null && canvasGroup.alpha != targetAlpha)
        {
            if (!Application.isPlaying) return;
            if (!entityInfo.isAlive)
            {
                canvasGroup.alpha = targetAlpha;
                break;
            }

            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, .125f);
            await Task.Yield();
        }

        changingAlpha = false;
    }
    async public void UpdateCanvasTimer(float timer)
    {
        if (timer > canvasTimer)
        {
            canvasTimer = timer;
        }


        targetAlpha = 1f;

        UpdateCanvas(true);

        if (activeTimer) return;
        activeTimer = true;

        while (canvasTimer > 0)
        {
            canvasTimer -= Time.deltaTime;

            await Task.Yield();
        }

        activeTimer = false;
        
        targetAlpha = 0f;
        UpdateCanvas(false);
    }
    
    public void OnLifeChange(bool isAlive)
    {
        var canvases = GetComponentsInChildren<Canvas>();
        foreach(var canvas in canvases)
        {
            canvas.enabled = isAlive;
        }
    }

    public void OnChangeNPCType(NPCType before, NPCType after)
    {
        SetHealthBarColor();
    }
    void SetHealthBarColor()
    {
        if (healthBar == null) { return; }

        var npcProperty = World.active.NPCPRoperty(entityInfo.npcType);

        healthBar.color = npcProperty.color;

    }
    void HandleHideMana()
    {

        if(entityInfo && entityInfo.npcType == NPCType.Hostile)
        {
            if(resourceBar)
            {
                resourceBar.transform.parent.gameObject.SetActive(false);
            }
        }
    }
    public void OnHealthChange(float health, float shield, float maxHealth)
    {
        UpdateHealthBar();

        void UpdateHealthBar()
        {
            if (healthBar == null || shieldBar == null) { return; }

            healthBar.fillAmount = entityInfo.health / (entityInfo.maxHealth + entityInfo.shield);

            if (entityInfo.health / entityInfo.maxHealth > .90f &&
                entityInfo.health + entityInfo.shield > entityInfo.maxHealth)
            {
                healthBar.fillAmount = .90f;
            }
            shieldBar.fillAmount = (entityInfo.health + entityInfo.shield) / entityInfo.maxHealth;
        }
    }
    public void OnManaChange(float mana, float maxMana)
    {
        UpdateResourceBar();

        void UpdateResourceBar()
        {
            if (resourceBar == null) { return; }

            resourceBar.fillAmount = entityInfo.mana / entityInfo.maxMana;

        }
    }
    //void UpdateBars()
    //{
    //    UpdateCastBar();

    //    void UpdateCastBar()
    //    {
    //        if (!castBarActive) return;
    //        if (castBar == null) { return; }

    //        castBar.fillAmount = currentAbility.castTimer / currentAbility.castTime;
    //    }
    //}
    public void UpdateCastBar()
    {
        ArchUI.SetCanvas(castBarCanvas, castBarActive);
    }



    public void OnHealingTaken(HealthEvent eventData)
    {
        UpdateCanvasTimer(2.5f);
    }

    public void OnNewBuff(BuffInfo newBuff, EntityInfo source)
    {

        UpdateCanvasTimer(3f);
        

    }

    public async void OnAbilityStart(AbilityInfo ability)
    {
        UpdateCanvasTimer(3f);

        if(!ability.vfx.showCastBar) { return; }

        currentAbility = ability;
        castBar.fillAmount = ability.progress;
        castBarActive = true;

        UpdateCastBar();

        var intervals = 0f;

        await ability.EndActivation((AbilityInfo activatedAbility) => {
            castBar.fillAmount = activatedAbility.progress;

            if(intervals > 0)
            {
                intervals -= Time.deltaTime;
                return;

            }

            intervals = 2f;
            UpdateCanvasTimer(3f);
        });


        castBarActive = false;
        UpdateCastBar();

        
    }
}

[Serializable]
public struct TaskProgressBarHandler
{
    private EntityInfo entityInfo;
    private Image progressBar;
    public ProgressBarsBehavior behavior;

    float timeBetween;

    public void Initialize(ProgressBarsBehavior behavior)
    {
        entityInfo = behavior.entityInfo;
        progressBar = behavior.castBar;
        this.behavior = behavior; 

        entityInfo.taskEvents.OnStartTask += OnStartTask;
        entityInfo.taskEvents.WhileWorkingOnTask += WhileWorkingOnTask;
        entityInfo.taskEvents.OnEndTask += OnEndTask;
        entityInfo.taskEvents.OnTaskComplete += OnTaskComplete;
    }

    void OnStartTask(TaskEventData eventData)
    {
        behavior.UpdateCanvasTimer(3f);
        var prop = eventData.task.properties;

        progressBar.fillAmount = prop.workDone / prop.workAmount;

        progressBar.transform.parent.gameObject.SetActive(true);

        behavior.castBarActive = true;
        behavior.UpdateCastBar();


    }

    void WhileWorkingOnTask(TaskEventData eventData)
    {
        var prop = eventData.task.properties;

        progressBar.fillAmount = prop.workDone / prop.workAmount;

        if(timeBetween > 0)
        {
            timeBetween -= Time.deltaTime;
            return;
        }

        timeBetween = 1f;

        behavior.UpdateCanvasTimer(2f);
        
    }

    void OnTaskComplete(TaskEventData eventData)
    {
        behavior.castBarActive = false;
        behavior.UpdateCastBar();
    }

    void OnEndTask(TaskEventData eventData)
    {

        behavior.castBarActive = false;
        behavior.UpdateCastBar();

        
    }

}