using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.UI;

namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    public class ArchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        public bool interactable = true;
        public AudioClip rollOverSound;
        public AudioClip clickSound;

        public Action<ArchButton> OnClick;
        public AudioSource audioSource;
        public UnityEvent OnUnityClick;


        [Serializable]
        public struct Info
        {
            public Image buttonColor;
            public Color enableColor;
            public Color disableColor;
        }

        public Info info;

        void Start()
        {

        }

        void OnValidate()
        {
            audioSource = GetComponent<AudioSource>();

            if (info.buttonColor)
            {
                info.buttonColor.color = interactable ? info.enableColor : info.disableColor;
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetButton(bool val)
        {
            var buttonColor = val ? info.enableColor : info.disableColor;

            info.buttonColor.color = buttonColor;

            this.enabled = val;

            foreach (var button in GetComponentsInChildren<Button>(true))
            {
                button.enabled = val;
            }

            interactable = val;
        }

        

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            if (rollOverSound == null) return;
            audioSource.clip = rollOverSound;
            audioSource.Play();
            
        }

        public void OnPointerExit(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!interactable) return;
            OnClick?.Invoke(this);
            OnUnityClick?.Invoke();

            if (clickSound == null) return;

            audioSource.clip = clickSound;
            audioSource.Play();
        }
    }

}
