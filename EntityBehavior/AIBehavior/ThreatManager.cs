using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
using System.Threading.Tasks;
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

    [Serializable]
    public class ThreatInfo
    {
        public ThreatManager threatManager;
        public GameObject sourceObject;
        public GameObject threatObject;
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
            this.sourceObject = source;
            this.threatObject = threat;
            this.threatValue = val;

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
                threatManager.RemoveThreat(this);
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

    public event Action<ThreatInfo> OnNewThreat;
    public event Action<ThreatInfo, float> OnIncreaseThreat;
    public event Action<ThreatInfo> OnRemoveThreat;
    public event Action<ThreatInfo, float> OnGenerateThreat;
    public event Action<ThreatManager> OnEmptyThreats;
    public event Action<ThreatManager> OnClearThreats;
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
        }

        behavior = GetComponentInParent<AIBehavior>();

        if (behavior)
        {
            lineOfSight = behavior.LineOfSight();
            behavior.events.OnSightedEntity += OnSighted;
            combatInfo = behavior.GetComponentInChildren<CombatInfo>();
        }

        //if(GetComponentInParent<AIBehavior>())
        //{ 
        //    behavior = GetComponentInParent<AIBehavior>();
        //    if(behavior.LineOfSight())
        //    {
                
        //    }

        //    combatInfo = transform.parent.GetComponentInChildren<CombatInfo>();
            
        //}

        //if(lineOfSight == null)
        //{
        //    if (transform.parent.GetComponent<AIBehavior>())
        //    {
        //        behavior = transform.parent.GetComponent<AIBehavior>();

        //        if (behavior.entityObject)
        //        {
        //            entityObject = behavior.entityObject;
        //            entityInfo = entityObject.GetComponent<EntityInfo>();

        //            if (entityInfo.AbilityManager())
        //            {
        //                abilityManager = entityInfo.AbilityManager();
        //            }
        //        }

                
        //    }

        //    if(behavior && behavior.LineOfSight())
        //    {
        //        lineOfSight = behavior.LineOfSight();
        //    }
        //}
       
    }
    void Start()
    {
        GetDependencies();
        StartCoroutine(HandleThreat());
    }
    // Update is called once per frame
    void Update()
    {
    }
    public void OnChangeNpcType(NPCType before, NPCType after)
    {
        if(after == NPCType.Friendly)
        {
            ArchAction.Delay(() => 
            {
                CheckThreats(NPCType.Hostile, true, false);
            }, .50f);
        }
        else if (after == NPCType.Hostile)
        {
            CheckThreats(NPCType.Friendly);
        }

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

        var threatVal = val;
        if(threatVal == 0) { threatVal++; }

        
        if(source.role == Role.Tank)
        {
            if(GMHelper.Difficulty() != null)
            {
                threatVal *= GMHelper.Difficulty().settings.tankThreatMultiplier;
            }
        }
        
        IncreaseThreat(source.gameObject, threatVal, true);
    }

    public void OnBuffApply(BuffInfo appliedBuff, EntityInfo source)
    {

    }
    public void OnNewBuff(BuffInfo newBuff, EntityInfo source)
    {
        if(newBuff.buffTargetType == BuffTargetType.Harm)
        {
            var value = newBuff.properties.value;
            if(value <= 0) { value = 15; }
            IncreaseThreat(source.gameObject, value, true);
        }
    }
    public void OnHealingTaken(CombatEventData eventData)
    {
        var source = eventData.source;
        var val = eventData.value;

        var threatMultiplier = GMHelper.Difficulty().settings.healThreatMultiplier;

        val *= threatMultiplier;

        TriggerEnemies(source.gameObject, val);
    }
    public void RemoveThreat(ThreatInfo threat)
    {
        if(threat == null) { return; }
        if(!threats.Contains(threat)) { return; }
        threats.Remove(threat);
        HandleMaxThreat();
        OnRemoveThreat?.Invoke(threat);


        if(!ThreatContainsEnemies())
        {
            ClearThreats();
        }

        if(threats.Count == 0)
        {
            OnEmptyThreats?.Invoke(this);
        }
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
    public IEnumerator HandleThreat()
    {
        if(abilityManager == null || behavior == null)
        {
            StopCoroutine(HandleThreat());
        }

        while(true)
        {
            yield return new WaitForSeconds(.5f);
            if (!entityInfo.isAlive) { continue; }
            HandleThreatHostile();

        }

        
        
        void HandleThreatHostile()
        {
            if(entityInfo.npcType != NPCType.Hostile) { return; }
            if (behavior.behaviorType != AIBehaviorType.NoControl) return;
            CheckThreats(NPCType.Friendly, false, true);
        }
    }

    public void OnSighted(GameObject target)
    {
        if (behavior.combatType != CombatBehaviorType.Aggressive) return;
        if (!entityInfo.CanAttack(target)) return;
        if (entityInfo.isInCombat) return;
        if (!entityInfo.isAlive) return;

        IncreaseThreat(target, 15, true);
        behavior.events.OnDetectedEnemy?.Invoke(target);
    }

    public async void CheckThreats(NPCType enemyType, bool checkForCombat = false, bool alertAllies = false)
    {
        return;
        var enemiesDetected = lineOfSight.DetectedEntities(enemyType);

        if(enemiesDetected.Count == 0) { return; }

        foreach(var sighted in enemiesDetected)
        {
            await Task.Yield();
            if (!lineOfSight.HasLineOfSight(sighted.gameObject)) continue;
            if (!sighted.isAlive) continue;
            if (Threat(sighted.gameObject) != null) continue;


            if(entityInfo.CanAttack(sighted.gameObject))
            {
                if(checkForCombat)
                {
                    if(sighted.isInCombat)
                    {
                        IncreaseThreat(sighted.gameObject, 15, alertAllies);
                    }
                }
                else
                {
                    IncreaseThreat(sighted.gameObject, 15, alertAllies);
                }
            }
        }
    }
    public void IncreaseThreat(GameObject source, float value, bool alertAllies = false)
    {
        if (!entityInfo.isAlive) return;
        if(entityInfo.gameObject == source) { return; }
        if (entityInfo.npcType == source.GetComponent<EntityInfo>().npcType) return;


        if (!source.GetComponent<EntityInfo>().isAlive) { return; }
        var sourceInfo = source.GetComponent<EntityInfo>();
        var sourceThreat = source.GetComponentInChildren<ThreatManager>();

        

        var threatInfo = Threat(source);

        if(threatInfo == null)
        {
            threatInfo = new ThreatInfo(this, entityObject, source, value);
            threats.Add(threatInfo);
            OnNewThreat?.Invoke(threatInfo);
        }
        else
        {
            threatInfo.IncreaseThreat(value);
        }

        OnIncreaseThreat?.Invoke(threatInfo, value);
        sourceThreat.OnGenerateThreat?.Invoke(threatInfo, value);

        HandleMaxThreat();

        

        if(alertAllies)
        {
            AlertAllies(source);
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

        foreach(var info in threats)
        {
            if(info.threatValue > max)
            {
                if(entityInfo.CanAttack(info.threatObject))
                {
                    highestThreat = info.threatObject;
                    max = info.threatValue;
                }
            }
        }


        if(threats.Count == 0)
        {
            highestThreat = null;
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
    public async void AlertAllies(GameObject target)
    {
        if (!entityInfo.isAlive) return;
        

        var allies = lineOfSight.DetectedAllies();

        await Task.Delay((int)(.25f) * 1000);

        foreach (var ally in allies)
        {
            if (ally == entityInfo) continue;
            var threatManager = ally.GetComponentInChildren<ThreatManager>();
            if (threatManager == null) continue;
            if (threatManager.Threat(target) != null) continue;

            threatManager.IncreaseThreat(target, 15f);
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
        foreach(var current in threats)
        {
            if(current.threatObject == source)
            {
                return current;
            }
        }

        return null;
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
