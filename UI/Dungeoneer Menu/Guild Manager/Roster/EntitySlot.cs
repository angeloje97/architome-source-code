using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.EventSystems;
using System;
using TMPro;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Architome
{
    public class EntitySlot : MonoBehaviour
    {
        public Role slotRole;
        public EntitySlotType slotType;

        public EntityInfo entity;


        public UnityEvent OnIcon;
        public UnityEvent OnNoIcon;



        [Serializable]
        public struct Info
        {
            public Image iconTarget;
            public Transform borderTrans;
            public TextMeshProUGUI namePlate;
        }

        public Info info;
        EntityInfo entityCheck;

        public Action<EntityInfo, EntityInfo> OnEntityChange;
        public Action<EntitySlot> OnSlotAction, OnSlotSelect;

        public Action<bool> OnActiveChange;

        private void Update()
        {
            HandleEvents();
        }

        public void SelectSlot()
        {
            OnSlotSelect?.Invoke(this);
        }

        public void ActionSlot()
        {
            OnSlotAction?.Invoke(this);
        }


        void HandleEvents()
        {
            if (entityCheck != entity)
            {
                HandleEntityChange(entityCheck, entity);
                entityCheck = entity;
                
            }
        }

        public void HandleEntityChange(EntityInfo before, EntityInfo after)
        {
            var action = after != null ? OnIcon : OnNoIcon;
            action?.Invoke();

            HandleNewEntity(before, after);
        }

        public void HandleNewEntity(EntityInfo before, EntityInfo after)
        {
            

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
                
                if (info.namePlate)
                {
                    info.namePlate.text = after.entityName;
                } 
                info.iconTarget.sprite = after.PortraitIcon();
            }
        }

    }
}
