using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;

namespace Architome
{
    public class ArchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        // Start is called before the first frame update
        public Action<ArchButton> OnClick;

        public UnityEvent OnUnityClick;

        public struct Info
        {

        }

        public Info info;

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnPointerEnter(PointerEventData eventData)
        {

        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnClick?.Invoke(this);
            OnUnityClick?.Invoke();
        }
    }

}
