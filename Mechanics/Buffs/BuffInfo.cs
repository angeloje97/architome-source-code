using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Architome.Enums;
using System;
using Architome;
using UnityEngine.UI;

[Serializable]
public class BuffProperties
{
    public float value = 1;
    public float valueContributionToBuff = 1;
    public float intervals;
    public float time;
    public float radius;


    [Header("Stacks")]
    public bool canStack;
    public bool loseStackAndResetTimer;
    public bool infiniteTime;
    public int stacksPerApplication;
    public int maxStacks;
    public float valueStackMultiplier;

    //public bool selfBuffOnDestroy;
    public bool reapplyResetsTimer;
    public bool reapplyResetsBuff;
    
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
    public Item sourceItem;

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
    public CleanseConditions cleanseConditions;
    public UISettings settings;

    [Serializable]
    public struct CleanseConditions {
        public bool enterCombat, exitCombat, damageTaken, isMoving;
    }

    [SerializeField]
    public struct UISettings
    {
        public bool hideBuff;
    }

    [Header("Buff Properties")]
    public DamageType damageType;
    public int stacks = 0;
    public bool buffTimeComplete { get; private set; }
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
            [Header("Debugging")]
            public int effectPlayed = 0;
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

    LayerMask structureLayer;
    LayerMask entityLayer;


    public static List<BuffInfo> buffs;

    bool powerSet;
    private void Awake()
    {
        GetDependencies();
    }

    public void Start()
    {
        ArchAction.Yield(() => { 
            if(!failed) OnBuffStart?.Invoke(this);
            if (properties.time <= 0) Expire();
                
        });
    }
    private void Update()
    {
        if (properties.time <= 0) return;
        if (properties.infiniteTime) return;
        HandleTimer();
    }

    public void SetId(int id, bool forceChange = false)
    {
        if (idSet && !forceChange) return;
        
        _id = id;
        idSet = true;
    }

    public void GetVessel()
    {
        buffsManager = GetComponentInParent<BuffsManager>();
        hostInfo = GetComponentInParent<EntityInfo>();
        if(buffsManager)
        {
            buffsManager.OnBuffStack += OnBuffStack;
            buffsManager.OnBuffTimerReset += OnBuffTimerReset;
            buffsManager.OnResetBuff += OnResetBuff;
        }
        if (hostInfo)
        {
            hostObject = hostInfo.gameObject;
            hostInfo.OnNewBuff?.Invoke(this, sourceInfo);
        }   
    }

    void UpdateValues()
    {
        if (powerSet) return;

        FromAbility();
        FromItem();

        void FromAbility()
        {
            if (sourceAbility == null) return;

            properties.value = sourceAbility.value * properties.valueContributionToBuff;

        }

        //void FromConsumable()
        //{
        //    if (sourceItem == null) return;
        //    if (sourceItem.GetType() != typeof(Consumable)) return;

        //    var consumable = (Consumable)sourceItem;

        //    properties.value = consumable.value * properties.valueContributionToBuff;
        //}

        void FromItem()
        {
            if (sourceItem == null) return;
            
            if (sourceItem.GetType() == typeof(Consumable))
            {
                var consumable = (Consumable)sourceItem;
                properties.value = consumable.value * properties.valueContributionToBuff;

            }

            if (Item.Equipable(sourceItem))
            {
                var equipment = (Equipment)sourceItem;
                properties.value = equipment.itemLevel * properties.valueContributionToBuff;
            }
        }
    }

    public void SetPower(float basePower, bool ignoreValueContribution = false)
    {
        powerSet = true;
        
        properties.value = properties.valueContributionToBuff * basePower;

        if (ignoreValueContribution)
        {
            properties.value = basePower;
        }
    }


    void GetDependencies()
    {
        buffTimeComplete = false;
        buffTimer = properties.time;
        progress = 1;
        buffIcon = Icon();
        GetVessel();
        UpdateValues();
        StartCoroutine(BuffIntervalHandler());

        var layerMasksData = LayerMasksData.active;

        if (layerMasksData)
        {
            entityLayer = layerMasksData.entityLayerMask;
            structureLayer = layerMasksData.structureLayerMask;
        }
    }
    public void ApplyBaseValue(float value)
    {
        properties.value = value * properties.valueContributionToBuff;
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
        var result = "";


        if (properties.canStack)
        {
            result += $"Can stack to a maximum of {properties.maxStacks} times.";

            if (properties.loseStackAndResetTimer)
            {
                result += " Stacks fall off over time.";
            }

            result += "\n";
        }

        var conditions = new List<string>();

        foreach (var field in cleanseConditions.GetType().GetFields())
        {
            var condition = (bool)field.GetValue(cleanseConditions);
            if (!condition) continue;

            conditions.Add(ArchString.CamelToTitle(field.Name));
        }
        

        if (conditions.Count > 0)
        {
            result += $"Buff ends from: {ArchString.StringList(conditions)}\n";
        }

        return result;
    }

    public string TypesDescription()
    {

        var descriptions = new List<string>();

        foreach (var buffType in GetComponents<BuffType>())
        {
            //result += $"{buffType.Description()}";
            descriptions.Add(buffType.Description());
        }

        return ArchString.NextLineList(descriptions);

    }

