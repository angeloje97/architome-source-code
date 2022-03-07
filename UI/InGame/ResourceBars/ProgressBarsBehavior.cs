using Architome.Enums;
using UnityEngine;
using UnityEngine.UI;
using Architome;
public class ProgressBarsBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public GraphicsInfo graphicsInfo;
    public EntityClusterAgent clusterAgent;

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
    public Image castBar;


    //Original Properties
    public float originalHealthBarHeight;
    public float originalResourceBarHeight;
    public float originalCastBarHeight;
    public float clusterAgentOffset;
    public Vector3 localPosition;


    void GetDependencies()
    {
        if (GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            SetHealthBarColor();
            entityInfo.OnHealthChange += OnHealthChange;
            entityInfo.OnManaChange += OnManaChange;
            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnChangeNPCType += OnChangeNPCType;

        }

        if(GetComponentInParent<GraphicsInfo>())
        {
            graphicsInfo = GetComponentInParent<GraphicsInfo>();
            if(graphicsInfo.EntityClusterAgent())
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

    }
    public void OnValidate()
    {
        localPosition = transform.localPosition;
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
        switch (entityInfo.npcType)
        {
            case NPCType.Hostile:
                healthBar.color = Color.red;
                break;
            case NPCType.Friendly:
                healthBar.color = Color.green;
                break;
            case NPCType.Neutral:
                healthBar.color = Color.yellow;
                break;
            case NPCType.Untargetable:
                healthBar.color = Color.grey;
                break;
        }
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
    void Start()
    {
        GetDependencies();
        HandleHideMana();
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
    void Update()
    {
        if (entityInfo == null) { return; }
        UpdateBars();
    }
    void UpdateBars()
    {
        UpdateCastBar();

        void UpdateCastBar()
        {
            if (castBar == null) { return; }
            if (!entityInfo.AbilityManager()) { return; }
            var currentAbility = entityInfo.AbilityManager().currentlyCasting;


            if (castBar.transform.parent.gameObject.activeSelf != entityInfo.AbilityManager().currentlyCasting)
            {
                if (currentAbility && currentAbility.isAttack) { return; }
                castBar.transform.parent.gameObject.SetActive(entityInfo.AbilityManager().currentlyCasting);
            }

            if (currentAbility == null) { return; }

            castBar.fillAmount = currentAbility.castTimer / currentAbility.castTime;
        }
    }
}
