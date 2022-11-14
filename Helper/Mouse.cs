using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architome
{
    public class Mouse : MonoBehaviour
    {
        // Start is called before the first frame update

        public static List<GameObject> mouseOvers = new List<GameObject>();

        public static Vector3 CurrentPosition()
        {
            if (CameraManager.Main == null) return new Vector3();
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit))
            {
                return raycastHit.point;
            }

            return new Vector3(0, 0, 0);
        }

        public static Vector3 RelativePosition(Vector3 position)
        {
            if (CameraManager.Main == null) return new Vector3();
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);
            var direction = ray.direction;
            var distance = V3Helper.Distance(position, CameraManager.Main.transform.position);

            return CameraManager.Main.transform.position + (direction * distance);
        }
        public static GameObject CurrentHover(LayerMask layer)
        {
            if (CameraManager.Main == null) return null;
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);

            

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, layer))
            {
                return raycastHit.transform.gameObject;
                
            }

            return null;
        }

        
        public static GameObject CurrentTargetable(LayerMask layer)
        {
            if (CameraManager.Main == null) return null;
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, layer))
            {
                Debugger.InConsole(1432, $"{raycastHit.transform.gameObject}");

                return raycastHit.transform.gameObject;

            }
            return null;
        }

        public static GameObject CurrentHoverObject()
        {
            if (CameraManager.Main == null) return null;
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                return hit.transform.gameObject;
            }
            return null;
        }

        public static Vector3 CurrentPositionLayer(LayerMask layer)
        {
            if (CameraManager.Main == null) return new Vector3();
            Ray ray = CameraManager.Main.ScreenPointToRay(Input.mousePosition);


            var position = new Vector3();
            GameObject target = null;

            while (target == null)
            {
                if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, layer))
                {
                    position = raycastHit.point;
                    if (raycastHit.transform.GetComponent<Renderer>().enabled == false)
                    {
                        ray.origin = raycastHit.point + (ray.direction * 1f);
                    }
                    else
                    {
                        target = raycastHit.transform.gameObject;
                    }
                }
                else
                {
                    break;
                }

            }

            return position;
        }

        public static bool IsMouseOverUI()
        {
            var results = RayCastResults();

            for (int i = 0; i < results.Count; i++)
            {
                if (results[i].gameObject.GetComponent<IgnoreRayCast>())
                {
                    continue;
                }
                if (results[i].gameObject.GetComponentInParent<ProgressBarsBehavior>())
                {
                    continue;
                }
                return true;
            }

            return false;
        }

        public static List<GameObject> CurrentMouseOvers()
        {
            var results = RayCastResults();
            var list = new List<GameObject>();
            foreach (RaycastResult ray in results)
            {
                list.Add(ray.gameObject);
            }
            return list;
        }

        public static List<RaycastResult> RayCastResults()
        {
            List<RaycastResult> result = new List<RaycastResult>();

            PointerEventData pointerEvent = new PointerEventData(EventSystem.current);

            pointerEvent.position = Input.mousePosition;

            EventSystem.current.RaycastAll(pointerEvent, result);

            return result;
        }

        public static List<GameObject> WorldResults()
        {
            List<GameObject> result = new List<GameObject>();

            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            RaycastHit[] hits = Physics.RaycastAll(ray);

            foreach (RaycastHit hit in hits)
            {
                result.Add(hit.transform.gameObject);
            }


            return result;

        }



        public static List<GameObject> RayCastResultObjects()
        {
            var results = RayCastResults();
            var objects = new List<GameObject>();

            for (int i = 0; i < results.Count; i++)
            {
                objects.Add(results[i].gameObject);
            }

            return objects;
        }

        public static List<GameObject> AllMouseOvers()
        {
            var worldMouseOvers = WorldResults();
            var raycastResults = RayCastResultObjects();

            foreach (GameObject current in worldMouseOvers)
            {
                raycastResults.Add(current);
            }

            return raycastResults;
        }

        public static List<GameObject> WorldUI()
        {
            return mouseOvers;
        }



    }

}