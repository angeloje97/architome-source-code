using Architome.Enums;
using UnityEngine;
using UnityEngine.UI;
using Architome;
using System;
using System.Threading.Tasks;
public class ProgressBarsBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
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

    void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();
        graphicsInfo = GetComponentInParent<GraphicsInfo>();

        if (entityInfo)
        {
            SetHealthBarColor();
            entityInfo.OnHealthChange += OnHealthChange;
            entityInfo.OnManaChange += OnManaChange;
            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnChangeNPCType += OnChangeNPCType;
            entityInfo.OnCombatChange += OnCombatChange;
            entityInfo.OnHealingTaken += OnHealingTaken;
            entityInfo.OnNewBuff += OnNewBuff;
            OnCombatChange(entityInfo.isInCombat);


            if(entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();


                abilityManager.OnAbilityStart += OnAbilityStart;
                abilityManager.OnAbilityEnd += OnAbilityEnd;
                abilityManager.WhileCasting += WhileCasting;


            }
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

        if(healthBarRect.gameObject.activeSelf)
        {
            originalHealthBarHeight = healthBarRect.localPosition.y;
            clusterAgentOffset += healthBarRect.rect.height;
        }

        if(resourceBarRect.gameObject.activeSelf)
        {
            originalResourceBarHeight = resourceBarRect.localPosition.y;
            clusterAgentOffset += resourceBarRect.rect.height;
        }

        clusterAgentOffset += castBarRect.rect.height;

        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup && entityInfo)
        {
            canvasGroup.alpha = entityInfo.isInCombat ? 1f : 0f;
        }

    }
    public void OnValidate()
    {
        localPosition = transform.localPosition;
    }
    void Start()
    {
        GetDependencies();
        HandleHideMana();
        UpdateCastBar();
        taskProgressHandler.Initialize(this);

    }


    void Update()
    {
        if (entityInfo == null) { return; }
        UpdateBars();
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

    public void OnCombatChange(bool isInCombat)
    {
        


        var delay = isInCombat ? 0f : 3f;

        ArchAction.Delay(() => {
            UpdateCanvas(entityInfo.isInCombat);
        }, delay);
    }

    async public void UpdateCanvas(bool val)
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
    async void UpdateOutOfCombatCanvas(float timer)
    {
        if (timer > canvasTimer)
        {
            canvasTimer = timer;
        }

        if (entityInfo.isInCombat) return;

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



        if (entityInfo.isInCombat) return;
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
    void UpdateBars()
    {
        UpdateCastBar();

        void UpdateCastBar()
        {
            if (!castBarActive) return;
            if (castBar == null) { return; }

            castBar.fillAmount = currentAbility.castTimer / currentAbility.castTime;
        }
    }
    public void UpdateCastBar()
    {
        castBar.transform.parent.gameObject.SetActive(castBarActive);
    }


    public void OnHealingTaken(CombatEventData eventData)
    {
        UpdateOutOfCombatCanvas(2.5f);
    }

    public void OnNewBuff(BuffInfo newBuff, EntityInfo source)
    {

        UpdateOutOfCombatCanvas(3f);
        

    }

    public void OnAbilityStart(AbilityInfo ability)
    {
        UpdateOutOfCombatCanvas(ability.castTime + 2f);

        if(!ability.vfx.showCastBar) { return; }

        currentAbility = ability;
        castBar.fillAmount = ability.progress;
        castBarActive = true;

        UpdateCastBar();
    }

    public void OnAbilityEnd(AbilityInfo ability)
    {
        canvasTimer = 2f;

        if (!ability.vfx.showCastBar) return;

        castBarActive = false;
        UpdateCastBar();
        
    }


    public void WhileCasting(AbilityInfo ability)
    {
        if (!castBarActive) return;
        if (!ability.vfx.showCastBar) return;
        castBar.fillAmount = ability.progress;
    }

}

[Serializable]
public struct TaskProgressBarHandler
{
    private EntityInfo entityInfo;
    private Image progressBar;
    public ProgressBarsBehavior behavior;

    public void Initialize(ProgressBarsBehavior behavior)
    {
        entityInfo = behavior.entityInfo;
        progressBar = behavior.castBar;
        this.behavior = behavior; 

        entityInfo.taskEvents.OnStartTask += OnStartTask;
        entityInfo.taskEvents.WhileWorkingOnTask += WhileWorkingOnTask;
        entityInfo.taskEvents.OnEndTask += OnEndTask;
    }

    void OnStartTask(TaskEventData eventData)
    {
        if (!entityInfo.isInCombat)
        {
            behavior.UpdateCanvas(true);
        }
        var prop = eventData.task.properties;

        progressBar.fillAmount = prop.workDone / prop.workAmount;

        progressBar.transform.parent.gameObject.SetActive(true);
    }

    void WhileWorkingOnTask(TaskEventData eventData)
    {
        var prop = eventData.task.properties;

        progressBar.fillAmount = prop.workDone / prop.workAmount;
        
    }
    void OnEndTask(TaskEventData eventData)
    {
        if (!entityInfo.isInCombat)
        {
            behavior.UpdateCanvas(false);
        }
        progressBar.transform.parent.gameObject.SetActive(false);
    }

}