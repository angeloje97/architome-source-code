using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using Architome;
using UnityEngine.UI;

[Serializable]
public class BuffProperties
{
    public float value;
    public float valueContributionToBuff = 1;
    public float intervals;
    public float time;
    public float radius;


    [Header("Stacks")]
    public bool canStack;
    public bool loseStackAndResetTimer;
    public int stacksPerApplication;
    public int maxStacks;
    public float valueStackMultiplier;

    public bool selfBuffOnDestroy;
    public bool reapplyResetsTimer;
    public bool reapplyResetsBuff;
    public bool outOfCombatCleanse;
}

[RequireComponent(typeof(BuffFXHandler))]
public class BuffInfo : MonoBehaviour
{

    [SerializeField] int id;
    public int _id
    {
        get
        {
            return id != 0 ?  id : 9999999;
        }
        private set
        {
            id = value;
        }
    }
    bool idSet;


    public new string name;
    [Multiline]
    public string description;

    public BuffsManager buffsManager;

    [Header("Source")]
    public GameObject sourceObject;
    public EntityInfo sourceInfo;
    public AbilityInfo sourceAbility;
    public CatalystInfo sourceCatalyst;

    [Header("Host")]
    public GameObject hostObject;
    public EntityInfo hostInfo;

    [Header("Target")]
    public GameObject targetObject;
    public EntityInfo targetInfo;

    public BuffTargetType buffTargetType;
    public Sprite buffIcon;


    public float expireDelay = .125f;

    public BuffProperties properties;

    [Header("Buff Properties")]
    public DamageType damageType;
    public int stacks = 0;
    bool buffTimeComplete = false;
    public float buffTimer;
    public float progress;

    public bool failed { get; set; }

    public bool IsComplete { get { return buffTimeComplete; } }

    [Serializable]
    public struct BuffFX
    {
        [Serializable]
        public class EffectData
        {
            [Header("Behavior")]
            public BuffEvents playTrigger;

            [Header("Particles")]
            public GameObject particle;
            public BodyPart bodyPart;
            public CatalystParticleTarget target;
            public RadiusType radiusType;
            
            public bool playForDuration;
            public Vector3 offset, offsetScale, offsetRotation, radiusPortion;

            [Header("Audio")]
            public AudioClip audioClip;
            public bool loops;
        }

        public List<EffectData> effectsData;

        [Header("Transform Effects")]
        public bool shrinkOnEnd;
    }

    public BuffFX effects;

    //Events
    public Action<BuffInfo> OnAcquireVessel;
    public Action<BuffInfo> OnBuffStart;
    public Action<BuffInfo> OnBuffCompletion;
    public Action<BuffInfo> OnBuffCleanse;
    public Action<BuffInfo> OnBuffInterval;
    public Action<BuffInfo> OnBuffDeplete;
    public Action<BuffInfo> OnBuffEnd;
    public Action<CombatEventData> OnBuffDamage;
    public Action<CombatEventData> OnBuffHeal;
    public Action<BuffInfo, int, float> OnStack;
    public Action<BuffInfo, float, float> OnChangeValue;


    public static List<BuffInfo> buffs;

    public void SetId(int id, bool forceChange = false)
    {
        if (idSet && !forceChange) return;
        
        _id = id;
        idSet = true;
    }

    public void GetVessel()
    {
        if(GetComponentInParent<BuffsManager>())
        {
            buffsManager = GetComponentInParent<BuffsManager>();
            buffsManager.OnBuffStack += OnBuffStack;
            buffsManager.OnBuffTimerReset += OnBuffTimerReset;
            buffsManager.OnResetBuff += OnResetBuff;
        }
        if (GetComponentInParent<EntityInfo>())
        {
            hostInfo = GetComponentInParent<EntityInfo>();
            hostObject = hostInfo.gameObject;
            hostInfo.OnNewBuff?.Invoke(this, sourceInfo);

        }   
    }

    void UpdateValues()
    {
        if (sourceAbility == null) return;

        properties.value = sourceAbility.value * properties.valueContributionToBuff;
    }

    private void Awake()
    {
        buffTimeComplete = false;
        buffTimer = properties.time;
        progress = 1;
        GetVessel();
        UpdateValues();
        //Invoke("SpawnParticle", .125f);
        StartCoroutine(BuffIntervalHandler());
    }

    public void Start()
    {
        ArchAction.Delay(() => { if(!failed) OnBuffStart?.Invoke(this); }, .0625f);
    }
    private void Update()
    {
        HandleTimer();
    }


    public string Description()
    {
        var result = "";

        if (description != null && description.Length > 0)
        {
            result += $"{description}\n";
        }

        return result;
    }

    public string PropertiesDescription()
    {
        if (!properties.canStack) return "";

        var result = "";

        result += $"Can stack to a maximum of {properties.maxStacks} times.";

        if (properties.loseStackAndResetTimer)
        {
            result += " Stacks fall off over time.";
        }

        result += "\n";

        return result;
    }

    public string TypesDescription()
    {
        var result = "";

        foreach (var buffType in GetComponents<BuffType>())
        {
            result += $"{buffType.BuffTypeDescription()}";
        }

        return result;
    }

