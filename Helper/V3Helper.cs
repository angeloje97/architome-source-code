using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class V3Helper
{
    // Start is called before the first frame update


    public static Vector3 Direction(Vector3 end, Vector3 start)
    {
        var direction = Vector3.Normalize(end - start);

        return direction;
    }
    public static float Distance(Vector3 end, Vector3 start)
    {
        return Abs(end - start);
    }

    public static float Abs(Vector3 v)
    {
        return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
    }

    public static Vector3 MidPoint(Vector3 end, Vector3 start)
    {
        var midPoint = new Vector3((end.x + start.x) / 2, (end.y + start.y) / 2, 0);


        return midPoint;
    }

    public static Vector3 Difference(Vector3 end, Vector3 start)
    {
        return end - start;
    }

    public static Transform ClosestObject(Transform source, List<Transform> listTransform)
    {
        if(listTransform == null || listTransform.Count == 0) { return null; }
        var closestDistance = 300f;

        var closestObject = listTransform[0];
        foreach(Transform tran in listTransform)
        {
            if(Distance(tran.position, source.position) < closestDistance)
            {
                closestObject = tran;
                closestDistance = Distance(tran.position, source.position);
            }
        }

        return closestObject;
    }

    public static Vector3 MidPoint(List<Transform> transforms)
    {
        var maxX = transforms.Max(trans => trans.position.x);
        var minX = transforms.Min(trans => trans.position.x);
        var maxY = transforms.Max(trans => trans.position.y);
        var minY = transforms.Min(trans => trans.position.y);
        var maxZ = transforms.Max(trans => trans.position.z);
        var minZ = transforms.Min(trans => trans.position.z);

        return new Vector3((maxX + minX) /2, (maxY + minY) /2 , (maxZ + minZ) / 2);
    }

    public static float MaxDistance(Vector3 point, List<Transform> transforms)
    {



        var orderedList = transforms.OrderByDescending(trans => Distance(trans.position, point)).ToList();

        return Distance(orderedList[0].position, point);
    }

    public static Quaternion LerpLookAt(Transform obj, Transform target, float smoothening)
    {
        var targetRotation = Quaternion.LookRotation(target.position - obj.position);
        return Quaternion.Lerp(obj.rotation, targetRotation, smoothening);
    }
    
    public static Vector3 Dimensions(List<Transform> transforms)
    {
        var maxX = transforms.Max(trans => trans.position.x);
        var minX = transforms.Min(trans => trans.position.x);
        var maxY = transforms.Max(trans => trans.position.y);
        var minY = transforms.Min(trans => trans.position.y);
        var maxZ = transforms.Max(trans => trans.position.z);
        var minZ = transforms.Min(trans => trans.position.z);

        return new Vector3(maxX - minX, maxY - minY, maxZ - minZ);

    }

    public static Vector3 RandomVector3(Vector3 min, Vector3 max)
    {
        return new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), Random.Range(min.z, max.z));
    }

    public static Vector2 ToTopDown(Vector3 position)
    {
        return new Vector2(position.x, position.z);
    }

    public static Vector3 ToVector3(Vector2 position)
    {
        return new Vector3(position.x, 0, position.y);
    }

    public static Vector3 InterceptionPoint(Vector3 source, Vector3 target, LayerMask interceptionLayerMask)
    {
        var direction = Direction(target, source);
        var distance = Distance(target, source);
        Ray ray = new Ray(source, direction);

        if(Physics.Raycast(ray, out RaycastHit hit, distance, interceptionLayerMask))
        {
            return hit.point;
        }
        else
        {
            return target;
        }

    }

    public static Vector3 GroundPosition(Vector3 source, LayerMask groundLayerMask, float heightOffSet = 0f, float hitHeightOffset = 0f)
    {
        var position = new Vector3(source.x, source.y + heightOffSet, source.z);
        var direction = Vector3.down;
        var distance = Mathf.Infinity;

        Ray ray = new Ray(position, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, groundLayerMask))
        {
            return new Vector3(hit.point.x, hit.point.y + hitHeightOffset, hit.point.z);
        }

        return source;
    }


    public static float HeightFromGround(Vector3 source, LayerMask groundLayerMask)
    {
        if (Physics.Raycast(new(source, Vector3.down), out RaycastHit hit, Mathf.Infinity, groundLayerMask))
        {
            return Distance(hit.point, source);
        }

        if (Physics.Raycast(new(source, Vector3.up), out RaycastHit secondHit, Mathf.Infinity, groundLayerMask))
        {
            return -Distance(secondHit.point, source);
        }

        return 0;
    }

    public static bool IsObstructed(Vector3 target, Vector3 source, LayerMask obstructionLayerMask)
    {
        var direction = Direction(target, source);
        var distance = Distance(target, source);

        if(Physics.Raycast(source, direction, distance, obstructionLayerMask))
        {
            return true; 
        }

        return false;
    }

    public static List<GameObject> TargetsWithinSight(Transform source, float radius, LayerMask targetMask, LayerMask structureMask)
    {
        var targets = new List<GameObject>();

        var detected = Physics.OverlapSphere(source.position, radius, targetMask);

        for(int i = 0; i < detected.Length; i++)
        {
            var direction = Direction(detected[i].transform.position, source.transform.position);
            var distance = Distance(detected[i].transform.position, source.transform.position);

            if(!Physics.Raycast(source.transform.position, direction, distance, structureMask))
            {
                targets.Add(detected[i].gameObject);
            }
        }

        return targets;
    }


    public static Vector3 Sum(List<GameObject> list)
    {
        var sum = new Vector3();

        foreach(var index in list)
        {
            sum += index.transform.position;
        }

        return sum;
    }

    public static Vector3 Sum(List<Transform> transformList)
    {
        return new Vector3
            (
                transformList.Sum(x => x.position.x),
                transformList.Sum(y => y.position.y),
                transformList.Sum(z => z.position.z)
            );
    }

    

}
