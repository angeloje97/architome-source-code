using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mainObject;
    public Camera currentCamera;

    public bool perspective;
    public bool orthographic;

    public float cameraRange;
    public float smoothSpeed = 0.125f;

    public float zoom = 20;
    public float zoomOffset = 20;

    public float minZoom;
    public float maxZoom;

    void Start()
    {
        currentCamera = gameObject.GetComponent<Camera>();
    }
    // Update is called once per frame
    void FixedUpdate()
    {
        FollowPlayerPerspective();
        FollowPlayerOrthographic();
        HandleScrollWheel();

    }
    void FollowPlayerPerspective()
    {
        if(!perspective)
        {
            return;
        }
        var offSet = Camera.main.ScreenToViewportPoint(Input.mousePosition) * cameraRange;
        offSet = new Vector3(offSet.x - 2, zoom*1.5f, offSet.y - 2);
        var targetPosition = new Vector3(mainObject.transform.position.x, mainObject.transform.position.y, mainObject.transform.position.z);
        Vector3 desiredPosition = targetPosition + offSet;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;
    }

    void FollowPlayerOrthographic()
    {
        if(!orthographic)
        {
            return;
        }


        var offSet = Camera.main.ScreenToViewportPoint(Input.mousePosition) * cameraRange;

        offSet = new Vector3(offSet.x - 2, mainObject.transform.position.y + 20, offSet.y - 2);

        var targetPosition = new Vector3(mainObject.transform.position.x, mainObject.transform.position.y, mainObject.transform.position.z);

        Vector3 desiredPosition = targetPosition + offSet;

        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

        transform.position = smoothedPosition;

    }
    void HandleScrollWheel()
    {
        if(orthographic)
        {
            currentCamera.orthographicSize = zoom;
        }
        

        if (Input.mouseScrollDelta.y > 0)
        {
            zoomOffset -= 4;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            zoomOffset += 4;
        }

        var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, smoothSpeed);
        zoom = smoothedPosition;

        zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
}
