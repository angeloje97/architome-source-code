using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
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

        public static Vector3 NegativeInfinity()
        {
            return new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        }

        public static Vector3 PositiveInfinity()
        {
            return new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        }

        public static (Vector3, float) DirectionDistance(Vector3 end, Vector3 start)
        {
            return (Vector3.Normalize(end - start), Abs(end - start));
        }

        public static float Abs(Vector3 v)
        {
            return Mathf.Sqrt(v.x * v.x + v.y * v.y + v.z * v.z);
        }

        public static Vector3 MidPoint(Vector3 end, Vector3 start)
        {
            return (end + start) / 2;
        }

        public static Vector3 MidPoint<T>(List<T> list) where T : Component
        {
            var min = PositiveInfinity();
            var max = NegativeInfinity();

            foreach (var item in list)
            {
                var pos = item.transform.position;

                min.x = pos.x < min.x ? pos.x : min.x;
                min.y = pos.y < min.y ? pos.y : min.y;
                min.z = pos.z < min.z ? pos.z : min.z;

                max.x = pos.x > max.x ? pos.x : max.x;
                max.y = pos.y > max.y ? pos.y : max.y;
                max.z = pos.z > max.z ? pos.z : max.z;
                
            }

            return (max + min) / 2;
        }


        public static float Height(Transform trans)
        {
            if (trans == null) return 0f;
            var height = 0f;


            foreach (Transform child in trans)
            {
                Debugger.InConsole(84923, $"{child.name}");
                var rectTransform = child.GetComponent<RectTransform>();
                if (rectTransform == null) continue;


                height += rectTransform.rect.height;
            }

            return height;
        }

        public static float Height(List<Transform> trans)
        {
            if (trans == null) return 0f;
            var height = 0f;

            foreach (var tran in trans)
            {
                var rect = tran.GetComponent<RectTransform>();
                if (rect == null) continue;

                height += rect.rect.height;
            }

            return height;
        }

        public static float Width(List<Transform> trans)
        {
            var width = 0f;


            foreach (var tran in trans)
            {
                var rect = tran.GetComponent<RectTransform>();
                if (rect == null) continue;

                width += rect.rect.width;
            }

            return width;
        }

        public static bool EqualVector3(Vector3 a, Vector3 b, float offset = 0f)
        {
            if (a.x < b.x - offset) return false;
            if (a.y < b.y - offset) return false;
            if (a.z < b.z - offset) return false;

            if (a.x > b.x + offset) return false;
            if (a.y > b.y + offset) return false;
            if (a.z > b.z + offset) return false;

            return true;
        }

        public static Vector3 Difference(Vector3 end, Vector3 start)
        {
            return end - start;
        }

        public static bool IsValidPosition(Vector3 end, Vector3 start, LayerMask structureLayer, LayerMask groundLayer)
        {
            if (IsObstructed(end, start, structureLayer)) return false;
            if (!IsAboveGround(end, groundLayer, 0, 5f)) return false;

            return true;
        }

        public static Transform ClosestObject(Transform source, List<Transform> listTransform)
        {
            if (listTransform == null || listTransform.Count == 0) { return null; }
            var closestDistance = 300f;

            var closestObject = listTransform[0];
            foreach (Transform tran in listTransform)
            {
                if (Distance(tran.position, source.position) < closestDistance)
                {
                    closestObject = tran;
                    closestDistance = Distance(tran.position, source.position);
                }
            }

            return closestObject;
        }

        public static Vector3 MidPoint(List<Transform> transforms)
        {
            Vector3 min = new();
            Vector3 max = new();

            foreach (var trans in transforms)
            {
                min.x = trans.position.x < min.x ? trans.position.x : min.x;
                min.y = trans.position.y < min.y ? trans.position.y : min.y;
                min.z = trans.position.z < min.z ? trans.position.z : min.z;

                max.x = trans.position.x > max.x ? trans.position.x : max.x;
                max.y = trans.position.y > max.y ? trans.position.y : max.y;
                max.z = trans.position.z > max.z ? trans.position.z : max.z;
            }

            return (min + max) / 2;
        }

        public static float MaxDistance(Vector3 point, List<Transform> transforms)
        {
            var orderedList = transforms.OrderByDescending(trans => Distance(trans.position, point)).ToList();

            return Distance(orderedList[0].position, point);
        }

        public static GameObject HitScan(Transform source, float range, LayerMask layer)
        {
            var ray = new Ray(source.position, source.forward);

            if (Physics.Raycast(ray, out RaycastHit hit, range, layer))
            {
                return hit.transform.gameObject;
            }

            return null;
        }

        public static Quaternion LerpLookAtWithAxis(Transform obj, Vector3 axis, Vector3 target = new(), float smoothening = 1f)
        {
            if (target == new Vector3())
            {
                target = obj.transform.forward;
            }

            var direction = target - obj.position;

            var toRotation = Quaternion.FromToRotation(axis, direction);

            return Quaternion.Lerp(obj.rotation, toRotation, smoothening);
        }

        public static Quaternion LerpLookAt(Transform obj, Transform target, float smoothening)
        {
            var targetRotation = Quaternion.LookRotation(target.position - obj.position);
            return Quaternion.Lerp(obj.rotation, targetRotation, smoothening);
        }

        public static Quaternion LerpLookAt(Transform obj, Vector3 target, float smoothening)
        {
            var targetRotation = Quaternion.LookRotation(target - obj.position);

            return Quaternion.Lerp(obj.rotation, targetRotation, smoothening);
        }

        public static Vector3 Dimensions(List<Transform> transforms)
        {
            var (min, max) = MinMax(transforms);

            return max - min;

        }

        public static (Vector3, Vector3) MinMax(List<Transform> transforms)
        {
            Vector3 min = new();
            Vector3 max = new();

            foreach (var trans in transforms)
            {
                min.x = trans.position.x < min.x ? trans.position.x : min.x;
                min.y = trans.position.y < min.y ? trans.position.y : min.y;
                min.z = trans.position.z < min.z ? trans.position.z : min.z;

                max.x = trans.position.x > max.x ? trans.position.x : max.x;
                max.y = trans.position.y > max.y ? trans.position.y : max.y;
                max.z = trans.position.z > max.z ? trans.position.z : max.z;
            }

            return (min, max);
        }

        public static bool PlaceEntity(EntityInfo entity, Vector3 targetPosition, LayerMask groundLayer)
        {
            var boxCollider = entity.GetComponent<BoxCollider>();
            var heightOffset = boxCollider.size.y / 2f;
            var groundPosition = GroundPosition(targetPosition, groundLayer, 1, heightOffset);
            
            if (groundPosition == targetPosition)
            {
                return false;
            }

            entity.transform.position = groundPosition;
            return true;
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

        public static Vector3 NearestNodePosition(Vector3 position)
        {
            return (Vector3) AstarPath.active.GetNearest(position).node.position;
        }

        public static Vector3 InterceptionPoint(Vector3 source, Vector3 target, LayerMask interceptionLayerMask, float near = 0f)
        {
            var (direction, distance) = DirectionDistance(target, source);
            Ray ray = new Ray(source, direction);

            if (Physics.Raycast(ray, out RaycastHit hit, distance, interceptionLayerMask))
            {
                var location = hit.point;

                if (near > 0f)
                {
                    direction = Direction(source, target);
                    location -= direction * near;
                }

                return location;
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

        public static bool IsAboveGround(Vector3 location, LayerMask groundLayer, float heightOffSet = 0f, float maxDistance = 10f)
        {

            var position = location;
            position.y += heightOffSet;

            var direction = Vector3.down;


            if (Physics.Raycast(position, direction, maxDistance, groundLayer))
            {
                return true;
            }

            return false;
        }

        public static List<Vector3> PointsAroundPosition(Vector3 position, int points, float radius, LayerMask obstructionLayer, float startingOffset = 0f)
        {
            var positions = new List<Vector3>();

            if (points <= 1)
            {
                return new List<Vector3>() { position };
            }

            var center = new GameObject("Position center");

            center.transform.SetPositionAndRotation(position, new());

            var percentPerPoint = 360f / points;

            center.transform.Rotate(startingOffset * Vector3.up);

            for (int i = 0; i < points; i++)
            {
                center.transform.Rotate(percentPerPoint * Vector3.up);

                var newPosition = center.transform.position + (center.transform.forward * radius);

                var direction = Direction(newPosition, center.transform.position);


                var ray = new Ray(newPosition, direction);

                if (Physics.Raycast(ray, out RaycastHit hit, radius, obstructionLayer))
                {
                    var hitDistance = Distance(hit.transform.position, center.transform.position);

                    newPosition = center.transform.position + (center.transform.forward * (hitDistance * .75f));
                }

                positions.Add(newPosition);
            }

            Object.Destroy(center);

            return positions;
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

            if (Physics.Raycast(source, direction, distance, obstructionLayerMask))
            {
                return true;
            }

            return false;
        }

        public static List<GameObject> TargetsWithinSight(Transform source, float radius, LayerMask targetMask, LayerMask structureMask)
        {
            var targets = new List<GameObject>();

            var detected = Physics.OverlapSphere(source.position, radius, targetMask);

            for (int i = 0; i < detected.Length; i++)
            {
                var direction = Direction(detected[i].transform.position, source.transform.position);
                var distance = Distance(detected[i].transform.position, source.transform.position);

                if (!Physics.Raycast(source.transform.position, direction, distance, structureMask))
                {
                    targets.Add(detected[i].gameObject);
                }
            }

            return targets;
        }


        public static Vector3 Sum(List<GameObject> list)
        {
            var sum = new Vector3();

            foreach (var index in list)
            {
                sum += index.transform.position;
            }

            return sum;
        }

        public static Vector3 Sum<T>(List<T> list) where T : Component
        {
            var sum = new Vector3();

            foreach (var index in list)
            {
                sum += index.transform.position;
            }

            return sum;
        }

        public static Vector3 Sum(List<Transform> transformList)
        {
            var newVector = new Vector3();

            foreach (var trans in transformList)
            {
                newVector += trans.position;
            }

            return newVector;
        }



    }

}