    public Sprite Icon()
    {
        if (buffIcon != null)
        {
            return buffIcon;
        }

        if (sourceAbility)
        {
            return sourceAbility.Icon();
        }

        return buffIcon;
    }
    public void OnBuffStack(BuffsManager.BuffData data)
    {
        var buff = data.buffInfo;
        if(buff.id != id) { return; }
        if(stacks == properties.maxStacks) { return; }
        if (!properties.canStack) { return; }


        sourceAbility = buff.sourceAbility;

        ChangeStack(properties.stacksPerApplication);
    }

    public void ChangeStack(int amount)
    {
        var newStacks = stacks + amount;

        if (newStacks > properties.maxStacks)
        {
            newStacks = properties.maxStacks;
        }
        if (newStacks < 0)
        {
            newStacks = 0;
        }

        var stackDifference = newStacks - stacks;

        var valueIncrease = (sourceAbility.value * properties.valueContributionToBuff) * properties.valueStackMultiplier * stackDifference;
        properties.value += valueIncrease;

        stacks = newStacks;

        OnStack?.Invoke(this, stacks, valueIncrease);

    }

    public void OnResetBuff(BuffsManager.BuffData data)
    {
        var buff = data.buffInfo;
        if(buff != this) { return; }
        Cleanse();
    }

    public void OnBuffTimerReset(BuffsManager.BuffData data)
    {
        var buff = data.buffInfo;
        if (buff.id != id) return;

        buffTimer = properties.time;
    }

    public void ChangeValue(float newValue)
    {
        OnChangeValue?.Invoke(this, properties.value, newValue);
        properties.value = newValue;
    }
    public void HandleTimer()
    {
        HandleBuffTimer();

        void HandleBuffTimer()
        {
            if (buffTimeComplete == true) { return; }
            if (buffTimer == -1) return;

            if (buffTimer > 0)
            {
                buffTimer -= Time.deltaTime;
                progress = buffTimer / properties.time;
            }
            if (buffTimer <= 0)
            {
                if (properties.canStack && properties.loseStackAndResetTimer && stacks > 1)
                {
                    buffTimer = properties.time;
                    ChangeStack(-1);
                    return;
                }
                buffTimer = 0;
                buffTimeComplete = true;
                OnBuffCompletion?.Invoke(this);
                StartCoroutine(Expire());
            }
        }
    }
    public void Deplete()
    {
        OnBuffDeplete?.Invoke(this);
        StartCoroutine(Expire());
    }
    public void Cleanse()
    {
        OnBuffCleanse?.Invoke(this);
        StartCoroutine(Expire());
    }
    public void CompleteEarly()
    {
        stacks = 0;
        buffTimer = 0;
    }
    public IEnumerator Expire()
    {
        OnBuffEnd?.Invoke(this);
        buffTimeComplete = true;

        if(transform.parent && GetComponentInParent<BuffsManager>())
        {
            if(GetComponentInParent<BuffsManager>().buffObjects.Contains(gameObject))
            {
                GetComponentInParent<BuffsManager>().buffObjects.Remove(gameObject);
            }
        }

        if(buffsManager)
        {
            buffsManager.OnBuffStack -= OnBuffStack;
            buffsManager.OnBuffTimerReset -= OnBuffTimerReset;
            buffsManager.OnResetBuff -= OnResetBuff;
        }

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
    IEnumerator BuffIntervalHandler()
    {
        
        while(true)
        {
            if(sourceAbility == null) 
            { 
                yield return new WaitForSeconds(.125f);
                continue;
            }

            if (properties.intervals == 0)
            {
                break;
            }

            yield return new WaitForSeconds(properties.intervals);
            OnBuffInterval?.Invoke(this);

        }
    }
    public List<EntityInfo> EnemiesWithinRange()
    {

        var entitiesWithinRange = Entity.EntitiesWithinRange(hostObject.transform.position, properties.radius);


        for(int i = 0; i < entitiesWithinRange.Count; i++)
        {
            if(sourceInfo.CanAttack(entitiesWithinRange[i].gameObject))
            {
                continue;
            }
            else
            {
                entitiesWithinRange.RemoveAt(i);
                i--;
            }
        }

        return entitiesWithinRange;
    }
    public List<EntityInfo> AlliesWithinRange()
    {
        var allyList = Entity.EntitiesWithinRange(hostObject.transform.position, properties.radius);

        for (int i = 0; i < allyList.Count; i++)
        {
            if(sourceInfo.CanHelp(allyList[i].gameObject))
            {
                continue;
            }
            else
            {
                allyList.RemoveAt(i);
                i--;
            }
        }


        return allyList;
    }
    public void HandleTargetHealth(EntityInfo target, float val, BuffTargetType targettingType)
    {
        var combatData = new CombatEventData(this, sourceInfo, val);
        combatData.target = target;

        HandleDamage();
        HandleHealing();

        void HandleDamage()
        {
            if(targettingType == BuffTargetType.Assist) { return; }
            target.Damage(combatData);
            OnBuffDamage?.Invoke(combatData);
        }

        void HandleHealing()
        {
            if(targettingType == BuffTargetType.Harm) { return; }
            target.Heal(combatData);
            OnBuffHeal?.Invoke(combatData);
        }
    }

}
