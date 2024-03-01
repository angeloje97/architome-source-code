using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
using System.Threading.Tasks;
using System.Linq;
public class ThreatManager : EntityProp
{
    // Start is called before the first frame update
    public GameObject entityObject;

    public AIBehavior behavior;
    public AbilityManager abilityManager;
    public LineOfSight lineOfSight;
    public CombatInfo combatInfo;
    public EntityInfo highestThreat;
    public EntityInfo highestNonTargettingThreat;



    [Serializable]
    public class ThreatInfo
    {
        [HideInInspector] public string name;
        public ThreatManager threatManager; //Owner
        public GameObject sourceObject;     //Owner
        public EntityInfo sourceInfo;       //Owner
        public GameObject threatObject;     //Attacker
        public EntityInfo threatInfo;       //Attacker
        public float threatValue;
        public float timeInCombat;
        public bool isActive;

        public Action<ThreatInfo> OnThreatEnd;

        bool initialized;

        public void IncreaseThreat(float val)
        {
            threatValue += val;
        }
        public ThreatInfo(ThreatManager threatManager, EntityInfo source, EntityInfo threat, float val)
        {
            this.threatManager = threatManager;
            sourceObject = source.gameObject;
            sourceInfo = source;
            threatObject = threat.gameObject;
            threatValue = val;
            threatInfo = threat;
            name = threat.entityName;

            threatManager.OnClearThreats += OnClearThreats;
            //source.OnDeath += OnSourceDeath;
            //threat.OnDeath += OnThreatDeath;

            OnThreatEnd += delegate (ThreatInfo threatInfo)
            {
                threatManager.OnClearThreats -= OnClearThreats;
                //source.OnDeath -= OnSourceDeath;
                //threat.OnDeath -= OnThreatDeath;
            };
        }

        public void Initialize()
        {
            if (initialized) return;
            initialized = true;
            isActive = true;
            CombatTimeRoutine();
        }
        public async void CombatTimeRoutine()
        {
            
            while (isActive)
            {
                if (!Application.isPlaying) return;


                if(!sourceInfo.isAlive)
                {
                    isActive = false;
                    threatManager.ClearThreats();
                };

                if (!threatInfo.isAlive)
                {
                    RemoveThreat();
                }
                
                if (!sourceInfo.CanAttack(threatInfo))
                {
                    RemoveThreat();
                }

                await Task.Delay(1000);
                timeInCombat += 1;
            }
        }
        public void OnThreatDeath(CombatEventData eventData)
        {
            RemoveThreat();
        }
        public void OnClearThreats(ThreatManager threatManager)
        {
            RemoveThreat();
        }
        public void OnSourceDeath(CombatEventData eventData)
        {
            
            threatManager.ClearThreats();
        }
        public void RemoveThreat()
        {
            isActive = false;
            OnThreatEnd?.Invoke(this);
            threatManager.RemoveThreat(threatInfo);
        }
    }

    public List<ThreatInfo> threats;
    public Dictionary<EntityInfo, ThreatInfo> threatDict { get; set; }
    public List<EntityInfo> threatQueue;

    public event Action<ThreatInfo> OnNewThreat;
    public event Action<ThreatInfo, float> OnIncreaseThreat;
    public  Action<ThreatInfo> OnFirstThreat { get; set; }
    public Action<ThreatInfo> OnRemoveThreat;
    public event Action<ThreatManager> OnEmptyThreats;
    public event Action<ThreatManager> OnClearThreats;

    bool sortingThreats;
    bool combatActive;
    public override void GetDependencies()
    {
        entityInfo = GetComponentInParent<EntityInfo>();

        
        if(entityInfo)
        {
            entityObject = entityInfo.gameObject;

            abilityManager = entityInfo.AbilityManager();

            entityInfo.OnKill += OnKill;
            entityInfo.OnDamageDone += OnDamageDone;
            entityInfo.OnDamageTaken += OnDamageTaken;
            entityInfo.OnLifeChange += OnLifeChange;
            entityInfo.OnHealingTaken += OnHealingTaken;
            entityInfo.OnNewBuff += OnNewBuff;
            entityInfo.OnChangeNPCType += OnChangeNpcType;
            entityInfo.OnBuffApply += OnBuffApply;
            entityInfo.OnCombatChange += OnCombatChange;


            combatEvents.AddListenerThreat(eThreatEvent.OnPingThreat, OnPingThreat, this);
        }

        behavior = GetComponentInParent<AIBehavior>();

        if (behavior)
        {
            lineOfSight = behavior.LineOfSight();
            behavior.events.OnSightedEntity += OnSighted;
            
            combatInfo = behavior.GetComponentInChildren<CombatInfo>();
        }

        HandleSummons();

    }

