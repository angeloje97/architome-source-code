using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class CameraManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public static CameraManager active;
        public Camera current;
        Camera previous;
        public static Camera Main { get { return active.current; } }
        public Camera Current { get { return current; } private set { current = value; } }



        List<Camera> entityCameras;
        List<Camera> partyCameras;

        Camera freeCamera;

        public CameraAnchor cameraAnchor;

        public Action<Camera> OnCameraChange;
        public Action<CameraManager> BeforeCameraChange;
        public List<Func<Task>> tasksBeforeCameraChange;


        private void Awake()
        {
            active = this;
        }

        private void Update()
        {
            if (previous != current)
            {
                previous = current;

                if (previous)
                {
                    previous.enabled = false;
                }


                if (current)
                {
                    current.enabled = true;
                }
                OnCameraChange?.Invoke(current);
            }
        }

        public async void SetCurrentCamera(Camera camera)
        {
            tasksBeforeCameraChange = new();
            BeforeCameraChange?.Invoke(this);

            foreach(var task in tasksBeforeCameraChange)
            {
                await task();
            }


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