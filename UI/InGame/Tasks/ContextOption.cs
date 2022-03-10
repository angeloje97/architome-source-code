using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Architome
{
    public class ContextOption : MonoBehaviour, IPointerDownHandler
    {
        // Start is called before the first frame update
        public TextMeshProUGUI optionLable;
        private TaskModuleInfo taskModuleInfo;

        public RectTransform rectTransform;
        public RectTransform optionTransform;

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

        public void SetOption(string optionLable)
        {
            
            this.optionLable.text = optionLable;
            taskModuleInfo = GetComponentInParent<TaskModuleInfo>();
        }

        public void SelectOption()
        {
            taskModuleInfo.SelectOption(optionLable.text);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if(eventData.button != PointerEventData.InputButton.Left) { return; }
            SelectOption();
        }
    }

}