    void HandleSummons()
    {
        ArchAction.Delay(() => {
            if (entityInfo.summon.isSummoned)
            {
                ResetThreats();
            }
        }, .25f);
    }

    public void OnChangeNpcType(NPCType before, NPCType after)
    {
        //HandleMaxThreat();
        ResetThreats();
    }
    public void OnDamageDone(CombatEventData eventData)
    {
        var target = eventData.target;
        if(Threat(target) == null)
        {
            IncreaseThreat(target, 1);
        }
    }
    public void OnDamageTaken(CombatEventData eventData)
    {
        if(eventData.source == null) { return; }
        var val = eventData.value;
        var source = eventData.source;

        combatActive = true;

        var threatVal = val;
        if(threatVal == 0) { threatVal++; }

        
        if(source.role == Role.Tank)
        {
            if(GMHelper.Difficulty() != null)
            {
                threatVal *= ThreatMultiplier(eventData.source);
            }
        }

        threatVal += ExtraThreat();
        
        IncreaseThreat(source, threatVal);
        AlertAllies(source);


        float ExtraThreat()
        {
            if (eventData.ability == null) return 0f;
            

            var threat = eventData.ability.threat;
            if (!threat.enabled) return 0f;

            return threat.additiveThreatMultiplier * eventData.value;
        }

    }
    public float ThreatMultiplier(EntityInfo target)
    {
        if (target == null) return 1f;

        if (target.role == Role.Tank)
        {
            return GMHelper.Difficulty().settings.tankHealthMultiplier;
        }

        if (target.role == Role.Healer)
        {
            return GMHelper.Difficulty().settings.healThreatMultiplier;
        }

        return 1f;
    }
    public void OnBuffApply(BuffInfo appliedBuff, EntityInfo source)
    {

    }
    public void OnNewBuff(BuffInfo newBuff, EntityInfo source)
    {
        if (newBuff.buffTargetType != BuffTargetType.Harm) return;
        if (!entityInfo.CanAttack(source)) return;

        var value = newBuff.properties.value;
        if(value <= 0) { value = 15; }
        IncreaseThreat(source, value);
        AlertAllies(source, .25f);

    }
    public void OnHealingTaken(CombatEventData eventData)
    {
        var source = eventData.source;
        var val = eventData.value;

        var threatMultiplier = ThreatMultiplier(source);

        val *= threatMultiplier;

        TriggerEnemies(source, val);
    }
    public void OnCombatChange(bool isInCombat)
    {
        if (isInCombat) return;
        ClearThreatQueue();
        if (threatQueue.Count == 0) return;


        var target = threatQueue[0];
        threatQueue.RemoveAt(0);
        IncreaseThreat(target, 15f, true);

    }
    public void OnPingThreat(ThreatEvent threatEventData)
    {
        var value = threatEventData.threatValue;
        var source = threatEventData.source;
        IncreaseThreat(source, value);
    }
    public void ClearThreatQueue()
    {
        if (!entityInfo.isAlive)
        {
            threatQueue.Clear();
            return;
        }

        for (int i = 0; i < threatQueue.Count; i++)
        {
            var target = threatQueue[i];

            if (target == null)
            {
                threatQueue.RemoveAt(i);
                i--;
                continue;
            }

            if (!lineOfSight.entitiesWithinLineOfSight.Contains(target))
            {
                threatQueue.RemoveAt(i);
                i--;
                continue;
            }

            if (!target.GetComponent<EntityInfo>().isAlive)
            {
                threatQueue.RemoveAt(i);
                i--;
            }

            
        }

        threatQueue = threatQueue.OrderBy(threat => Vector3.Distance(threat.transform.position, entityObject.transform.position)).ToList();
    }
    public void RemoveThreat(EntityInfo threatInfo)
    {
        var threat = Threat(threatInfo);
        if(threat == null) { return; }

        if(!threats.Contains(threat)) { return; }
        threats.Remove(threat);
        threatDict.Remove(threatInfo);
        if (highestThreat == threatInfo) highestThreat = null;
        if (highestNonTargettingThreat == threatInfo) highestNonTargettingThreat = null;

        HandleMaxThreat();

        OnRemoveThreat?.Invoke(threat);
        UpdateCombatActive();



        if(!ThreatContainsEnemies())
        {
            ClearThreats();
            threatDict = new();
        }

        if(threats.Count == 0)
        {
            OnEmptyThreats?.Invoke(this);
        }
    }
    void UpdateCombatActive()
    {
        foreach (var threat in threats)
        {
            if (threat.threatInfo.isInCombat)
            {
                combatActive = true;
                return;
            }
        }

        combatActive = false;
    }
    public void OnLifeChange(bool isAlive)
    {
        ClearThreats();
    }
    public void OnKill(CombatEventData eventData)
    {
        //var target = eventData.target;

        //RemoveThreat(Threat(target.gameObject));
    }
    public void OnSighted(EntityInfo target)
    {
        if (behavior.combatType != CombatBehaviorType.Aggressive) return;
        if (!entityInfo.CanAttack(target)) return;
        if (Threat(target) != null) return;
        if (!entityInfo.isAlive) return;

        if (entityInfo.isInCombat)
        {
            if (!threatQueue.Contains(target))
            {
                threatQueue.Add(target);
            }
            return;
        }

        IncreaseThreat(target, 15);

        if (entityInfo.rarity != EntityRarity.Player)
        {
            AlertAllies(target);
        }

        behavior.events.OnDetectedEnemy?.Invoke(target);
    }
    public void IncreaseThreat(EntityInfo source, float value, bool fromAlert = false)
    {
        if (!source.isAlive) return;
        if (!entityInfo.isAlive) return;
        if (!entityInfo.CanAttack(source)) return;
        if(entityInfo.gameObject == source) { return; }
        if (entityInfo.npcType == source.npcType) return;
        if (source == null) return;
        bool firstThreat = threats.Count == 0;



        var threatInfo = Threat(source);

        if(threatInfo == null)
        {
            threatInfo = new ThreatInfo(this, entityInfo, source, value);
            threats.Add(threatInfo);
            threatInfo.Initialize();
            OnNewThreat?.Invoke(threatInfo);
            threatDict.Add(source, threatInfo);
        }
        else
        {
            threatInfo.IncreaseThreat(value);
        }

        var threatEvent = new ThreatEvent(threatInfo);

        OnIncreaseThreat?.Invoke(threatInfo, value);
        source.combatEvents.InvokeThreat(eThreatEvent.OnGenerateThreat, threatEvent);

        CheckMaxThreat();
        CheckMaxThreatNonTargetting();
        //HandleMaxThreat();
        

        if (firstThreat && !fromAlert)
        {
            OnFirstThreat?.Invoke(threatInfo);

            if (source.IsPlayer())
            {
                entityInfo.combatEvents.OnFirstThreatWithPlayer?.Invoke(threatInfo);
            }
        }
        

        void CheckMaxThreat()
        {
            if (highestThreat == null)
            {
                highestThreat = source;
                return;
            }

            var highestThreatInfo = Threat(highestThreat);



            if (highestThreatInfo.threatInfo == source) return;
            if (threatInfo.threatValue >= highestThreatInfo.threatValue)
            {
                highestThreat = source;

            }
        }

        void CheckMaxThreatNonTargetting()
        {
            if (source.combatTarget == entityInfo)
            {
                if (highestNonTargettingThreat == source)
                {
                    highestNonTargettingThreat = null;
                }
                return;
            }

            if (highestNonTargettingThreat == null)
            {
                highestNonTargettingThreat = source;
                return;
            }


            var highestThreat = Threat(highestNonTargettingThreat);
            

            if (highestThreat.threatValue > threatInfo.threatValue)
            {
                highestNonTargettingThreat = source;
            }
        }
    }
    async void SortThreats()
    {
        if (sortingThreats) return;
        sortingThreats = true;

        threats = threats.OrderBy(threat => Vector3.Distance(threat.threatInfo.transform.position, entityObject.transform.position)).ToList();

        await Task.Delay(500);

        sortingThreats = false;
    }
    public void Bump()
    {
        foreach (var threat in threats)
        {
            IncreaseThreat(threat.threatInfo, 1);
        }
    }
    public EntityInfo RandomTargetBlackList(List<Role> blackListRole)
    {

        if (BlackList(blackListRole).Count == 0) { return null; }

        else
        {
            return BlackList(blackListRole)[UnityEngine.Random.Range(0, BlackList(blackListRole).Count)];
        }
    }
    public List<EntityInfo> WhiteList(List<Role> whiteListRole)
    {
        var whiteListed = new List<EntityInfo>();

        foreach(var threat in threats)
        {
            if(whiteListRole.Contains(threat.threatInfo.role))
            {
                whiteListed.Add(threat.threatInfo);
            }
        }
        return whiteListed;
    }
    public List<EntityInfo> BlackList(List<Role> blackListRole)
    {
        var blackListed = new List<EntityInfo>();

        foreach(var threat in threats)
        {
            if(!blackListRole.Contains(threat.threatInfo.role))
            {
                blackListed.Add(threat.threatInfo);
            }
        }
        return blackListed;
    }
    public void IncreaseMassiveThreat(EntityInfo entity)
    {
        IncreaseThreat(entity, 5000);
    }
    public bool ContainsRole(Role role)
    {
        foreach(var i in threats)
        {
            if(i.threatObject.GetComponent<EntityInfo>() && i.threatObject.GetComponent<EntityInfo>().role == role)
            {
                return true;
            }
        }

        return false;
    }
    public void HandleMaxThreat()
    {
        float max = float.NegativeInfinity;
        float maxNonTargetting = float.NegativeInfinity;

        highestThreat = null;
        highestNonTargettingThreat = null;

        foreach (var info in threats)
        {

            HandleMax(info);
            HandleNonTargetting(info);
        }

        void HandleMax(ThreatInfo info)
        {
            if (!info.threatInfo.isInCombat && combatActive) return;

            if (info.threatValue > max)
            {
                if (entityInfo.CanAttack(info.threatInfo))
                {
                    highestThreat = info.threatInfo;
                    max = info.threatValue;
                }
            }
        }
        
        void HandleNonTargetting(ThreatInfo info)
        {
            var enemy = info.threatInfo;

            if (enemy.combatTarget == entityInfo) return;
            if (info.threatValue <= maxNonTargetting) return;

            maxNonTargetting = info.threatValue;
            highestNonTargettingThreat = enemy;
        }
    }
    public EntityInfo NearestHighestThreat(float range)
    {

        EntityInfo target = null;
        var furthest = 0f;
        var highestThreat = 0f;
        foreach(var threat in threats)
        {
            var distance = V3Helper.Distance(threat.threatObject.transform.position, entityObject.transform.position);
            if(distance <= range && distance > furthest && threat.threatValue > highestThreat)
            {
                target = threat.threatInfo;

                if (!entityInfo.CanSee(target.transform)) continue;
                furthest = distance;
                highestThreat = threat.threatValue;
            }
        }
        return target;
    }
    async public void AlertAllies(EntityInfo target, float delay = 0f, float val = 0f)
    {
        if (!entityInfo.isAlive) return;
        await Task.Delay((int)(delay * 1000));

        //Debugger.InConsole(4598, $"Delay is {(int)(.25f * 1000)}");

        var allies = lineOfSight.DetectedAllies();


        foreach (var ally in allies)
        {
            if (ally.npcType != entityInfo.npcType) continue;
            if (ally == entityInfo) continue;
            if (ally == null) continue;
            var threatManager = ally.GetComponentInChildren<ThreatManager>();
            if (threatManager == null) continue;
            if (threatManager.Threat(target) != null) continue;

            threatManager.IncreaseThreat(target, val == 0f ? 15f : val, true);
        }
    }
    public void TriggerEnemies(EntityInfo source, float value)
    {
        try 
        {
            var enemies = combatInfo.EnemiesTargetedBy();
            foreach(var enemy in enemies)
            {
                //enemy.GetComponentInChildren<ThreatManager>().IncreaseThreat(source, value);
                enemy.ThreatManager().IncreaseThreat(source, value);
            }


        } catch { }
    }
    public void ClearThreats()
    {
        OnClearThreats?.Invoke(this);
        highestThreat = null;
    }

    public void ResetThreats()
    {
        ClearThreats();
        bool foundEntity = false;
        lineOfSight.EmitOmniSensor(HandleEntity);
        

        void HandleEntity(EntityInfo entity)
        {
            if (foundEntity) return;
            if (entityInfo.CanAttack(entity))
            {
                foundEntity = true;
                IncreaseThreat(entity, 15f, true);
            }
        }
    }
    public ThreatInfo Threat(EntityInfo source)
    {
        if(threats == null)
        {
            threats = new List<ThreatInfo>();
            return null;
        }

        if (threatDict == null)
        {
            threatDict = new();
            return null;
        }

        if (!threatDict.ContainsKey(source)) return null;

        return threatDict[source];
        //foreach(var current in threats)
        //{
        //    if(current.threatObject == source)
        //    {
        //        return current;
        //    }
        //}

        //return null;
    }
    public bool ThreatContainsEnemies()
    {
        foreach(var info in threats)
        {
            if(entityInfo.CanAttack(info.threatInfo))
            {
                return true;
            }
        }

        return false;
    }
}
