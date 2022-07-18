using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
using System.Threading.Tasks;
using System.Linq;
public class ThreatManager : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;

    public AIBehavior behavior;
    public AbilityManager abilityManager;
    public LineOfSight lineOfSight;
    public CombatInfo combatInfo;
    public GameObject highestThreat;
    public GameObject highestNonTargettingThreat;

    [Serializable]
    public class ThreatInfo
    {
        public ThreatManager threatManager;
        public CombatBehavior combatBehavior;
        public GameObject sourceObject;
        public GameObject threatObject;
        public EntityInfo threatInfo;
        public float threatValue;
        public float timeInCombat;
        public bool isActive;

        public void IncreaseThreat(float val)
        {
            threatValue += val;
        }
        public ThreatInfo(ThreatManager threatManager, GameObject source, GameObject threat, float val)
        {
            this.threatManager = threatManager;
            sourceObject = source;
            threatObject = threat;
            threatValue = val;
            threatInfo = threatObject.GetComponent<EntityInfo>();
            combatBehavior = threatObject.GetComponentInChildren<CombatBehavior>();

            threatManager.OnClearThreats += OnClearThreats;
            sourceObject.GetComponent<EntityInfo>().OnDeath += OnSourceDeath;
            threatObject.GetComponent<EntityInfo>().OnDeath += OnThreatDeath;
            

            isActive = true;
            CombatTimeRoutine();
        }
        public async void CombatTimeRoutine()
        {
            while (isActive)
            {
                await Task.Delay(1000);
                timeInCombat += 1;
                try
                {
                    if (!threatObject.GetComponent<EntityInfo>().isAlive)
                    {
                        RemoveThreat();
                    }

                    if (!sourceObject.GetComponent<EntityInfo>().CanAttack(threatObject))
                    {
                        RemoveThreat();
                    }

                    if (!sourceObject.GetComponent<EntityInfo>().isAlive)
                    {
                        threatManager.ClearThreats();
                    }
                }
                catch {}
                

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
            try
            {
                threatManager.RemoveThreat(threatObject);
                Debugger.InConsole(3974, $"{threatObject} removes from {sourceObject}");
                sourceObject.GetComponent<EntityInfo>().OnDeath -= OnSourceDeath;
                threatObject.GetComponent<EntityInfo>().OnDeath -= OnThreatDeath;
                threatManager.OnClearThreats -= OnClearThreats;
                isActive = false;
            }
            catch
            {

            }
            
        }
    }

    public List<ThreatInfo> threats;
    public Dictionary<GameObject, ThreatInfo> threatDict { get; set; }
    public List<GameObject> threatQueue;

    public event Action<ThreatInfo> OnNewThreat;
    public event Action<ThreatInfo, float> OnIncreaseThreat;
    public event Action<ThreatInfo> OnFirstThreat;
    public event Action<ThreatInfo> OnRemoveThreat;
    public event Action<ThreatInfo, float> OnGenerateThreat;
    public event Action<ThreatManager> OnEmptyThreats;
    public event Action<ThreatManager> OnClearThreats;


    bool sortingThreats;
    bool combatActive;
    public void GetDependencies()
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
            entityInfo.combatEvents.OnPingThreat += OnPingThreat;
        }

        behavior = GetComponentInParent<AIBehavior>();

        if (behavior)
        {
            lineOfSight = behavior.LineOfSight();
            behavior.events.OnSightedEntity += OnSighted;
            
            combatInfo = behavior.GetComponentInChildren<CombatInfo>();
        }
       
    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void OnChangeNpcType(NPCType before, NPCType after)
    {

        HandleMaxThreat();


    }
    public void OnDamageDone(CombatEventData eventData)
    {
        var target = eventData.target;
        if(Threat(target.gameObject) == null)
        {
            IncreaseThreat(target.gameObject, 1);
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
        
        IncreaseThreat(source.gameObject, threatVal);
        AlertAllies(source.gameObject);


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


        var value = newBuff.properties.value;
        if(value <= 0) { value = 15; }
        IncreaseThreat(source.gameObject, value);
        AlertAllies(source.gameObject, .25f);

    }
    public void OnHealingTaken(CombatEventData eventData)
    {
        var source = eventData.source;
        var val = eventData.value;

        var threatMultiplier = ThreatMultiplier(source);

        val *= threatMultiplier;

        TriggerEnemies(source.gameObject, val);
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

    public void OnPingThreat(EntityInfo source, float value)
    {
        IncreaseThreat(source.gameObject, value);
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
    public void RemoveThreat(GameObject threatObject)
    {
        var threat = Threat(threatObject);
        if(threat == null) { return; }

        if(!threats.Contains(threat)) { return; }
        threats.Remove(threat);
        threatDict.Remove(threatObject);
        
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
        if(!isAlive)
        {
            ClearThreats();
        }
    }
    public void OnKill(CombatEventData eventData)
    {
        //var target = eventData.target;

        //RemoveThreat(Threat(target.gameObject));
    }
    public void OnSighted(GameObject target)
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
    public void IncreaseThreat(GameObject source, float value, bool fromAlert = false)
    {
        if (!entityInfo.isAlive) return;
        if (!entityInfo.CanAttack(source)) return;
        if(entityInfo.gameObject == source) { return; }
        if (entityInfo.npcType == source.GetComponent<EntityInfo>().npcType) return;

        bool firstThreat = threats.Count == 0;

        var sourceInfo = source.GetComponent<EntityInfo>();
        if (sourceInfo == null) return;

        var sourceThreat = source.GetComponentInChildren<ThreatManager>();

        var threatInfo = Threat(source);

        if(threatInfo == null)
        {
            threatInfo = new ThreatInfo(this, entityObject, source, value);
            threats.Add(threatInfo);
            OnNewThreat?.Invoke(threatInfo);
            threatDict.Add(source, threatInfo);
        }
        else
        {
            threatInfo.IncreaseThreat(value);
        }

        OnIncreaseThreat?.Invoke(threatInfo, value);
        sourceThreat.OnGenerateThreat?.Invoke(threatInfo, value);

        HandleMaxThreat();
        SortThreats();

        if (firstThreat && !fromAlert)
        {
            OnFirstThreat?.Invoke(threatInfo);
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
            IncreaseThreat(threat.threatObject, 1);
        }
    }
    public GameObject RandomTargetBlackList(List<Role> blackListRole)
    {

        if (BlackList(blackListRole).Count == 0) { return null; }

        else
        {
            return BlackList(blackListRole)[UnityEngine.Random.Range(0, BlackList(blackListRole).Count)];
        }
    }
    public List<GameObject> WhiteList(List<Role> whiteListRole)
    {
        var whiteListed = new List<GameObject>();

        foreach(var threat in threats)
        {
            if(whiteListRole.Contains(threat.threatObject.GetComponent<EntityInfo>().role))
            {
                whiteListed.Add(threat.threatObject);
            }
        }
        return whiteListed;
    }
    public List<GameObject> BlackList(List<Role> blackListRole)
    {
        var blackListed = new List<GameObject>();

        foreach(var threat in threats)
        {
            if(!blackListRole.Contains(threat.threatObject.GetComponent<EntityInfo>().role))
            {
                blackListed.Add(threat.threatObject);
            }
        }
        return blackListed;
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
                if (entityInfo.CanAttack(info.threatObject))
                {
                    highestThreat = info.threatObject;
                    max = info.threatValue;
                }
            }
        }
        
        void HandleNonTargetting(ThreatInfo info)
        {
            if (info.threatValue < maxNonTargetting) return;
            if (!info.threatInfo.isInCombat) return;
            if (info.combatBehavior.target == entityObject) return;

            maxNonTargetting = info.threatValue;
            highestNonTargettingThreat = info.threatObject;
        }
    }
    public void HandleMaxThreatNonTargetting()
    {
        
        if (threats.Count == 0)
        {
            highestNonTargettingThreat = null;
            return;
        }

        float max = float.NegativeInfinity;

        GameObject target = null;

        foreach (var threat in threats)
        {
            if (threat.threatValue < max) continue;
            if (!threat.threatInfo.isInCombat) continue;
            if (threat.combatBehavior.target == entityObject) continue;

            max = threat.threatValue;
            target = threat.threatObject;
        }

        if (target == null)
        {
            highestNonTargettingThreat = null;
        }
        else
        {
            highestNonTargettingThreat = target;
        }




    }
    public GameObject NearestHighestThreat(float range)
    {

        GameObject target = null;
        var furthest = 0f;
        var highestThreat = 0f;
        foreach(var threat in threats)
        {
            var distance = V3Helper.Distance(threat.threatObject.transform.position, entityObject.transform.position);
            if(distance <= range && distance > furthest && threat.threatValue > highestThreat)
            {
                target = threat.threatObject;

                if (!lineOfSight.HasLineOfSight(target)) continue;
                furthest = distance;
                highestThreat = threat.threatValue;
            }
        }
        return target;
    }
    async public void AlertAllies(GameObject target, float delay = 0f, float val = 0f)
    {
        if (!entityInfo.isAlive) return;
        await Task.Delay((int)(delay * 1000));

        //Debugger.InConsole(4598, $"Delay is {(int)(.25f * 1000)}");

        var allies = lineOfSight.DetectedAllies();


        foreach (var ally in allies)
        {
            if (ally.npcType != entityInfo.npcType) continue;
            if (ally == entityInfo) continue;
            var threatManager = ally.GetComponentInChildren<ThreatManager>();
            if (threatManager == null) continue;
            if (threatManager.Threat(target) != null) continue;

            threatManager.IncreaseThreat(target, val == 0f ? 15f : val, true);
        }
    }
    public void TriggerEnemies(GameObject source, float value)
    {
        try 
        {
            var enemies = combatInfo.EnemiesTargetedBy();
            foreach(var enemy in enemies)
            {
                enemy.GetComponentInChildren<ThreatManager>().IncreaseThreat(source, value);
            }


        } catch { }
    }
    public void ClearThreats()
    {
        OnClearThreats?.Invoke(this);
        highestThreat = null;
    }
    public ThreatInfo Threat(GameObject source)
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
            if(entityInfo.CanAttack(info.threatObject))
            {
                return true;
            }
        }

        return false;
    }
}
