using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class LineOfSight : MonoBehaviour
{
    public GameObject entityObject;
    public EntityInfo entityInfo;
    public PartyInfo partyInfo;

    public List<GameObject> entitiesDetected;
    public List<GameObject> enemiesWithinLineOfSight;
    //public List<GameObject> entitiesWithinLineOfSight;

    public LayerMask targetLayer;
    public LayerMask obstructionLayer;

    public bool isPlayer;
    public bool hasLineOfSightOnCurrentTarget;

    public float radius;
    public float combatRadius;
    public float detectionInterval;
    public void GetDependencies()
    {
        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            if(entityInfo)
            {
                entityObject = entityInfo.gameObject;
            }
            else
            {
                return;
            }


        }

        if (GetComponentInParent<PartyInfo>())
        {
            partyInfo = GetComponentInParent<PartyInfo>();
        }

        if(entityInfo)
        {
            switch(entityInfo.entityControlType)
            {
                case EntityControlType.EntityControl:
                    isPlayer = true;
                    break;
                case EntityControlType.PartyControl:
                    isPlayer = true;
                    break;
            }
        }

        if (GMHelper.Difficulty())
        {
            if(isPlayer)
            {
                radius = GMHelper.Difficulty().settings.playerDetectionRange;
                if(combatRadius == 0) { combatRadius = radius; }
                detectionInterval = .5f;
            }
            else
            {
                radius = GMHelper.Difficulty().settings.npcDetectionRange;

                if(combatRadius == 0)
                {
                    combatRadius = radius;
                }
            }
        }

        if(GMHelper.GameManager().playableEntities.Contains(entityInfo))
        {
            isPlayer = true;
        }
    }
    private void OnEnable()
    {
        GetDependencies();
        StartCoroutine(LineOfSightRoutine());
    }
    // Update is called once per frame
    void Update()
    {
        //RangeCheck();
    }
    IEnumerator LineOfSightRoutine()
    {
        while(true)
        {
            yield return new WaitForSeconds(detectionInterval);
            DetectionCheck();
        }
    }
    public void DetectionCheck()
    {
        if(entityObject == null)
        {
            return;
        }

        Collider[] rangeChecks = Physics.OverlapSphere(entityObject.transform.position, radius, targetLayer);
        

        foreach(Collider check in rangeChecks)
        {
            if(!entitiesDetected.Contains(check.gameObject))
            {
                if(check.GetComponent<EntityInfo>())
                {
                    if(check.GetComponent<EntityInfo>() == entityInfo)
                    {
                        continue;
                    }

                    if(check.GetComponent<EntityInfo>().isAlive)
                    {
                        if(check.GetComponent<EntityInfo>().currentRoom == null || 
                            (check.GetComponent<EntityInfo>().currentRoom && check.GetComponent<EntityInfo>().currentRoom.isRevealed))
                        {
                            entitiesDetected.Add(check.gameObject);
                        }
                        
                    }
                }
            }
        }

        for(int i = 0; i < entitiesDetected.Count; i++)
        {
            bool isDetected = false;

            foreach(Collider collide in rangeChecks)
            {
                if(collide.gameObject == entitiesDetected[i])
                {
                    isDetected = true;
                }
            }

            //Out of Range
            if(!isDetected)
            {
                PlayerOutOfRangeCheck(entitiesDetected[i]);
                entitiesDetected.RemoveAt(i);
                i--;
            }
        }

        DeadCheck();
        //PlayerLineOfSightCheck();
    }

    public void PlayerLineOfSightCheck()
    {
        if(!isPlayer) { return; }
        for(int i = 0; i < entitiesDetected.Count; i++)
        {
            if(!entityInfo.CanAttack(entitiesDetected[i])) { continue; }

            var isInLineOfSight = enemiesWithinLineOfSight.Contains(entitiesDetected[i]);

            if(HasLineOfSight(entitiesDetected[i]))
            {
                if (!isInLineOfSight)
                {
                    entitiesDetected[i].GetComponent<EntityInfo>().OnPlayerLineOfSight?.Invoke(entityInfo, partyInfo);
                    enemiesWithinLineOfSight.Add(entitiesDetected[i]);
                }
            }
            else
            {
                if(isInLineOfSight)
                {
                    entitiesDetected[i].GetComponent<EntityInfo>().OnPlayerLOSBreak?.Invoke(entityInfo, partyInfo);
                    entitiesDetected.RemoveAt(i);
                    i--;
                }
            }
        }
    }

    public void PlayerOutOfRangeCheck(GameObject target)
    {
        if (!isPlayer) { return; }
        target.GetComponent<EntityInfo>().OnPlayerOutOfRange?.Invoke(entityInfo, partyInfo);
    }
    
    public void RangeCheck()
    {
        foreach(GameObject entity in entitiesDetected)
        {
            if(V3Helper.Distance(entity.transform.position, entityObject.transform.position) > radius + 2)
            {
                
                
                entitiesDetected.Remove(entity);
                MemberLosCheck(entity);


                return;
            }
        }
    }
    public void DeadCheck()
    {
        for(int i = 0; i < entitiesDetected.Count; i++)
        {
            var entity = entitiesDetected[i];
            if(entity.GetComponent<EntityInfo>())
            {
                if(!entity.GetComponent<EntityInfo>().isAlive)
                {
                    entitiesDetected.RemoveAt(i);
                    i--;

                }
            }
        }
    }
    public void AddParty(GameObject target)
    {
        if(partyInfo)
        {
            if(entityInfo.AbilityManager().canAttack.Contains(target.GetComponent<EntityInfo>().npcType))
            {
                if(!partyInfo.enemiesInLineOfSight.Contains(target))
                {
                    partyInfo.enemiesInLineOfSight.Add(target);
                }
            }
        }
    }
    public void MemberLosCheck(GameObject target)
    {
        if(partyInfo == null)
        {
            return;
        }
        
        bool canSeeTarget = false;
        foreach (GameObject member in partyInfo.members)
        {
            if (member.GetComponent<EntityInfo>().LineOfSight())
            {
                var memberLineOfSight = member.GetComponent<EntityInfo>().LineOfSight();

                if(memberLineOfSight.HasLineOfSight(target))
                {
                    canSeeTarget = true;
                }
            }
        }

        if(!canSeeTarget)
        {
            partyInfo.enemiesInLineOfSight.Remove(target);
            return;
        }
        
        
    }
    public List<EntityInfo> DetectedEntities(NPCType npcType)
    {
        var entityList = new List<EntityInfo>();

        foreach(GameObject entity in entitiesDetected)
        {
            if(entity.GetComponent<EntityInfo>() &&
                entity.GetComponent<EntityInfo>().npcType == npcType)
            {
                entityList.Add(entity.GetComponent<EntityInfo>());
            }
        }

        return entityList;
    }
    public List<EntityInfo> EntitiesLOS()
    {
        var entityList = new List<EntityInfo>();


        foreach (GameObject entity in entitiesDetected)
        {
            if(entity.GetComponent<EntityInfo>() &&
                HasLineOfSight(entity))
            {
                entityList.Add(entity.GetComponent<EntityInfo>());
            }
        }

        return entityList;
    }

    public List<EntityInfo> EntitiesWithinRange(float val)
    {
        var entityList = new List<EntityInfo>();

        foreach(GameObject entity in entitiesDetected)
        {
            if(entity.GetComponent<EntityInfo>() &&
                Vector3.Distance(entity.transform.position, entityObject.transform.position) <= val)
            {
                entityList.Add(entity.GetComponent<EntityInfo>());
            }
        }

        return entityList;
    }

    public List<EntityInfo> EntitiesLOSInRange(float range)
    {
        var entityList = new List<EntityInfo>();

        foreach(GameObject entity in entitiesDetected)
        {
            if (entity.GetComponent<EntityInfo>() &&
                Vector3.Distance(entity.transform.position, entityObject.transform.position) <= range &&
                HasLineOfSight(entity))
            {
                entityList.Add(entity.GetComponent<EntityInfo>());
            }
        }

        return entityList;
    }

    public List<EntityInfo> FilterWhiteList(NPCType npc, List<EntityInfo> entityList)
    {
        var newList = entityList;

        for(int i = 0; i < newList.Count; i++)
        {
            if(newList[i].npcType != npc)
            {
                newList.RemoveAt(i);
                i--;
            }
        }

        return newList;
    }
    public bool PartyCanSee(GameObject target)
    {
        if(!partyInfo)
        {
            return false;
        }

        if (partyInfo.enemiesInLineOfSight.Contains(target))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool HasLineOfSight(GameObject target)
    {

        var direction = V3Helper.Direction(target.transform.position, entityInfo.transform.position);
        var distance = V3Helper.Distance(target.transform.position, entityInfo.transform.position);
        if(!Physics.Raycast(entityInfo.transform.position, direction, distance, obstructionLayer))
        {
            if(!entityInfo.isInCombat)
            {
                if (distance <= radius)
                {
                    hasLineOfSightOnCurrentTarget = true;
                    return true;
                }
            }
            else
            {
                if(distance <= combatRadius)
                {
                    return true;
                }
            }
        }

        hasLineOfSightOnCurrentTarget = false;
        return false;
    }
}
