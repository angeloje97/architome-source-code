using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class RBIndicatorBehavior : MonoBehaviour
{
    // Start is called before the first frame update
    //I have no idea what the RB stands for in RBIndicatorBehavior.
    
    public EntityInfo entityInfo;
    public CombatBehavior combatBehavior;
    public GameObject roleAggroIndicator;

    [Header("Indicator Settings")]
    public bool showAggroIndicator;

    public void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            combatBehavior = entityInfo.CombatBehavior() ? entityInfo.CombatBehavior() : null;
            entityInfo.OnChangeNPCType += OnChangeNPCType;

            if(entityInfo.npcType == NPCType.Hostile) { showAggroIndicator = true; }

            if(combatBehavior)
            {
                combatBehavior.OnNewTarget += OnNewTarget;
            }
        }
    }

    
    void Start()
    {
        GetDependencies();
    }

    public void OnNewTarget(GameObject previousTarget, GameObject newTarget)
    {
        
        HandleAggroIndicator();

        void HandleAggroIndicator()
        {
            if (!showAggroIndicator) { return; }
            if (newTarget == null)
            {
                SetAggroIndicator(false);
                return;
            }
            if (!entityInfo.CanAttack(newTarget)) { return; }

            var isTank = newTarget.GetComponent<EntityInfo>().role == Role.Tank;

            SetAggroIndicator(isTank ? false : true);
        }
    }

    public void OnChangeNPCType(NPCType before, NPCType after)
    {
        showAggroIndicator = after == NPCType.Hostile;

        if(!showAggroIndicator)
        {
            SetAggroIndicator(false);
        }
    }

    void SetAggroIndicator(bool val)
    {
        roleAggroIndicator.GetComponent<CanvasGroup>().alpha = val? 1 : 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}