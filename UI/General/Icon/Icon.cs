using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Architome
{
    public class Icon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public struct Components
        {
            public Image icon, border, background;
            public TextMeshProUGUI amount;
        }

        public Components components;

        public Action<Icon> OnSelectIcon;
        public Action<Icon> OnIconAction;
        public Action<Icon, bool> OnHoverIcon;

        public object data { get; private set; }

        public void SelectIcon()
        {
            OnSelectIcon?.Invoke(this);
        }

        public void IconAction()
        {
            OnIconAction?.Invoke(this);
        }


        //public void SetIconImage(Sprite sprite)
        //{
        //    components.icon.sprite = sprite;

        //}

        public void SetIcon(IconData data)
        {
            components.icon.sprite = data.sprite;
            components.amount.text = data.amount;
            this.data = data.data; //Confusing I know.

            components.icon.gameObject.SetActive(data.sprite != null);
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnHoverIcon?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnHoverIcon?.Invoke(this, false);
        }
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public struct IconData
        {
            public Sprite sprite;
            public string amount;
            public object data;
        }
    }
}
