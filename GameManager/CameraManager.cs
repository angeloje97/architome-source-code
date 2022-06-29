using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class CameraManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static CameraManager active;
        public Camera current;
        public static Camera Main { get { return active.current; } }
        public Camera Current { get { return current; } private set { current = value; } }


        List<Camera> entityCameras;
        List<Camera> partyCameras;

        Camera freeCamera;

        public CameraAnchor cameraAnchor;

        private void Awake()
        {
            active = this;
        }

        public void SetCurrentCamera(Camera camera)
        {
            Current = camera;
        }


        

        void TurnOffCameras()
        {
            foreach (var camera in entityCameras)
            {
                ChangeCamera(camera, false);
            }

            foreach (var camera in partyCameras)
            {
                ChangeCamera(camera, false);
            }

            ChangeCamera(freeCamera, false);
        }

        void ChangeCamera(Camera c, bool value)
        {
            c.enabled = value;
            c.tag = value ? "MainCamera" : "Untagged";
        }
    }

}