using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Linq;

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
            catalystInfo.OnDeadTarget += OnDeadTarget;

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
        HandleNearTargets();
        LookForNewTarget();
    }

    public void HandleNearTargets()
    {
        var entities = Entity.EntitesWithinLOS(transform.position, 2f);

        if (entities.Count == 0) return;

        foreach (var entity in entities)
        {
            if (catalystHit.CanHit(entity))
            {
                catalystHit.HandleTargetHit(entity);
            }

        }
    }

    public void OnDeadTarget(GameObject target)
    {
        LookForNewTarget();
    }
    public void LookForNewTarget()
    {
        if (catalystInfo == null || abilityInfo == null || catalystHit == null) { return; }

        var entityList = catalystInfo.EntitiesWithinRadius(bounceRadius);

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
            foreach (var entity in entityList)
            {
                if (entity.GetComponent<EntityInfo>())
                {
                    var targetInfo = entity.GetComponent<EntityInfo>();

                    bool canTarget = catalystHit.CanHit(targetInfo);
                    bool isNotSameTarget = entity != catalystInfo.target;
                    bool isAlive = targetInfo.isAlive;
                    bool isPriority = targetInfo.npcType == priority;
                    bool hasLOS = !V3Helper.IsObstructed(entity.transform.position, transform.position, abilityInfo.obstructionLayer);

                    if (canTarget && isPriority && isAlive && hasLOS && isNotSameTarget)
                    {
                        
                        catalystInfo.target = entity.gameObject;


                        if (V3Helper.Distance(entity.transform.position, transform.position) < .50f)
                        {
                            GetComponent<CatalystHit>().HandleTargetHit(targetInfo);
                        }
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
