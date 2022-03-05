using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class Mouse : MonoBehaviour
{
    // Start is called before the first frame update

    public static List<GameObject> mouseOvers = new List<GameObject>();
    public static Camera mainCamera;

    public static Vector3 CurrentPosition()
    {
        Ray ray =  Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit raycastHit))
        {
            return raycastHit.point;
        }

        return new Vector3(0,0,0);
    }
    public static GameObject CurrentHover(LayerMask layer)
    {
        if(mainCamera == null) { return null; }

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit raycastHit,Mathf.Infinity, layer))
        {
            return raycastHit.transform.gameObject;
        }

        return null;
    }
    public static GameObject CurrentTargetable(LayerMask layer)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, layer))
        {
            Debugger.InConsole(1432, $"{raycastHit.transform.gameObject}");

            return raycastHit.transform.gameObject;
            
        }
        return null;
    }

    public static GameObject CurrentHoverObject()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            return hit.transform.gameObject;
        }
        return null;
    }

    public static Vector3 CurrentPositionLayer(LayerMask layer)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if(Physics.Raycast(ray, out RaycastHit raycastHit, Mathf.Infinity, layer))
        {
            return raycastHit.point;
        }

        return new Vector3(0,0,0);
    }

    public static bool IsMouseOverUI()
    {
        var results = RayCastResults();

        for (int i = 0; i < results.Count; i++)
        {
            if(results[i].gameObject.GetComponent<IgnoreRayCast>())
            {
                results.RemoveAt(i);
                i--;
            }
            if(results[i].gameObject.GetComponentInParent<ProgressBarsBehavior>())
            {
                results.RemoveAt(i);
                i--;
            }
        }

        
        return results.Count > 0;
    }

    public static List<GameObject> CurrentMouseOvers()
    {
        var results = RayCastResults();
        var list = new List<GameObject>();
        foreach(RaycastResult ray in results)
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

        foreach(RaycastHit hit in hits)
        {
            result.Add(hit.transform.gameObject);
        }


        return result;

    }

    

    public static List<GameObject> RayCastResultObjects()
    {
        var results = RayCastResults();
        var objects = new List<GameObject>();

        for(int i = 0; i < results.Count; i++)
        {
            objects.Add(results[i].gameObject);
        }

        return objects;
    }

    public static List<GameObject> AllMouseOvers()
    {
        var worldMouseOvers = WorldResults();
        var raycastResults = RayCastResultObjects();

        foreach(GameObject current in worldMouseOvers)
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
