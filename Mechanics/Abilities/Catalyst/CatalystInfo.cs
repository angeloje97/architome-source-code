using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using UnityEngine.UI;
using System;
using System.Linq;


namespace Architome
{

}
[RequireComponent(typeof(CatalystDeathCondition))]
[RequireComponent(typeof(CatalystHit))]
public class CatalystInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public Sprite catalystIcon;

    [Header("Catalyst Animations")]
    public List<int> animationSequence;
    public Vector2 catalystStyle;
    [Range(0, 1)]
    public float releasePercent;
    

    public bool isCataling;
    public AbilityType catalyingAbilityType;

    public EntityInfo entityInfo;
    public AbilityInfo abilityInfo;

    private int ticks;
    public float currentRange;
    public float liveTime;
    public float distanceFromTarget;
    
    public List<EntityInfo> enemiesHit;
    public List<EntityInfo> alliesHealed;
    public List<EntityInfo> alliesAssisted;

    [Header("AbilityInfo Properties")]
    public float value;
    public bool requiresLockOnTarget;
    public GameObject target;
    public Vector3 direction;
    public Vector3 location;

    [Header("Catalyst Set Values")]
    public DamageType damageType;
    public float range;
    public float castTime;
    public float speed;
    public CatalystType catalystType;

    [Serializable]
    public class CatalystEffects
    {
        public List<GameObject> particleHarms;
        public List<GameObject> particleHeals;
        public List<AudioClip> castingSounds;
        public List<AudioClip> castReleaseSounds;
        public List<AudioClip> harmSounds;
        public List<AudioClip> assistSounds;
        public List<AudioClip> healSounds;
    }

    public CatalystEffects effects;
    public AudioSource audioSource;
    public Vector3 startPosition;
    public bool isDestroyed;

    //Events
    public Action<GameObject> OnDamage;
    public Action<GameObject> OnAssist;
    public Action<GameObject> OnHeal;
    public Action<GameObject> OnHit;
    public Action<GameObject> OnWrongTarget;
    public Action<GameObject> OnDeadTarget;
    public Action<GameObject, GameObject> OnNewTarget;
    public Action<CatalystInfo> OnReturn;
    public Action<CatalystInfo> OnCantFindEntity;
    public Action<CatalystInfo, int> OnTickChange;
    public Action<CatalystInfo, GameObject> OnCloseToTarget;
    public Action<CatalystInfo, GameObject> OnWrongTargetHit;
    public Action<GameObject, bool> OnPhysicsInteraction;

    public Action<CatalystInfo, CatalystInfo> OnCatalingRelease;
    public Action<CatalystDeathCondition> OnCatalystDestroy;

    private GameObject targetCheck;
    public void GetDependencies()
    {
        if(abilityInfo)
        {

            if(abilityInfo.entityObject)
            {
                entityObject = abilityInfo.entityObject;
                entityInfo = abilityInfo.entityInfo;
            }

            GetMetrics();
        }

        if(GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }

        void GetMetrics()
        {
            direction = abilityInfo.directionLocked;
            location = abilityInfo.locationLocked;
            if(abilityInfo.targetLocked)
            {
                target = abilityInfo.targetLocked;
            }
            if (!abilityInfo.nullifyDamage)
            {
                value = abilityInfo.value;
            }
            else
            {
                value = 0;
            }

        }
    }
    public void HandleComponents()
    {
        if(abilityInfo)
        {
            HandleCatalyst();
            HandleCatalyng();
        }

        void HandleCatalyst()
        {
            if (isCataling) return;
            if (abilityInfo.abilityType == AbilityType.SkillShot)
            {
                gameObject.AddComponent<CatalystFreeFly>();
                gameObject.GetComponent<CatalystFreeFly>().abilityInfo = abilityInfo;
                gameObject.GetComponent<CatalystFreeFly>().catalystInfo = gameObject.GetComponent<CatalystInfo>();
            }

            if (abilityInfo.abilityType == AbilityType.LockOn)
            {
                gameObject.AddComponent<CatalystLockOn>();
                gameObject.GetComponent<CatalystLockOn>().abilityInfo = abilityInfo;
                gameObject.GetComponent<CatalystLockOn>().catalystInfo = gameObject.GetComponent<CatalystInfo>();
            }

            if(abilityInfo.abilityType == AbilityType.Use)
            {
                var catalystUse =  gameObject.AddComponent<CatalystUse>();
                catalystUse.abilityInfo = abilityInfo;
                catalystUse.catalystInfo = this;
            }

            if (abilityInfo.bounces)
            {
                gameObject.AddComponent<CatalystBounce>();
            }

            if (abilityInfo.returns)
            {
                gameObject.AddComponent<CatalystReturn>();

            }

            if (abilityInfo.catalystStops)
            {
                gameObject.AddComponent<CatalystStop>();
            }

            if(abilityInfo.cataling)
            {
                gameObject.AddComponent<CatalingActions>();
            }

            if(abilityInfo.buffProperties.selfBuffOnDestroy)
            {
                gameObject.AddComponent<CatalystBuffOnDestroy>();
            }

            requiresLockOnTarget = abilityInfo.requiresLockOnTarget;
            ticks = abilityInfo.ticksOfDamage;
        }

        void HandleCatalyng()
        {
            if(!isCataling) { return; }

            if (abilityInfo.catalingAbilityType == AbilityType.LockOn)
            {
                gameObject.AddComponent<CatalystLockOn>();
            }
            else if (abilityInfo.catalingAbilityType == AbilityType.SkillShot)
            {
                gameObject.AddComponent<CatalystFreeFly>();
            }
            value = abilityInfo.value * abilityInfo.valueContributionToCataling;
            
            ticks = 1;
        }
    }
    void Awake()
    {
        startPosition = transform.position;
        GetDependencies();
        HandleComponents();
    }
    private void Start()
    {
        SpawnCatalystAudio();
    }

    public void OnTriggerEnter(Collider other)
    {
        OnPhysicsInteraction?.Invoke(other.gameObject, true);
    }

    public void OnTriggerExit(Collider other)
    {
        OnPhysicsInteraction?.Invoke(other.gameObject, false);
    }

    void SpawnCatalystAudio()
    {
        new GameObject().AddComponent<CatalystAudio>().Activate(this);
    }
    void Update()
    {
        UpdateMetrics();
        HandleEvents();
    }
    public void UpdateMetrics()
    {
        currentRange = V3Helper.Distance(startPosition, transform.position);
        liveTime += Time.deltaTime;

        if(target)
        {
            distanceFromTarget = V3Helper.Distance(target.transform.position, transform.position);
        }
    }
    public void HandleEvents()
    {
        if(targetCheck != target)
        {
            OnNewTarget.Invoke(targetCheck, target);
            targetCheck = target;
        }
    }
    public void HandleDeadTarget()
    {
        if(!abilityInfo.requiresLockOnTarget) { return; }
        if (target == null) return;

        var targetInfo = target.GetComponent<EntityInfo>();

        if(targetInfo.isAlive) { return; }

        if(abilityInfo.bounces)
        {
            GetComponent<CatalystBounce>().LookForNewTarget();
        }
    }
    public List<GameObject> EntitiesWithinRadius(float radius = 0f)
    {
        if (radius == 0) radius = range;


        List<GameObject> entities = new List<GameObject>();

        var structureLayer = GMHelper.LayerMasks().structureLayerMask;
        var entityLayer = GMHelper.LayerMasks().entityLayerMask;

        Collider[] collisions = Physics.OverlapSphere(transform.position, radius, entityLayer)
                                        .OrderBy(entity => V3Helper.Distance(entity.transform.position, transform.position))
                                        .ToArray();

        foreach(var i in collisions)
        {
            var direction = V3Helper.Direction(i.transform.position, transform.position);
            var distance = V3Helper.Distance(i.transform.position, transform.position);


            if(abilityInfo.requiresLineOfSight)
            {
                if (!Physics.Raycast(transform.position, direction, distance, structureLayer))
                {
                    entities.Add(i.gameObject);
                }
            }
            else
            {
                entities.Add(i.gameObject);
            }
            
        }

        return entities;
    }
    public List<GameObject> AlliesWithinRange()
    {
        var allies = new List<GameObject>();

        foreach(GameObject ally in EntitiesWithinRadius())
        {
            if(abilityInfo.entityInfo.CanHelp(ally))
            {
                allies.Add(ally);
            }
        }

        return allies;
    }
    public List<GameObject> EnemiesWithinRange()
    {
        var enemies = new List<GameObject>();

        foreach(var i in EntitiesWithinRadius())
        {
            if(abilityInfo.entityInfo.CanAttack(i))
            {
                enemies.Add(i);
            }
        }

        return enemies;
    }

    public void ReduceTicks()
    {
        ticks--;
        OnTickChange?.Invoke(this, ticks);
    }
    public void IncreaseTicks(bool triggerEvent = true)
    {
        ticks++;
        if (triggerEvent)
        {
            OnTickChange?.Invoke(this, ticks);
        }
    }

    public int Ticks()
    {
        return ticks;
    }
}
