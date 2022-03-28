using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome;
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

    public static List<EntityInfo> EntitiesWithinRange(GameObject target, float radius)
    {
        var entityList = new List<EntityInfo>();

        Collider[] entitiesWithinRange = Physics.OverlapSphere(target.transform.position, radius, GMHelper.LayerMasks().entityLayerMask);

        foreach(var i in entitiesWithinRange)
        {
            if (!i.GetComponent<EntityInfo>()) { continue; }

            var info = i.GetComponent<EntityInfo>();

            if(!IsObstructed(target, i.gameObject))
            {
                entityList.Add(info);
            }
        }

        return entityList;
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
        if(objectCheck.GetComponent<EntityInfo>() || objectCheck.GetComponentInParent<EntityInfo>())
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
        if(IsEntity(objectCheck))
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

        if(Physics.Raycast(sourceTarget.transform.position, direction, distance, GMHelper.LayerMasks().structureLayerMask))
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
