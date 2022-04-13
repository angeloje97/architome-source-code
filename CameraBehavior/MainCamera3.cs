using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MainCamera3 : MonoBehaviour
    {
        // Start is called before the first frame update
        public Camera currentCamera;
        public CameraAnchor anchor;
        public static Camera active { get; set; }

        public bool perspective;
        public bool orthographic;

        public float smoothSpeed;

        public float zoom = 20f;
        public float zoomOffset;

        public float minZoom = 10f;
        public float maxZoom = 20f;

        public void GetDependencies()
        {
            currentCamera = GetComponent<Camera>();
            anchor = GetComponentInParent<CameraAnchor>();
            
        }

        void Start()
        {
            GetDependencies();
            HandleCameraType();
            ArchInput.active.OnScrollWheel += OnScrollWheel;
            CameraManager.active.SetCurrentCamera(currentCamera);
        }

        private void OnValidate()
        {
            GetDependencies();

            HandleCameraType();
        }

        void HandleCameraType()
        {
            if (currentCamera.orthographic != orthographic)
            {
                orthographic = currentCamera.orthographic;
                perspective = !orthographic;
            }
        }

        void OnScrollWheel(float value)
        {
            if (Mouse.IsMouseOverUI()) return;
            zoomOffset += -value*4;
        }

        private void Awake()
        {
            active = GetComponent<Camera>();
        }

        // Update is called once per frame
        void Update()
        {
            HandleScrollWheelOrthoGraphic();
            HandleScrollWheelPerspective();
            HandleCameraType();
            LookAtAnchor();
        }

        void LookAtAnchor()
        {
            if (!anchor) return;

            transform.LookAt(anchor.transform);
        }

        public void HandleScrollWheelOrthoGraphic()
        {
            if (!orthographic)
            {
                return;
            }

            currentCamera.orthographicSize = zoom;

            var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, smoothSpeed);

            zoom = smoothedPosition;

            zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);

            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }

        public void HandleScrollWheelPerspective()
        {
            if (!perspective)
            {
                return;
            }

            currentCamera.transform.localPosition = new Vector3(0, zoom, -(zoom * 2 / 3));


            var smoothedPosition = Mathf.Lerp(zoom, zoomOffset, smoothSpeed);

            zoom = smoothedPosition;

            zoomOffset = Mathf.Clamp(zoomOffset, minZoom, maxZoom);

            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        }
    }

}