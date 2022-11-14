using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class UIArea : MonoBehaviour
    {
        public HashSet<IUIAreaElement> elements;
        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            var elements = GetComponentsInChildren<IUIAreaElement>();
            foreach (var element in elements)
            {
                AddUIElement(element);
            }
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void AddUIElement(IUIAreaElement element)
        {
            if (elements.Contains(element)) return;
            elements.Add(element);
            element.OnTriggerOtherElement += OnTriggerElements;
            element.OnTriggerExitElement -= OnTriggerExitElements;
        }

        public void RemoveUIElement(IUIAreaElement element)
        {
            if (!elements.Contains(element)) return;
            elements.Remove(element);
            element.OnTriggerOtherElement -= OnTriggerElements;
        }

        void AfterSetElementPosition(IUIAreaElement element)
        {
            if (element.Rect == null) return;
            if (element.IsVisible) return;
        }

        void OnTriggerElements(IUIAreaElement caller, IUIAreaElement collided)
        {
            if (caller.AttachedElements.Contains(collided)) return;
            caller.AttachedElements.Add(collided);
            caller.AfterSetPosition += AfterSetElementPosition;
        }

        void OnTriggerExitElements(IUIAreaElement caller, IUIAreaElement collided)
        {
            if (!caller.AttachedElements.Contains(collided)) return;
            caller.AttachedElements.Remove(collided);
            caller.AfterSetPosition -= AfterSetElementPosition;

        }
    }
}
