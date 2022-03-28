using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
        public float damageDone;
        public float damageTaken;
        public float healingDone;
        public float healingTaken;
        public float threatGenerated;

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

            damageDone += eventData.value;
        }

        public void OnDamageTaken(CombatEventData eventData)
        {
            damageTaken += eventData.value;
        }

        public void OnHealingDone(CombatEventData eventData)
        {
            healingDone += eventData.value;
        }
           
        public void OnHealingTaken(CombatEventData eventData)
        {
            healingTaken += eventData.value;
        }

        public void OnGenerateThreat(ThreatManager.ThreatInfo threatInfo)
        {
            threatGenerated += threatInfo.threatValue;
        }

    }
    public CombatLogs combatLogs;

    public List<GameObject> targetedBy;
    public List<GameObject> castedBy;

    //Events

    public event Action<GameObject, List<GameObject>> OnNewTargetedBy;
    public event Action<GameObject, List<GameObject>> OnTargetedByRemove;
    public Action<CombatInfo> OnTargetedByEvent;

    public bool isClearing;

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
    }

    protected void AddTarget(GameObject target)
    {
        if(!targetedBy.Contains(target))
        {
            targetedBy.Add(target);
            OnNewTargetedBy?.Invoke(target, targetedBy);
            OnTargetedByEvent?.Invoke(this);
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

    public async Task BeingAttacked()
    {
        while (EnemiesTargetedBy().Count > 0 || EnemiesCastedBy().Count > 0)
        {
            await Task.Delay(1000);
            ClearAttackers();
        }
    }

    public void ClearAttackers()
    {
        for (int i = 0; i < castedBy.Count; i++)
        {
            var remove = false;

            if (castedBy[i] == null)
            {
                remove = true;
            }

            var currentlyCasting = castedBy[i].GetComponentInChildren<AbilityManager>().currentlyCasting;

            if (currentlyCasting == null)
            {
                remove = true;
            }
            else
            {
                var target = currentlyCasting.targetLocked;

                if (target != entityInfo.gameObject)
                {
                    remove = true;
                }
            }

           

            if(remove)
            {
                castedBy.RemoveAt(i);
                i--;
            }
        }
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
        return targetedBy.FindAll(entity => entityInfo.CanAttack(entity));
    }

    public List<GameObject> EnemiesCastedBy()
    {
        return castedBy.FindAll(entity => entityInfo.CanAttack(entity));
    }



    // Update is called once per frame
}
