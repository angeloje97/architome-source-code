using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
namespace Architome
{
    public class Entity : MonoBehaviour
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
            var entities = FindObjectsOfType<EntityInfo>();

            return entities.Where(entity => entity.currentRoom == room).ToList();
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

        

        public static List<EntityInfo> EntitesWithinLOS(Vector3 position, float radius)
        {
            var obstructionLayer = GMHelper.LayerMasks().structureLayerMask;

            return EntitiesWithinRange(position, radius)
                .Where(entity => !V3Helper.IsObstructed(entity.transform.position, position, obstructionLayer))
                .ToList();
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
            if (IsEntity(objectCheck))
            {
                var info = objectCheck.GetComponent<EntityInfo>();

                if (GMHelper.GameManager().playableEntities.Contains(info))
                {
                    return true;
                }
            }

            return false;
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
    }

}