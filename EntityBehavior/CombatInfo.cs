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

    //Combat Logs
    [Serializable]
    public class CombatLogs
    {
        [Serializable]
        public struct Values
        {
            public float damageDone;
            public float damageTaken;
            public float healingDone;
            public float healingTaken;
            public float threatGenerated;
        }

        public Values values;


        public void ProcessEntity(EntityInfo entity)
        {
            entity.OnDamageDone += OnDamageDone;
            entity.OnDamageTaken += OnDamageTaken;
            entity.OnHealingDone += OnHealingDone;
            entity.OnHealingTaken += OnHealingTaken;

            var threatManager = entity.GetComponentInChildren<ThreatManager>();

            threatManager.OnGenerateThreat += OnGenerateThreat;
        }

        public void OnDamageDone(CombatEventData eventData)
        {

            values.damageDone += eventData.value;
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

        public void OnGenerateThreat(ThreatManager.ThreatInfo threatInfo)
        {
            values.threatGenerated += threatInfo.threatValue;
        }

    }
    public CombatLogs combatLogs;

    public List<GameObject> targetedBy;
    public List<GameObject> castedBy;

    public bool isClearing;

    //Events

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

    public void OnNewTarget(GameObject previousTarget, GameObject newTarget)
    {
        if(previousTarget != null)
        {
            var previousInfo = previousTarget.GetComponent<EntityInfo>();
            var combatInfo = previousInfo.GetComponentInChildren<CombatInfo>();
            combatInfo.RemoveTarget(entityInfo.gameObject);

        }

        if(newTarget != null)
        {
            var newInfo = newTarget.GetComponent<EntityInfo>();
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

        var combatInfo = ability.target.GetComponentInChildren<CombatInfo>();
        combatInfo.AddCaster(entityInfo.gameObject);
    }

    public void OnAbilityEnd(AbilityInfo ability)
    {
        if (ability.isAttack) return;
        if (ability.targetLocked == null) return;
        var combatInfo = ability.targetLocked.GetComponentInChildren<CombatInfo>();

        combatInfo.RemoveCaster(entityInfo.gameObject);
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
