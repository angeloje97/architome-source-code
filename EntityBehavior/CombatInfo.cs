using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome;
using System.Threading.Tasks;
using DungeonArchitect;

public class CombatInfo : EntityProp
{
    public AbilityManager abilityManager;
    public CombatBehavior combatBehavior;
    public bool ignoreIndicator;
    public bool isBeingAttacked;

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

        public CombatEvents combatEvents => entity.combatEvents;

        public void ProcessEntity(EntityInfo entity, EntityProp prop)
        {
            this.entity = entity;
            combatEvents.AddListenerHealth(eHealthEvent.OnDamageDone, OnDamageDone, prop);
            combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, OnDamageTaken, prop);

            combatEvents.AddListenerHealth(eHealthEvent.OnHealingDone, OnHealingDone, prop);
            combatEvents.AddListenerHealth(eHealthEvent.OnHealingTaken, OnHealingTaken, prop);
            combatEvents.AddListenerHealth(eHealthEvent.OnDamagePreventedFromShields, OnDamagePreventedFromShields, prop);

            entity.OnLifeChange += OnLifeChange;
            entity.OnCombatChange += OnCombatChange;
            entity.OnExperienceGain += OnExperienceGain;


            prop.combatEvents.AddListenerThreat(eThreatEvent.OnGenerateThreat, OnGenerateThreat, prop);

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

        public void OnDamageDone(HealthEvent eventData)
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

        public void OnDamageTaken(HealthEvent eventData)
        {
            values.damageTaken += eventData.value;
        }

        public void OnHealingDone(HealthEvent eventData)
        {
            values.healingDone += eventData.value;
        }
           
        public void OnHealingTaken(HealthEvent eventData)
        {
            values.healingTaken += eventData.value;
        }

        public void OnExperienceGain(float experience)
        {
            values.experienceGained += experience;
        }

        public void OnDamagePreventedFromShields(HealthEvent eventData)
        {
            values.damagePreventedFromShields += eventData.damagePreventedFromShield;
        }

        public void OnGenerateThreat(ThreatEvent eventData)
        {
            values.threatGenerated += eventData.threatValue;
        }

    }
    public CombatLogs combatLogs;

    public ActionHashList<EntityInfo> targetedBy;
    public ActionHashList<EntityInfo> castedBy;

    public bool isClearing;

    public EntityInfo currentTarget;
    public EntityInfo currentCastingTarget;

    public Action<EntityInfo, ActionHashList<EntityInfo>> OnNewTargetedBy { get; set; }
    public Action<EntityInfo, ActionHashList<EntityInfo>> OnTargetedByRemove { get; set; }
    public Action<CombatInfo> OnTargetedByEvent { get; set; }

    public Action<CombatInfo, bool> OnIsBeingAttackedChange;

    bool isBeingAttackedCheck;

    public override void GetDependencies()
    {
        combatBehavior = GetComponent<CombatBehavior>();

        castedBy = new();
        targetedBy = new();
        combatLogs = new CombatLogs();
        combatLogs.ProcessEntity(entityInfo, this);
        abilityManager = entityInfo.AbilityManager();
        combatEvents.AddListenerGeneral(eCombatEvent.OnNewCombatTarget, OnNewTarget, this);

        entityInfo.OnLifeChange += OnLifeChange;


        if (combatBehavior)
        {
            combatBehavior = GetComponent<CombatBehavior>();
        }

        if (abilityManager)
        {
            abilityManager.OnAbilityStart += OnAbilityStart;
        }
    }

    public override void EUpdate()
    {
        
        if(isBeingAttacked != isBeingAttackedCheck)
        {
            isBeingAttackedCheck = isBeingAttacked;

            OnIsBeingAttackedChange?.Invoke(this, isBeingAttacked);
        }
    }
    protected void AddTarget(EntityInfo target)
    {
        if (!entityInfo.isAlive) return;
        if (!targetedBy.Add(target)) return;

        OnNewTargetedBy?.Invoke(target, targetedBy);
        isBeingAttacked = IsBeingAttacked();

    }
    protected void AddCaster(EntityInfo target)
    {
        if (!entityInfo.isAlive) return;
        if (!castedBy.Add(target)) return;
        OnTargetedByEvent?.Invoke(this);

        isBeingAttacked = IsBeingAttacked();
    }
    protected void RemoveTarget(EntityInfo target)
    {
        if (!targetedBy.Remove(target)) return;

        OnTargetedByRemove?.Invoke(target, targetedBy);
        OnTargetedByEvent?.Invoke(this);

        isBeingAttacked = IsBeingAttacked();
    }
    protected void RemoveCaster(EntityInfo target)
    {
        if (!castedBy.Remove(target)) return;

        OnTargetedByEvent?.Invoke(this);
        isBeingAttacked = IsBeingAttacked();

        return;
    }
    public void OnNewTarget(CombatEvent eventData)
    {
        var newInfo = eventData.target;
        var previousInfo = eventData.previousTarget;
        currentTarget = newInfo;

        if(previousInfo != null)
        {
            var combatInfo = previousInfo.CombatInfo();
            combatInfo.RemoveTarget(entityInfo);

        }

        if(newInfo != null)
        {
            if (ignoreIndicator) return;
            var combatInfo = newInfo.CombatInfo();
            combatInfo.AddTarget(entityInfo);
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
    async public void ClearEnemies()
    {
        if (isClearing) return;
        isClearing = true;

        while (EnemiesCastedBy().Count > 0 || EnemiesTargetedBy().Count > 0)
        {
            await Task.Delay(1000);
            if (entityInfo == null) break;
            //ClearLogic();
        }

        isClearing = false;

    }
    public void OnLifeChange(bool isAlive)
    {
        if (isAlive) return;
        castedBy.Clear();
        targetedBy.Clear();

        isBeingAttacked = false;

        if (currentCastingTarget)
        {
            var combatInfo = currentCastingTarget.CombatInfo();

            combatInfo.RemoveCaster(entityInfo);
        }

        if (currentTarget)
        {
            var combatInfo = currentTarget.CombatInfo();

            combatInfo.RemoveTarget(entityInfo);
        }
    }
    public async void OnAbilityStart(AbilityInfo ability)
    {
        if(ability.isAttack) { return; }
        if (ability.target == null) return;
        if (ignoreIndicator) return;

        currentCastingTarget = ability.target;
        var abilityTarget = currentCastingTarget;
        var combatInfo = ability.target.CombatInfo();

        await Task.Delay(125);

        if (combatInfo)
        {
            combatInfo.AddCaster(entityInfo);
        }


        await ability.EndActivation();

        if(currentCastingTarget == abilityTarget)
        {
            currentCastingTarget = null;
        }
        
        combatInfo.RemoveCaster(entityInfo);
    }
    public List<EntityInfo> EnemiesTargetedBy()
    {
        targetedBy ??= new();
        return targetedBy.Where(entity => entity.CanAttack(entityInfo)).ToList();
    }
    public List<EntityInfo> EnemiesCastedBy()
    {
        castedBy ??= new();
        return castedBy.Where(entity => entity.CanAttack(entityInfo)).ToList();
    }



    // Update is called once per frame
}
