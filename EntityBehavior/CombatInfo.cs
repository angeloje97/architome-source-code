using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome;
public class CombatInfo : MonoBehaviour
{
    // Start is called before the first frame update
    public EntityInfo entityInfo;
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

    //Events

    public event Action<GameObject, List<GameObject>> OnNewTargetedBy;
    public event Action<GameObject, List<GameObject>> OnTargetedByRemove;

    void Start()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            combatLogs = new CombatLogs();
            combatLogs.ProcessEntity(entityInfo);
        }

        if(GetComponent<CombatBehavior>())
        {
            combatBehavior = GetComponent<CombatBehavior>();

            combatBehavior.OnNewTarget += OnNewTarget;
        }
    }

    protected void AddTarget(GameObject target)
    {
        if(!targetedBy.Contains(target))
        {
            targetedBy.Add(target);
            OnNewTargetedBy?.Invoke(target, targetedBy);
        }
    }

    protected void RemoveTarget(GameObject target)
    {
        if(targetedBy.Contains(target))
        {
            targetedBy.Remove(target);
            OnTargetedByRemove?.Invoke(target, targetedBy);
        }
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

    public List<GameObject> EnemiesTargetedBy()
    {
        return targetedBy.FindAll(entity => entityInfo.CanAttack(entity));
    }



    // Update is called once per frame
}