    public string TypesDescriptionGeneral()
    {

        var descriptions = new List<string>();

        foreach (var buffType in GetComponents<BuffType>())
        {
            //result += $"{buffType.Description()}";
            descriptions.Add(buffType.GeneralDescription());
        }

        return ArchString.NextLineList(descriptions);

    }

    public string TypeDescriptionFace(float theoreticalValue)
    {

        var projectedValue = properties.valueContributionToBuff * theoreticalValue;

        var description = new List<string>() {
            Description()
        };

        foreach (var buffType in GetComponents<BuffType>())
        {

            description.Add(buffType.FaceDescription(projectedValue));
        }

        return ArchString.NextLineList(description);
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

        if (sourceItem)
        {
            return sourceItem.itemIcon;
        }

        return buffIcon;
    }
    public void OnBuffStack(BuffsManager.BuffData data)
    {
        var buff = data.buffInfo;
        if(buff.id != id) { return; }
        if(stacks == properties.maxStacks) { return; }
        if (!properties.canStack) { return; }


        sourceAbility = data.sourceAbility;

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
        if(buff._id != _id) { return; }
        Cleanse();
    }

    public void OnBuffTimerReset(BuffsManager.BuffData data)
    {
        var buff = data.buffInfo;
        if (buff.id != id) return;

        sourceAbility = data.sourceAbility;

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
                OnBuffCompletion?.Invoke(this);
                Expire();
            }
        }
    }
    public void Deplete()
    {
        OnBuffDeplete?.Invoke(this);
        Expire();
    }
    public void Cleanse(string reason = "")
    {
        OnBuffCleanse?.Invoke(this);
        Expire();
    }
    public void CompleteEarly()
    {
        stacks = 0;
        buffTimer = 0;
    }
    public async void Expire()
    {
        if (buffTimeComplete) return;
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

        await Task.Delay(1000);
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
    public void AddEventAction(BuffEvents trigger, Action action)
    {
        Action<CombatEventData> combatAction = (CombatEventData eventData) => {
            action();
        };

        if (buffTimeComplete && trigger != BuffEvents.OnEnd) return;

        switch (trigger)
        {
            case BuffEvents.OnInterval:
                OnBuffInterval += (BuffInfo buff) => { action(); };
                break;
            case BuffEvents.OnCleanse:
                OnBuffCleanse += (BuffInfo buff) => { action(); };
                break;
            case BuffEvents.OnComplete:
                OnBuffCompletion += (BuffInfo buff) => { action(); };
                break;
            case BuffEvents.OnDamageTaken:
                hostInfo.OnDamageTaken += combatAction;
                OnBuffEnd += (BuffInfo buff) => { hostInfo.OnDamageTaken -= combatAction; };
                break;
            case BuffEvents.OnDamageImmune:
                hostInfo.combatEvents.OnImmuneDamage += combatAction;
                OnBuffEnd += (BuffInfo buff) => { hostInfo.combatEvents.OnImmuneDamage -= combatAction; };
                break;
            default:
                action();
                break;
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

    public List<EntityInfo> EntitiesWithinRange(bool requiresLOS, Predicate<EntityInfo> valid)
    {
        var entityList = new List<EntityInfo>();

        Entity.ProcessEntitiesInRange(transform.position, properties.radius, entityLayer, (EntityInfo entity) => {

            if (requiresLOS)
            {
                if (V3Helper.IsObstructed(entity.transform.position, transform.position, structureLayer)) return;
            }

            if (!valid(entity)) return;

            entityList.Add(entity);
        });

        return entityList;
    }
    public void ProcessEntitiesInRange(Action<EntityInfo> processes)
    {

        var entities = Physics.OverlapSphere(hostInfo.transform.position, properties.radius, entityLayer);

        foreach (var entity in entities)
        {
            var info = entity.GetComponent<EntityInfo>();
            if (info == null) continue;

            processes(info);
        }
    }

    public void ProcessEntitiesLineOfSight(Action<EntityInfo> process)
    {
        var entities = Physics.OverlapSphere(transform.position, properties.radius, entityLayer);

        foreach (var entity in entities)
        {
            var info = entity.GetComponent<EntityInfo>();
            if (info == null) continue;

            var direction = V3Helper.Direction(entity.transform.position, transform.position);
            var distance = V3Helper.Distance(entity.transform.position, transform.position);
            var ray = new Ray(transform.position, direction);

            if (Physics.Raycast(ray, distance, structureLayer)) continue;

            process(info);
        }
    }
    public void HandleTargetHealth(EntityInfo target, float val, BuffTargetType targettingType)
    {
        var combatData = new CombatEventData(this, val);
        combatData.target = target;

        HandleNeutral();
        HandleDamage();
        HandleHealing();
        
        void HandleNeutral()
        {
            if (targettingType != BuffTargetType.Neutral) return;

            if (val >= 0)
            {
                targettingType = BuffTargetType.Assist;
            }
            else
            {
                targettingType = BuffTargetType.Harm;
                val = -val;
            }

        }

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
