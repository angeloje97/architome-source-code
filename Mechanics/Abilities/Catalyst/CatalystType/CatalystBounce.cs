using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;

public class CatalystBounce : MonoBehaviour
{
    // Start is called before the first frame update
    public AbilityInfo abilityInfo;
    public CatalystInfo catalystInfo;
    public CatalystHit catalystHit;

    public LayerMask targetLayerMask;
    public LayerMask obstructionLayer;

    public NPCType priorityNPCType;
    public List<NPCType> possibleNPCTypes;
    public int currentTicks;

    public float bounceRadius;

    public bool cantFindEntity;

    void GetDependencies()
    {
        if(gameObject.GetComponent<CatalystInfo>())
        {
            catalystInfo = gameObject.GetComponent<CatalystInfo>();

            catalystInfo.OnTickChange += OnTickChange;

            abilityInfo = catalystInfo.abilityInfo;

            if(catalystInfo.target)
            {
                priorityNPCType = catalystInfo.target.GetComponent<EntityInfo>().npcType;
            }
            currentTicks = catalystInfo.Ticks();
        }
        if(gameObject.GetComponent<CatalystHit>())
        {
            catalystHit = gameObject.GetComponent<CatalystHit>();
        }

        if(abilityInfo)
        {
            targetLayerMask = abilityInfo.targetLayer;
            obstructionLayer = abilityInfo.obstructionLayer;
            bounceRadius = abilityInfo.bounceRadius;
        }
        possibleNPCTypes = new List<NPCType>();
        possibleNPCTypes.Add(NPCType.Friendly);
        possibleNPCTypes.Add(NPCType.Hostile);
        possibleNPCTypes.Add(NPCType.Neutral);

    }
    void Start()
    {
        GetDependencies();
    }
    // Update is called once per frame
    void Update()
    {
    }

    public void OnTickChange(CatalystInfo catalyst, int ticks)
    {
        if(ticks == 0) { return; }
        LookForNewTarget();

    }
    public void LookForNewTarget()
    {
        if (catalystInfo == null || abilityInfo == null || catalystHit == null) { return; }

        Collider[] rangeChecks = Physics.OverlapSphere(gameObject.transform.position, bounceRadius, targetLayerMask);

        Debugger.InConsole(9414, $"Starting to find new target");

        if (FindEntity(priorityNPCType)) {
            Debugger.InConsole(9415, $"Found PriorityNPC");
            return; }
        else
        {
            foreach (NPCType npcType in possibleNPCTypes)
            {
                if (npcType != priorityNPCType)
                {
                    if (FindEntity(npcType)){
                        Debugger.InConsole(9417, $"Found Targetable NPC");
                        return; }
                }
            }

            cantFindEntity = true;
            catalystInfo.OnCantFindEntity?.Invoke(catalystInfo);
        }
        
        bool FindEntity(NPCType priority)
        {
            foreach (Collider entity in rangeChecks)
            {
                if (entity.GetComponent<EntityInfo>())
                {
                    var targetInfo = entity.GetComponent<EntityInfo>();

                    bool canTarget = catalystHit.CanHit(targetInfo);
                    bool isAlive = targetInfo.isAlive;
                    bool isPriority = targetInfo.npcType == priority;
                    bool hasLOS = HasLineOfSight(targetInfo);

                    if (canTarget && isPriority && isAlive && hasLOS)
                    {
                        catalystInfo.target = entity.gameObject;
                        return true;
                    }
                }
            }
            return false;

            bool HasLineOfSight(EntityInfo targetInfo)
            {
                if (abilityInfo.bounceRequiresLOS == false) { return true; }

                var direction = V3Helper.Direction(targetInfo.transform.position, transform.position);
                var distance = V3Helper.Distance(targetInfo.transform.position, transform.position);

                if(!Physics.Raycast(transform.position, direction, distance, abilityInfo.obstructionLayer))
                {
                    return true;
                }

                return false;
            }
        }
    }
}
