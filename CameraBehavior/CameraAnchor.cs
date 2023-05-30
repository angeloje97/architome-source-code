using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using Architome.Enums;
using System;
using UnityEditor;

namespace Architome
{
    public class CameraAnchor : MonoBehaviour
    {
        public static CameraAnchor active;
        public CameraTarget targetType;
        public CameraAnchor check { get; set; }

        public GameObject target;

        public Vector3 anchorRotation;
        public float anchorYVal;

        public float smoothSpeed = .125f;
        public float rotationSpeed = 5;

        public Action<float, float> OnAngleChange;
        public KeyBindings keyBindData;

        bool moveCameraTimer;


        void GetDependencies()
        {
            ArchInput.active.OnMiddleMouse += OnMiddleMouse;
            CameraManager.active.cameraAnchor = this;
            keyBindData = KeyBindings.active;


            var gameManager = GameManager.active;

            if (targetType == CameraTarget.PartyCenter)
            {
                gameManager.OnNewPlayableParty += OnNewPlayableParty;
            }

            check = GetComponentInChildren<CameraAnchor>();



        }

        void Start()
        {
            GetDependencies();
            HandleEvents();
            anchorYVal = transform.eulerAngles.y;
        }
        private void Awake()
        {
            active = this;
        }

        async void HandleEvents()
        {
            float anchorYCheck = anchorYVal;
            while (this)
            {
                await Task.Yield();

                if (anchorYCheck != anchorYVal)
                {
                    OnAngleChange?.Invoke(anchorYCheck, anchorYVal);
                    anchorYCheck = anchorYVal;
                }
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            FollowTarget();
            HandleRotation();
        }

        public void OnNewPlayableParty(PartyInfo party, int index)
        {
            target = party.center;
        }

        async void OnMiddleMouse()
        {
            if (Mouse.IsMouseOverUI()) return;
            var keyCode = keyBindData.keyBinds["CameraRotator"];
            while (!Input.GetKeyUp(keyCode))
            {
                await Task.Yield();
                anchorYVal += Input.GetAxis("Mouse X") * rotationSpeed;
                
                if (anchorYVal < 0)
                {
                    anchorYVal += 360;
                    AngleJump(360);
                }

                if (anchorYVal > 360)
                {
                    anchorYVal -= 360;
                    AngleJump(-360);
                }


            }
        }


        public void EnableMoveCamera()
        {
            if (Mouse.IsMouseOverUI()) return;

            anchorYVal += Input.GetAxis("Mouse X") * rotationSpeed;

            if(anchorYVal < 0)
            {
                anchorYVal += 360;
                AngleJump(360);
            }

            if(anchorYVal > 360)
            {
                anchorYVal -= 360;
                AngleJump(-360);
            }
        }

        public void AngleJump(float amount)
        {
            anchorRotation = new Vector3(0, anchorRotation.y + amount, 0);

        }

        public void FollowTarget()
        {
            if (target == null) return;
            var smoothedPosition = Vector3.Lerp(transform.position, target.transform.position, smoothSpeed);
            transform.position = smoothedPosition;
        }


        public void HandleRotation()
        {
            Vector3 desiredRotation = new Vector3(0, anchorYVal, 0);

            anchorRotation = Vector3.Lerp(anchorRotation, desiredRotation, smoothSpeed);

            transform.rotation = Quaternion.Euler(anchorRotation);
        }

        public void DelayFollowing(float delay = 1f)
        {
            var originalTransform = target;
            target = null;


            ArchAction.Delay(() => { target = originalTransform; }, delay );
        }

        public async void DelayFollowingUntil(Task task)
        {
            var originalTransform = target;
            target = null;
            await task;

            target = originalTransform;
        }

        public void SetTarget(Transform target, bool instantTransform = false)
        {
            this.target = target.gameObject;

            if (instantTransform)
            {
                transform.position = target.position;
            }
        }

    }

}