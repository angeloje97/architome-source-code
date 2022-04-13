using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

namespace Architome
{
    public class CameraAnchor : MonoBehaviour
    {
        public static CameraAnchor active;

        public GameObject target;

        public Vector3 anchorRotation;
        public float anchorYVal;

        public float smoothSpeed = .125f;
        public float rotationSpeed = 5;

        void GetDependencies()
        {
            ArchInput.active.OnMiddleMouse += OnMiddleMouse;
            CameraManager.active.cameraAnchor = this;
        }

        void Start()
        {
            GetDependencies();
        }

        public void OnValidate()
        {
            active = this;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            FollowTarget();
            HandleRotation();
        }
        async void OnMiddleMouse()
        {
            if (Mouse.IsMouseOverUI()) return;
            while (!Input.GetKeyUp(KeyCode.Mouse2))
            {
                await Task.Yield();
                anchorYVal += Input.GetAxis("Mouse X") * rotationSpeed;
            }
        }

        public void FollowTarget()
        {
            var smoothedPosition = Vector3.Lerp(transform.position, target.transform.position, smoothSpeed);
            transform.position = smoothedPosition;
        }


        public void HandleRotation()
        {
            Vector3 desiredRotation = new Vector3(0, anchorYVal, 0);

            anchorRotation = Vector3.Lerp(anchorRotation, desiredRotation, smoothSpeed);

            transform.rotation = Quaternion.Euler(anchorRotation);
        }

    }

}