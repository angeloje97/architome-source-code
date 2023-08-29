using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class Entity
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public static EntityInfo EntityInfo(GameObject entityObject)
        {

            if (entityObject.GetComponent<EntityInfo>())
            {
                Debugger.InConsole(2, $"entity object has entity info? {entityObject.GetComponent<EntityInfo>()}");
                return entityObject.GetComponent<EntityInfo>();

            }
            else
            {
                return null;
            }
        }

        public static List<EntityInfo> EntitiesFromRoom(RoomInfo room)
        {
            return room.entities.EntitiesInRoom;
        }

        public static RoomInfo Room(Vector3 point)
        {
            List<Ray> rays = new();

            rays.Add(new Ray(point, Vector3.down));
            rays.Add(new Ray(point, Vector3.left));
            rays.Add(new Ray(point, Vector3.right));
            rays.Add(new Ray(point, Vector3.forward));
            rays.Add(new Ray(point, Vector3.back));


            foreach (var ray in rays)
            {
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GMHelper.LayerMasks().structureLayerMask))
                {
                    if (hit.transform.GetComponentInParent<RoomInfo>())
                    {
                        return hit.transform.GetComponentInParent<RoomInfo>();
                    }
                }
            }

            return null;

        }

        public static bool PlayerIsInRoom(RoomInfo room)
        {
            var entitiesInRoom = GameManager.active.playableEntities.Where(entity => entity.currentRoom == room).ToList();

            return entitiesInRoom.Count > 0;
        }

        public static Vector3 PositionFacing(EntityInfo entity)
        {
            var character = entity.CharacterInfo();
            if (character == null)
            {
                return entity.transform.position;
            }

            return character.transform.position + (Vector3.forward * 2);
        }

        

        public static List<EntityInfo> EntitiesWithinRange(Vector3 location, float radius)
        {
            var entityList = new List<EntityInfo>();

            Collider[] entitiesWithinRange = Physics.OverlapSphere(location, radius, GMHelper.LayerMasks().entityLayerMask);

            entityList = entitiesWithinRange.Select(entity => entity.GetComponent<EntityInfo>()).OrderBy(entity => Vector3.Distance(entity.transform.position, location)).ToList();

            return entityList;
        }

        public static void ProcessEntitiesInRange(Vector3 location, float radius, LayerMask entityLayer, Action<EntityInfo> process)
        {
            Collider[] entitiesWithinRange = Physics.OverlapSphere(location, radius, entityLayer);

            foreach (var collider in entitiesWithinRange)
            {
                var entity = collider.GetComponent<EntityInfo>();
                if (entity == null) continue;

                process(entity);
            }
        }

        public static void ProcessEntitiesWithinLOS(Vector3 location, float radius, LayerMask entityLayer, LayerMask structureLayer, Action<EntityInfo> process)
        {
            Collider[] entitiesWithinRange = Physics.OverlapSphere(location, radius, entityLayer);

            foreach (var collider in entitiesWithinRange)
            {
                var entity = collider.GetComponent<EntityInfo>();
                if (entity == null) continue;

                if(V3Helper.IsObstructed(entity.transform.position, location, structureLayer))
                {
                    continue;
                }

                process(entity);
            }
        }

        public static void ProcessEntitiesInRange(Vector3 location, float radius, bool requiresLOS, LayerMask structureLayer, LayerMask entityLayer, Action<EntityInfo> process)
        {
            Collider[] entitiesWithinRange = Physics.OverlapSphere(location, radius, entityLayer);

            foreach (var collider in entitiesWithinRange)
            {
                var entity = collider.GetComponent<EntityInfo>();
                if (entity == null) continue;

                if (requiresLOS)
                {
                    var direction = V3Helper.Direction(entity.transform.position, location);
                    var distance = V3Helper.Distance(entity.transform.position, location);

                    var ray = new Ray(location, direction);

                    if(Physics.Raycast(ray, distance, structureLayer))
                    {
                        continue;
                    }
                }

                process(entity);
            }

            //entityList = entitiesWithinRange.Select(entity => entity.GetComponent<EntityInfo>()).OrderBy(entity => Vector3.Distance(entity.transform.position, location)).ToList();

        }



        public static List<EntityInfo> EntitiesWithinLOS(Vector3 position, float radius)
        {
            var obstructionLayer = GMHelper.LayerMasks().structureLayerMask;
            var entityLayer = GMHelper.LayerMasks().entityLayerMask;

            //var obstructionLayer = GMHelper.LayerMasks().structureLayerMask;
            var entitiesInSight = new List<EntityInfo>();

            var entitiesWithinRange = Physics.OverlapSphere(position, radius, entityLayer);

            foreach (var collider in entitiesWithinRange)
            {
                if (!V3Helper.IsObstructed(collider.transform.position, position, obstructionLayer))
                {
                    entitiesInSight.Add(collider.GetComponent<EntityInfo>());
                }
            }

            return entitiesInSight.OrderBy(entity => Vector3.Distance(entity.transform.position, position)).ToList();

        }

        public static List<EntityInfo> ToEntities(List<GameObject> entityObjects)
        {
            List<EntityInfo> entities = new();
            foreach (var entity in entityObjects)
            {
                if (entity.GetComponent<EntityInfo>())
                {
                    entities.Add(entity.GetComponent<EntityInfo>());
                }
            }

            return entities;
        }

        public static List<ItemInfo> ItemsFromAllPlayableEntities(ItemData itemData)
        {
            var playableEntities = PlayableEntities();
            int count = 0;
            var items = new List<ItemInfo>();

            foreach(var entity in playableEntities)
            {
                var inventory = entity.Inventory();
                if (inventory == null) continue;

                inventory.ItemsInInventory((ItemInfo item) => {
                    if (count >= itemData.amount) return;
                    items.Add(item);
                    count += item.currentStacks;
                }, itemData);
            }

            if(count != itemData.amount)
            {
                items = new();
            }

            return items;
        }

        public static bool IsOfEntity(GameObject objectCheck)
        {
            if (objectCheck.GetComponent<EntityInfo>() || objectCheck.GetComponentInParent<EntityInfo>())
            {
                return true;
            }

            return false;
        }

        public static bool IsEntity(GameObject objectCheck)
        {
            return objectCheck.GetComponent<EntityInfo>() != null;

        }

        public static bool IsPlayer(GameObject objectCheck)
        {
            var info = objectCheck.GetComponent<EntityInfo>();

            if (info)
            {
                if (info.rarity == EntityRarity.Player)
                {
                    return true;
                }
            }


            return false;
        }

        public static bool IsPlayer(EntityInfo entity)
        {
            return entity.rarity == EntityRarity.Player;
        }

        public static bool IsObstructed(GameObject endTarget, GameObject sourceTarget)
        {
            var distance = Vector3.Distance(endTarget.transform.position, sourceTarget.transform.position);
            var direction = V3Helper.Direction(endTarget.transform.position, sourceTarget.transform.position);

            if (Physics.Raycast(sourceTarget.transform.position, direction, distance, GMHelper.LayerMasks().structureLayerMask))
            {
                return true;
            }


            return false;
        }

        public static List<EntityInfo> PlayableEntities()
        {
            return GMHelper.GameManager().playableEntities;
        }

        public static List<GameObject> LiveEntityObjects(List<GameObject> entityObjects)
        {
            return entityObjects.FindAll(entityObject => entityObject.GetComponent<EntityInfo>().isAlive);
        }

        public static List<EntityInfo> LiveEntities(List<EntityInfo> entities)
        {
            var liveEntities = new List<EntityInfo>();
            foreach (var entity in entities)
            {
                if (entity.isAlive)
                {
                    liveEntities.Add(entity);
                }
            }

            return liveEntities;
        }
    }

}