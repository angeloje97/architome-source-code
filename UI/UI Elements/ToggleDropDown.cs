
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Architome
{
    public class ToggleDropDown : MonoBehaviour
    {
        public bool showing;
        public bool closeOnClickOut;

        [SerializeField] Image activeImage, nonActiveImage;
        [SerializeField] CanvasController targetGroup;
        [SerializeField] ToggleDropDownItem itemTemplate;
        [SerializeField] Transform itemBin;
        [SerializeField] List<ToggleDropDownItem> items;
        [SerializeField] bool update;
        

        bool isHovering;

        float clickOutTimer;

        [Serializable]
        public class ToggleData
        {
            public string option;
            public bool isOn;
        }

        public UnityEvent<Int32> OnSelectItem;

        public List<ToggleData> currentToggleData;

        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (!showing) return;
            HandleCloseOnClickOut();
        }

        void HandleCloseOnClickOut()
        {
            if (!closeOnClickOut) return;
            if (isHovering) return;

            if(clickOutTimer > 0)
            {
                clickOutTimer -= Time.deltaTime;
                return;
            }

            if (!Input.GetKeyUp(KeyCode.Mouse0)) return;
            Toggle();
        }

        private void OnValidate()
        {
            if (!update) return;
            update = false;
            UpdateGroup();
        }

        public void Toggle()
        {
            clickOutTimer = .25f;
            showing = !showing;
            UpdateGroup();
        }


        void UpdateGroup()
        {
            targetGroup.SetCanvas(showing);

            if (activeImage) activeImage.gameObject.SetActive(showing);
            if (nonActiveImage) nonActiveImage.gameObject.SetActive(!showing);

        }

        public void SelectItem(int index)
        {
            OnSelectItem?.Invoke(index);
        }

        public void SetToggleOptions(List<ToggleData> toggleDatas)
        {
            currentToggleData = toggleDatas;

            UpdateToggleDatas();
        }

        void UpdateToggleDatas()
        {
            items ??= new();

            while(items.Count < currentToggleData.Count)
            {
                items.Add(Instantiate(itemTemplate, itemBin));
            }

            while(currentToggleData.Count < items.Count)
            {
                items.RemoveAt(currentToggleData.Count);
            }


            for(int i = 0; i < currentToggleData.Count; i++)
            {
                items[i].SetDropDownItem(this, i, currentToggleData[i].option);
            }
        }

        public void ChangeHovering(bool hovering)
        {
            isHovering = hovering;
        }


    }
}
