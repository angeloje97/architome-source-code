using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.Events;

namespace Architome
{
    public class EntitySlot : MonoBehaviour, IPointerClickHandler
    {
        public Role slotRole;
        public EntitySlotType slotType;
        public EntitySlotIcon currentIcon;


        public UnityEvent OnIcon;
        public UnityEvent OnNoIcon;


        [Serializable]
        public struct Info
        {
            public Transform content;
            public Transform borderTrans;
            public TextMeshProUGUI namePlate;
        }

        public Info info;
        EntitySlotIcon iconCheck;

        public EntityInfo entity
        {
            get
            {
                if (currentIcon == null)
                {
                    return null;
                }

                if (currentIcon.entity == null) return null;

                return currentIcon.entity;
            }
        }

        public Action<EntitySlotIcon, EntitySlotIcon> OnNewIcon { get; set; }

        public void InsertContent(Transform item)
        {
            item.SetParent(info.content);
            info.borderTrans.SetAsLastSibling();
        }

        private void Update()
        {
            HandleEvents();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!Input.GetKeyDown(KeyCode.Mouse1)) return;


        }

        void HandleEvents()
        {
            if (iconCheck != currentIcon)
            {
                OnNewIcon?.Invoke(iconCheck, currentIcon);
                HandleNewIcon(iconCheck, currentIcon);
                iconCheck = currentIcon;
            }
        }

        public void HandleNewIcon(EntitySlotIcon before, EntitySlotIcon after)
        {
            if (before)
            {
                Destroy(before.gameObject);
            }


            SetDefault();
            HandleNewEntity();
            HandleEvents();

            void HandleEvents()
            {
                var action = entity != null ? OnIcon : OnNoIcon;

                action?.Invoke();
            }

            void SetDefault()
            {
                if (info.namePlate)
                {
                    info.namePlate.text = "";
                }
            }

            void HandleNewEntity()
            {
                if (after == null) return;
                if (after.entity == null) return;

                if (info.namePlate)
                {
                    info.namePlate.text = after.entity.entityName;
                }
            }
        }

    }
}
