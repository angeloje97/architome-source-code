using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class UIAreaTester : MonoBehaviour, IUIAreaElement
    {
        public bool isVisible;
        public RectTransform rect;
        public Transform target;
        public Camera currentCamera;

        public List<IUIAreaElement> attachedElements;
        
        public bool IsVisible { get { return isVisible; } }
        public RectTransform Rect { get { return rect; } }
        public Action<IUIAreaElement> AfterSetPosition { get; set; }
        public Action<IUIAreaElement, IUIAreaElement> OnTriggerOtherElement { get; set; }
        public Action<IUIAreaElement, IUIAreaElement> OnTriggerExitElement { get; set; }
        public List<IUIAreaElement> AttachedElements { get { return attachedElements; } set { attachedElements = value; } }

        void Start()
        {
            attachedElements = new();
        }

        // Update is called once per frame
        void Update()
        {
            FollowTarget();
        }

        void FollowTarget()
        {
            if (!target) return;
            if (!currentCamera)
            {
                FindCamera();
                if (!currentCamera) return;
            }


            var position = currentCamera.WorldToScreenPoint(target.position);

            transform.position = position;
            AfterSetPosition?.Invoke(this);
        }

        void OnTriggerEnter2D(Collider2D collision)
        {
            var otherElement = collision.GetComponent<IUIAreaElement>();
            if (otherElement == null) return;
            

        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            var otherElement = collision.GetComponent<IUIAreaElement>();
            if (otherElement == null) return;
            OnTriggerExitElement?.Invoke(this, otherElement);
        }

        void FindCamera()
        {
            currentCamera = Camera.main;
        }
    }
}
