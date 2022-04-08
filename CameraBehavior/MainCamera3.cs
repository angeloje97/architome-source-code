using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCamera3 : MonoBehaviour
{
    // Start is called before the first frame update
    public Camera currentCamera;
    public static Camera active;

    public bool perspective;
    public bool orthographic;

    public float smoothSpeed;

    public float zoom = 20f;
    public float zoomOffset;

    public float minZoom = 10f;
    public float maxZoom = 20f;

    public void GetDependencies()
    {
        if(gameObject.GetComponent<Camera>())
        {
            currentCamera = gameObject.GetComponent<Camera>();
        }
    }

    void Start()
    {
        GetDependencies();
        HandleCameraType();
    }

    private void OnValidate()
    {
        GetDependencies();

        HandleCameraType();
    }

    void HandleCameraType()
    {
        if(currentCamera.orthographic != orthographic)
        {
            orthographic = currentCamera.orthographic;
            perspective = !orthographic;
        }
    }

    private void Awake()
    {
        Mouse.mainCamera = GetComponent<Camera>();
        active = GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleScrollWheelOrthoGraphic();
        HandleScrollWheelPerspective();
        HandleCameraType();
    }

    public void HandleScrollWheelOrthoGraphic()
    {
        if (!orthographic)
        {
            return;
        }

        currentCamera.orthographicSize = zoom;

        if (Input.mouseScrollDelta.y > 0)
        {
            if (!Mouse.IsMouseOverUI())
            {
                zoomOffset -= 4;
            }
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (!Mouse.IsMouseOverUI())
            {
                zoomOffset += 4;
            }

        }

        var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, smoothSpeed);

        zoom = smoothedPosition;

        zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);

        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }

    public void HandleScrollWheelPerspective()
    {
        if(!perspective)
        {
            return;
        }

        currentCamera.transform.localPosition = new Vector3(0, zoom, -(zoom*2/3));

        if (Input.mouseScrollDelta.y > 0)
        {

            if (!Mouse.IsMouseOverUI())
            {
                zoomOffset -= 4;
            }

        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (!Mouse.IsMouseOverUI())
            {
                zoomOffset += 4;
            }
        }

        var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, smoothSpeed);

        zoom = smoothedPosition;

        zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);

        zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
    }
}
