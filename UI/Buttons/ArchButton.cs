using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    public class ArchButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public bool interactable = true;
        public bool ignoreDoubleClick;
        public TextMeshProUGUI buttonName;
        public AudioClip rollOverSound;
        public AudioClip clickSound;


        public Action<ArchButton> OnClick;
        
        public AudioSource audioSource;
        public UnityEvent OnUnityClick;
        public UnityEvent OnDoubleClick;
        public UnityEvent OnRightClick;
        public UnityEvent MouseEnter;
        public UnityEvent MouseExit;


        [Serializable]
        public struct Info
        {
            public Image buttonColor;
            public Color enableColor;
            public Color disableColor;

            public Image background, border;
        }

        public Info info;

        bool leftClicked;

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

        public void SetName(string name)
        {
            if (buttonName == null) return;
            buttonName.text = name;
        }


        public void SetButton(string name, Action action)
        {
            if (buttonName)
            {
                buttonName.text = name;
            }

            OnClick += (ArchButton) => { action(); };
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

        
        public void SetBorder(Color color)
        {
            if (info.border == null) return;
            info.border.color = color;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!interactable) return;
            if (rollOverSound == null) return;
            audioSource.clip = rollOverSound;
            audioSource.Play();
            MouseEnter?.Invoke();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!interactable) return;
            MouseExit?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            HandleClick();
        }

        void HandleClick()
        {
            if (!interactable) return;
            //OnUnityClick?.Invoke();

            HandleRightClick();
            HandleLeftClick();

            if (clickSound == null) return;

            audioSource.clip = clickSound;
            audioSource.Play();

            async void HandleLeftClick()
            {
                if (!Input.GetKeyUp(KeyCode.Mouse0)) return;

                if (ignoreDoubleClick)
                {
                    OnUnityClick?.Invoke();
                    OnClick?.Invoke(this);
                    return;
                }

                if (leftClicked)
                {
                    leftClicked = false;
                    OnDoubleClick?.Invoke();
                    return;
                }

                OnUnityClick?.Invoke();
                OnClick?.Invoke(this);


                leftClicked = true;

                var timer = 0.5f;

                while (timer > 0)
                {
                    timer -= Time.deltaTime;

                    await Task.Yield();

                    if (leftClicked == false)
                    {
                        return;
                    }
                }

                leftClicked = false;


            }

            void HandleRightClick()
            {
                if (!Input.GetKeyUp(KeyCode.Mouse1)) return;
                OnRightClick?.Invoke();
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

    }

}
