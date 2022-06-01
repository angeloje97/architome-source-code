using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Architome.Enums;

namespace Architome
{
    public class EntitySlotIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {



        public EntityInfo entity;

        public EntitySlot currentSlot;

        public Action<EntitySlotIcon> OnRightClick;
        public Action<EntitySlotIcon> OnLeftClick;

        [SerializeField] bool isAvailable = true;
        protected bool canDestroy;

        [Serializable]
        public struct Info
        {
            public Transform dragAndDropScope;
            public Image iconTarget;

        }

        public Info info;
        
        public void ReturnToSlot()
        {
            if (currentSlot == null) return;

            currentSlot.InsertContent(transform);
            

            transform.position = currentSlot.transform.position;

            GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta;

            transform.localScale = new(1, 1, 1);
        }

        public void SetAvailable(bool val)
        {
            if (isAvailable == val) return;
            isAvailable = val;

            info.iconTarget.color = val ? Color.white : Color.black;
        }


        public void SetIcon(EntityInfo entity, EntitySlot slot)
        {
            info.iconTarget.sprite = entity.entityPortrait;
            this.entity = entity;

            currentSlot = slot;

            slot.currentIcon = this;

            ReturnToSlot();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //ReturnToSlot();
        }
        

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isAvailable) return;
            HandleLeftClick();
            HandleRightClick();
        }

        public void HandleLeftClick()
        {
            if (!Input.GetKeyDown(KeyCode.Mouse0)) return;

            OnLeftClick?.Invoke(this);
        }

        public void HandleRightClick()
        {
            if (!Input.GetKey(KeyCode.Mouse1)) return;

            OnRightClick?.Invoke(this);


        }
    }
}
