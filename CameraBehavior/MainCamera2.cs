using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera2 : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject mainObject;
    public Camera currentCamera;

    public float cameraRange;
    public float smoothSpeed = 0.125f;

    public float zoom = 20;
    public float zoomOffset = 20;

    public float minZoom;
    public float maxZoom;

    public Vector3 offSet;
    void Start()
    {
        currentCamera = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        FollowObject();
        HandleScrollWheel();
        HandleOffSet();
    }

    public void FollowObject()
    {
        var positionY = mainObject.transform.position.y + 25;
        var positionX = mainObject.transform.position.x;
        var positionZ = mainObject.transform.position.z - 25;

        var desiredPosition = new Vector3(positionX, positionY, positionZ);
        desiredPosition += offSet;

        var smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, .125f);

        transform.position = smoothedPosition;

    }

    public void HandleOffSet()
    {
        offSet = Camera.main.ScreenToViewportPoint(Input.mousePosition) * cameraRange;
        offSet = new Vector3(offSet.x - 2, 0, offSet.y - 2);
        
    }

    void HandleScrollWheel()
    {
        currentCamera.orthographicSize = zoom;
        

        if (Input.mouseScrollDelta.y > 0)
        {
            zoomOffset -= 4;
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            zoomOffset += 4;
        }

        var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, .05f);
        zoom = smoothedPosition;

        zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);
        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }


}
