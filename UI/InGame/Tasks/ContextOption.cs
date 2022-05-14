using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architome
{
    public class ContextOption : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        // Start is called before the first frame update
        public TextMeshProUGUI optionLable;

        public RectTransform rectTransform;
        public RectTransform optionTransform;

        public ContextMenu contextMenu;

        int index;

        void GetDependencies()
        {
            rectTransform = GetComponent<RectTransform>();

            optionTransform = optionLable.GetComponent<RectTransform>();
        }
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateHeight();
        }


        void UpdateHeight()
        {
            if(rectTransform.sizeDelta.y == optionTransform.rect.height) { return; }

            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, optionTransform.rect.height);
        }

        public void SetOption(string optionLable, int index)
        {
            contextMenu = GetComponentInParent<ContextMenu>();
            this.index = index;
            this.optionLable.text = optionLable;
        }

        public void SelectOption()
        {
            contextMenu.PickOption(index);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.button != PointerEventData.InputButton.Left) { return; }
            SelectOption();
        }
    }

}
