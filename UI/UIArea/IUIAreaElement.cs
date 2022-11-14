using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public interface IUIAreaElement 
    {
        public bool IsVisible { get; }
        public RectTransform Rect { get; }
        public Action<IUIAreaElement> AfterSetPosition { get; set; }
        public Action<IUIAreaElement, IUIAreaElement> OnTriggerOtherElement { get; set; }
        public Action<IUIAreaElement, IUIAreaElement> OnTriggerExitElement { get; set; }

        public List<IUIAreaElement> AttachedElements { get; set; }




    }
}
