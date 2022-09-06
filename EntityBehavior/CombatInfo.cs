using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome;
using System.Threading.Tasks;
public class CombatInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
    public AbilityManager abilityManager;
    public CombatBehavior combatBehavior;
    public bool ignoreIndicator;

    //Combat Logs
    [Serializable]
    public class CombatLogs
    {
        [Serializable]
        public struct Values
        {
            public float damageDone;
            public float damageTaken;
            public float damagePreventedFromShields;
            public float healingDone;
            public float healingTaken;
            public float threatGenerated;
            public float experienceGained;
            public float deaths;
        }

        [Serializable]
        public class Logs
        {
            public List<string> logs;
            public bool healing;
            public bool damage;

            public void LogCombatData(string text)
            {
                if (logs == null) logs = new();
                if (logs.Count > 25) logs.RemoveAt(0);

                logs.Add(text);
            }
        }

        [Serializable]
        public class LevelProperties
        {
            public int startingLevel, currentLevel;

            public float startingExperience, currentExperienceGained;


            public void InitializeEntity(EntityInfo entity)
            {
                entity.OnLevelUp += OnLevelUp;
                entity.OnExperienceGain += OnExperienceGain;



                startingLevel = entity.entityStats.Level;
                currentLevel = entity.entityStats.Level;

                startingExperience = entity.entityStats.experience;
                currentExperienceGained = startingExperience;
            }

            public void OnLevelUp(int newLevel)
            {
                var difficulty = DifficultyModifications.active;
                if (difficulty == null) return;

                currentLevel = newLevel;
            }

            public void OnExperienceGain(float amount)
            {
                Debugger.InConsole(4581, $"{amount}");
                currentExperienceGained += amount;
            }
        }

        public EntityInfo entity;

        public Values values;
        public Values startCombatValues;
        public Values combatValues;

        public LevelProperties levels;
        public Logs logs = new();

        public float secondsInCombat;
        public float totalSecondsInCombat;

        bool isRecording;

        public void ProcessEntity(EntityInfo entity)
        {
            this.entity = entity;
            entity.OnDamageDone += OnDamageDone;
            entity.OnDamageTaken += OnDamageTaken;
            entity.OnHealingDone += OnHealingDone;
            entity.OnHealingTaken += OnHealingTaken;
            entity.OnLifeChange += OnLifeChange;
            entity.OnCombatChange += OnCombatChange;
            entity.OnExperienceGain += OnExperienceGain;
            entity.OnDamagePreventedFromShields += OnDamagePreventedFromShields;

            entity.combatEvents.OnGenerateThreat += OnGenerateThreat;


            ArchAction.Delay(() => {
                levels = new();
                levels.InitializeEntity(entity);
            },.250f);

        }

        async void OnCombatChange(bool isInCombat)
        {
            if (!isInCombat) return;
            if (isRecording) return;

            isRecording = true;
            secondsInCombat = 0f;

            startCombatValues = values;

            while (entity.isInCombat || (entity.PartyInfo() && entity.PartyInfo().partyIsInCombat))
            {
                await Task.Delay(250);
                secondsInCombat += .25f;
                totalSecondsInCombat += .25f;
            }

            SaveCombatValues();

            isRecording = false;
        }

        void SaveCombatValues()
        {
            foreach (var field in combatValues.GetType().GetFields())
            {
                var total = (float)field.GetValue(values);
                var start = (float)field.GetValue(startCombatValues);
                var current = (float)field.GetValue(combatValues);
                field.SetValue(combatValues, current + total - start);
            }
        }

        public void OnDamageDone(CombatEventData eventData)
        {

            values.damageDone += eventData.value;
            if (logs.damage)
            {
                var source = eventData.source.entityName;
                if (eventData.catalyst)
                {
                    source = eventData.catalyst.name;
                }
                else if (eventData.buff)
                {
                    source = eventData.buff.name;
                }

                logs.LogCombatData($"{source} did {eventData.value} damage to {eventData.target.entityName}");

            }
        }

        
        public void OnLifeChange(bool val)
        {
            if (!val)
            {
                values.deaths++;
            }
        }

        public void OnDamageTaken(CombatEventData eventData)
        {
            values.damageTaken += eventData.value;
        }

        public void OnHealingDone(CombatEventData eventData)
        {
            values.healingDone += eventData.value;
        }
           
        public void OnHealingTaken(CombatEventData eventData)
        {
            values.healingTaken += eventData.value;
        }

        public void OnExperienceGain(float experience)
        {
            values.experienceGained += experience;
        }

        public void OnDamagePreventedFromShields(CombatEventData eventData)
        {
            values.damagePreventedFromShields += eventData.value;
        }

        public void OnGenerateThreat(ThreatManager.ThreatInfo threatInfo, float value)
        {
            values.threatGenerated += value;
        }

    }
    public CombatLogs combatLogs;

    public List<GameObject> targetedBy;
    public List<GameObject> castedBy;

    public bool isClearing;

    public event Action<GameObject, List<GameObject>> OnNewTargetedBy;
    public event Action<GameObject, List<GameObject>> OnTargetedByRemove;
    public Action<CombatInfo> OnTargetedByEvent;

    void Start()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            combatLogs = new CombatLogs();
            combatLogs.ProcessEntity(entityInfo);
            abilityManager = entityInfo.AbilityManager();
        }

        if(GetComponent<CombatBehavior>())
        {
            combatBehavior = GetComponent<CombatBehavior>();

            combatBehavior.OnNewTarget += OnNewTarget;
        }

        if (abilityManager)
        {
            abilityManager.OnAbilityStart += OnAbilityStart;
            abilityManager.OnAbilityEnd += OnAbilityEnd;
        }

        if (entityInfo)
        {
            entityInfo.OnLifeChange += OnLifeChange;
        }
    }
    protected void AddTarget(GameObject target)
    {
        if(!targetedBy.Contains(target))
        {
            targetedBy.Add(target);
            OnNewTargetedBy?.Invoke(target, targetedBy);
            OnTargetedByEvent?.Invoke(this);
            ClearEnemies();
        }
    }
    protected void RemoveTarget(GameObject target)
    {
        if(targetedBy.Contains(target))
        {
            targetedBy.Remove(target);
            OnTargetedByRemove?.Invoke(target, targetedBy);
            OnTargetedByEvent?.Invoke(this);
        }
    }
    protected void AddCaster(GameObject target)
    {
        if (castedBy.Contains(target)) return;

        castedBy.Add(target);
        OnTargetedByEvent?.Invoke(this);
        ClearEnemies();
    }
    protected void RemoveCaster(GameObject target)
    {
        if (!castedBy.Contains(target)) return;
        castedBy.Remove(target);
        OnTargetedByEvent?.Invoke(this);
    }
    public void OnNewTarget(EntityInfo previousInfo, EntityInfo newInfo)
    {
        if(previousInfo != null)
        {
            var combatInfo = previousInfo.GetComponentInChildren<CombatInfo>();
            combatInfo.RemoveTarget(entityInfo.gameObject);

        }

        if(newInfo != null)
        {
            if (ignoreIndicator) return;
            var combatInfo = newInfo.GetComponentInChildren<CombatInfo>();
            combatInfo.AddTarget(entityInfo.gameObject);
        }
    }
    public bool IsBeingAttacked()
    {
        if (EnemiesCastedBy().Count > 0)
        {
            return true;
        }

        if (EnemiesTargetedBy().Count > 0)
        {
            return true;
        }

        return false;
    }
    void ClearLogic()
    {
        if (entityInfo == null) return;
        for(int i = 0; i < castedBy.Count; i++) 
        {
            var caster = castedBy[i];
            var ability = caster.GetComponentInChildren<AbilityManager>().currentlyCasting;

            if (ability && !ability.isAttack && ability.target == entityInfo.gameObject)
            {
                continue;
            }

            castedBy.RemoveAt(i);

            i--;
            OnTargetedByEvent?.Invoke(this);
        }

        for (int i = 0; i < targetedBy.Count; i++)
        {
            var target = targetedBy[i];

            var combat = target.GetComponentInChildren<CombatBehavior>();
            

            if (combat.GetFocus() && combat.GetFocus() == entityInfo.gameObject)
            {
                continue;
            }

            if (combat.target == entityInfo.gameObject)
            {
                continue;
            }

            targetedBy.RemoveAt(i);
            i--;
            OnTargetedByEvent?.Invoke(this);
        }
    }
    async public void ClearEnemies()
    {
        if (isClearing) return;
        isClearing = true;

        while (EnemiesCastedBy().Count > 0 || EnemiesTargetedBy().Count > 0)
        {
            await Task.Delay(1000);
            if (entityInfo == null) break;
            ClearLogic();
        }

        isClearing = false;

    }
    public void OnLifeChange(bool isAlive)
    {
        if (isAlive) return;
        castedBy.Clear();
        targetedBy.Clear();
    }
    public void OnAbilityStart(AbilityInfo ability)
    {
        if(ability.isAttack) { return; }
        if (ability.target == null) return;
        if (ignoreIndicator) return;

        var combatInfo = ability.target.GetComponentInChildren<CombatInfo>();

        if (combatInfo)
        {
            combatInfo.AddCaster(entityInfo.gameObject);
        }
    }
    public void OnAbilityEnd(AbilityInfo ability)
    {
        if (ability.isAttack) return;
        if (ability.targetLocked == null) return;
        var combatInfo = ability.targetLocked.GetComponentInChildren<CombatInfo>();

        if (combatInfo)
        {
            combatInfo.RemoveCaster(entityInfo.gameObject);
        }
    }
    public List<GameObject> EnemiesTargetedBy()
    {
        return targetedBy.Where(entity => entity.GetComponent<EntityInfo>().CanAttack(entityInfo.gameObject)).ToList();
    }

    public List<GameObject> EnemiesCastedBy()
    {
        return castedBy.Where(entity => entity.GetComponent<EntityInfo>().CanAttack(entityInfo.gameObject)).ToList();
    }



    // Update is called once per frame
}
