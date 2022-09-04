using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class LineOfSight : MonoBehaviour
    {
        public GameObject entityObject;
        public EntityInfo entityInfo;
        public PartyInfo partyInfo;
        public AIBehavior behavior;
        public CharacterInfo character;

        //public List<GameObject> entitiesDetected;
        public List<EntityInfo> enemiesWithinLineOfSight;
        public List<EntityInfo> entitiesWithinLineOfSight;

        public LayerMask targetLayer { get; set; }
        public LayerMask obstructionLayer { get; set; }

        public bool isPlayer;
        public bool hasLineOfSightOnCurrentTarget;

        public float radius;
        public float combatRadius;
        public float detectionInterval;
        public void GetDependencies()
        {
            behavior = GetComponentInParent<AIBehavior>();
            entityInfo = GetComponentInParent<EntityInfo>();
            var layerMasksData = LayerMasksData.active;


            if (entityInfo)
            {
                entityObject = entityInfo.gameObject;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
                entityInfo.OnLifeChange += OnLifeChange;
                character = entityInfo.CharacterInfo();
            }
            else
            {
                return;
            }

            if (GetComponentInParent<PartyInfo>())
            {
                partyInfo = GetComponentInParent<PartyInfo>();
            }

            if (entityInfo)
            {
                switch (entityInfo.entityControlType)
                {
                    case EntityControlType.EntityControl:
                        isPlayer = true;
                        break;
                    case EntityControlType.PartyControl:
                        isPlayer = true;
                        break;
                }
            }

            if (layerMasksData)
            {
                obstructionLayer = layerMasksData.structureLayerMask;
                targetLayer = layerMasksData.entityLayerMask;
            }

            var gameManager = GMHelper.GameManager();
            if (gameManager == null) return;

            if (GMHelper.Difficulty())
            {
                if (isPlayer)
                {
                    radius = GMHelper.Difficulty().settings.playerDetectionRange;
                    if (combatRadius == 0) { combatRadius = radius; }
                    detectionInterval = .5f;
                }
                else
                {
                    radius = GMHelper.Difficulty().settings.npcDetectionRange;

                    if (combatRadius == 0)
                    {
                        combatRadius = radius;
                    }
                }
            }

            if (GMHelper.GameManager().playableEntities.Contains(entityInfo))
            {
                isPlayer = true;
            }
        }

        private void Start()
        {
            GetDependencies();
            LineOfSightRoutine();
        }
        // Update is called once per frame
        void Update()
        {
            //RangeCheck();
        }
        async void LineOfSightRoutine()
        {
            while (true)
            {
                await Task.Delay((int)(1000 * detectionInterval));
                if (!entityInfo.isAlive) continue;
                //DetectionCheck();
                if (!RoomIsRevealed()) continue;
                Scan();
            }
        }

        void OnLifeChange(bool isAlive)
        {
            entitiesWithinLineOfSight.Clear();
        }

        void OnChangeNPCType(NPCType before, NPCType after)
        {
            entitiesWithinLineOfSight.Clear();
            var entities = Entity.EntitiesWithinRange(entityInfo.transform.position, radius);

            ArchAction.Delay(() => {

                foreach (var entity in entities)
                {
                    if (entity == null) continue;
                    var scanner = entity.GetComponentInChildren<LineOfSight>();

                    if (scanner == null) continue;

                    scanner.RemoveEntityFromLOS(entityInfo);
                }

            }, .125f);
        }
        void Scan()
        {
            //var entities = Entity.EntitiesWithinLOS(entityObject.transform.position, radius);

            var entities = Physics.OverlapSphere(entityObject.transform.position, radius, targetLayer);
            var entitiesScanned = new List<EntityInfo>();

            foreach (var entity in entities)
            {
                var info = entity.GetComponent<EntityInfo>();

                if (info == null) continue;
                if (!info.isAlive) continue;
                //if (entitiesWithinLineOfSight.Contains(entity.gameObject)) continue;

                if (InvisibilityAffected(info)) continue;

                var distance = V3Helper.Distance(info.transform.position, entityObject.transform.position);
                var direction = V3Helper.Direction(info.transform.position, entityObject.transform.position);

                if (!CharacterIsFacing(info, distance)) continue;

                if (Physics.Raycast(entityObject.transform.position, direction, distance, obstructionLayer)) continue;

                if (!entitiesWithinLineOfSight.Contains(info))
                {
                    behavior.events.OnSightedEntity?.Invoke(info);
                    entitiesWithinLineOfSight.Add(info);
                }

                //entitiesWithinLineOfSight.Add(entity.gameObject);
                entitiesScanned.Add(info);
            }

            for (int i = 0; i < entitiesWithinLineOfSight.Count; i++)
            {
                var entity = entitiesWithinLineOfSight[i].GetComponent<EntityInfo>();

                if (!entity.isAlive)
                {
                    entitiesWithinLineOfSight.RemoveAt(i);
                    i--; continue;
                }

                if (!entitiesScanned.Contains(entity))
                {
                    entitiesWithinLineOfSight.RemoveAt(i);
                    i--;
                }

            }

            bool CharacterIsFacing(EntityInfo target, float distance)
            {
                if (entityInfo.rarity == EntityRarity.Player) return true;
                if (entityInfo.isInCombat) return true;
                if (character == null) return true;
                if (distance <= radius * .35f) return true;


                return character.IsFacing(target.transform.position);
            }

            bool InvisibilityAffected(EntityInfo entity)
            {
                if (!entity.states.Contains(EntityState.Invisible)) return false;
                if (entityInfo.CanHelp(entity.gameObject)) return false;

                var distance = V3Helper.Distance(entity.transform.position, transform.position);

                if (distance <= radius * .25f)
                {
                    return false;
                }

                return true;
            }
        }

        

        public bool RoomIsRevealed()
        {
            if (entityInfo.rarity == EntityRarity.Player) return true;
            if (entityInfo.currentRoom == null) return true;
            return entityInfo.currentRoom.isRevealed;
        }



        void RemoveEntityFromLOS(EntityInfo target)
        {
            if (!entitiesWithinLineOfSight.Contains(target)) return;

            entitiesWithinLineOfSight.Remove(target);
        }

        //public void DetectionCheck()
        //{
        //    if(entityObject == null)
        //    {
        //        return;
        //    }

        //    Collider[] rangeChecks = Physics.OverlapSphere(entityObject.transform.position, radius, targetLayer);
        //    var entities = Entity.EntitesWithinLOS(entityObject.transform.position, radius);


        //    foreach(Collider check in rangeChecks)
        //    {
        //        if(!entitiesDetected.Contains(check.gameObject))
        //        {
        //            if(check.GetComponent<EntityInfo>())
        //            {
        //                if(check.GetComponent<EntityInfo>() == entityInfo)
        //                {
        //                    continue;
        //                }

        //                if(check.GetComponent<EntityInfo>().isAlive)
        //                {
        //                    if(check.GetComponent<EntityInfo>().currentRoom == null || 
        //                        (check.GetComponent<EntityInfo>().currentRoom && check.GetComponent<EntityInfo>().currentRoom.isRevealed))
        //                    {
        //                        entitiesDetected.Add(check.gameObject);
        //                    }

        //                }
        //            }
        //        }
        //    }

        //    for(int i = 0; i < entitiesDetected.Count; i++)
        //    {
        //        bool isDetected = false;

        //        foreach(Collider collide in rangeChecks)
        //        {
        //            if(collide.gameObject == entitiesDetected[i])
        //            {
        //                isDetected = true;
        //            }
        //        }

        //        //Out of Range
        //        if(!isDetected)
        //        {
        //            PlayerOutOfRangeCheck(entitiesDetected[i]);
        //            entitiesDetected.RemoveAt(i);
        //            i--;
        //        }
        //    }

        //    DeadCheck();
        //    //PlayerLineOfSightCheck();
        //}

        //public void PlayerLineOfSightCheck()
        //{
        //    if(!isPlayer) { return; }
        //    for(int i = 0; i < entitiesDetected.Count; i++)
        //    {
        //        if(!entityInfo.CanAttack(entitiesDetected[i])) { continue; }

        //        var isInLineOfSight = enemiesWithinLineOfSight.Contains(entitiesDetected[i]);

        //        if(HasLineOfSight(entitiesDetected[i]))
        //        {
        //            if (!isInLineOfSight)
        //            {
        //                entitiesDetected[i].GetComponent<EntityInfo>().OnPlayerLineOfSight?.Invoke(entityInfo, partyInfo);
        //                enemiesWithinLineOfSight.Add(entitiesDetected[i]);
        //            }
        //        }
        //        else
        //        {
        //            if(isInLineOfSight)
        //            {
        //                entitiesDetected[i].GetComponent<EntityInfo>().OnPlayerLOSBreak?.Invoke(entityInfo, partyInfo);
        //                entitiesDetected.RemoveAt(i);
        //                i--;
        //            }
        //        }
        //    }
        //}

        //public void PlayerOutOfRangeCheck(GameObject target)
        //{
        //    if (!isPlayer) { return; }
        //    target.GetComponent<EntityInfo>().OnPlayerOutOfRange?.Invoke(entityInfo, partyInfo);
        //}

        //public void RangeCheck()
        //{
        //    foreach(GameObject entity in entitiesDetected)
        //    {
        //        if(V3Helper.Distance(entity.transform.position, entityObject.transform.position) > radius + 2)
        //        {


        //            entitiesDetected.Remove(entity);
        //            MemberLosCheck(entity);


        //            return;
        //        }
        //    }
        //}
        //public void DeadCheck()
        //{
        //    for(int i = 0; i < entitiesDetected.Count; i++)
        //    {
        //        var entity = entitiesDetected[i];
        //        if(entity.GetComponent<EntityInfo>())
        //        {
        //            if(!entity.GetComponent<EntityInfo>().isAlive)
        //            {
        //                entitiesDetected.RemoveAt(i);
        //                i--;

        //            }
        //        }
        //    }
        //}
        public void MemberLosCheck(GameObject target)
        {
            if (partyInfo == null)
            {
                return;
            }

            bool canSeeTarget = false;
            foreach (var member in partyInfo.members)
            {
                {
                    var memberLineOfSight = member.LineOfSight();

                    if (memberLineOfSight.HasLineOfSight(target))
                    {
                        canSeeTarget = true;
                    }
                }
            }

        }
        public List<EntityInfo> DetectedEntities(NPCType npcType)
        {
            var entityList = new List<EntityInfo>();

            foreach (var entity in entitiesWithinLineOfSight)
            {
                if (entity == null) continue;
                if (entity.npcType == npcType)
                {
                    entityList.Add(entity);
                }
            }

            return entityList;
        }

        public List<EntityInfo> DetectedAllies()
        {
            var entityList = new List<EntityInfo>();
            var listCheck = partyInfo ? partyInfo.members : entitiesWithinLineOfSight;

            foreach (var entity in listCheck)
            {
                var info = entity.GetComponent<EntityInfo>();

                if (info.npcType == entityInfo.npcType)
                {
                    entityList.Add(info);
                }
            }


            return entityList;

        }

        public List<EntityInfo> EntitiesLOS()
        {
            var entities = new List<EntityInfo>();

            foreach (var entity in entitiesWithinLineOfSight)
            {
                var info = entity.GetComponent<EntityInfo>();
                if (info == null) continue;
                entities.Add(info);
            }

            return entities;
        }

        public List<EntityInfo> EntitiesWithinRange(float val)
        {
            var entityList = new List<EntityInfo>();

            foreach (var entity in entitiesWithinLineOfSight)
            {
                var info = entity.GetComponent<EntityInfo>();
                if (info == null) continue;

                var distance = V3Helper.Distance(info.transform.position, entityInfo.transform.position);
                if (distance > val) continue;
                entityList.Add(info);
            }


            //foreach(GameObject entity in entitiesDetected)
            //{
            //    if(entity.GetComponent<EntityInfo>() &&
            //        Vector3.Distance(entity.transform.position, entityObject.transform.position) <= val)
            //    {
            //        entityList.Add(entity.GetComponent<EntityInfo>());
            //    }
            //}

            return entityList;
        }

        public List<EntityInfo> EntitiesLOSInRange(float range)
        {
            var entityList = new List<EntityInfo>();

            foreach (var entity in entitiesWithinLineOfSight)
            {
                var info = entity.GetComponent<EntityInfo>();
                if (info == null) continue;
                var distance = V3Helper.Distance(info.transform.position, entityInfo.transform.position);
                if (distance > range) continue;

                entityList.Add(info);
            }

            return entityList;
        }

        public List<EntityInfo> FilterWhiteList(NPCType npc, List<EntityInfo> entityList)
        {
            var newList = entityList;

            for (int i = 0; i < newList.Count; i++)
            {
                if (newList[i].npcType != npc)
                {
                    newList.RemoveAt(i);
                    i--;
                }
            }

            return newList;
        }
        public bool PartyCanSee(GameObject target)
        {
            if (!partyInfo)
            {
                return false;
            }
            else
            {
                return false;
            }
        }

        public bool HasLineOfSight(Vector3 location)
        {
            var direction = V3Helper.Direction(location, entityInfo.transform.position);
            var distance = V3Helper.Distance(location, entityInfo.transform.position);

            if (!Physics.Raycast(entityInfo.transform.position, direction, distance, GMHelper.LayerMasks().structureLayerMask))
            {
                return true;
            }

            return false;
        }
        public bool HasLineOfSight(GameObject target)
        {
            if (target == null) return false;
            var direction = V3Helper.Direction(target.transform.position, entityInfo.transform.position);
            var distance = V3Helper.Distance(target.transform.position, entityInfo.transform.position);
            if (!Physics.Raycast(entityInfo.transform.position, direction, distance, obstructionLayer))
            {
                if (!entityInfo.isInCombat)
                {
                    if (distance <= radius)
                    {
                        hasLineOfSightOnCurrentTarget = true;
                        return true;
                    }
                }
                else
                {
                    if (distance <= combatRadius)
                    {
                        return true;
                    }
                }
            }

            hasLineOfSightOnCurrentTarget = false;
            return false;
        }
    }
    
}