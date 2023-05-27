using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architome
{
    public class TargetableEntity : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] EntityInfo entity;

        bool hovering;
        bool triggeredEvent;

        void Start()
        {
            var entity = GetComponent<EntityInfo>();

            if (entity && this.entity == null)
            {
                this.entity = entity;
            }
        }

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;
        }

        void HandleEnter()
        {
            hovering = true;
            if (entity == null) return;
            if (Mouse.IsMouseOverUI()) return;

            entity.infoEvents.OnMouseHover?.Invoke(entity, true, gameObject);
            triggeredEvent = true;
        }

        public void HandleExit()
        {
            hovering = false;
            if (entity == null) return;
            if (!triggeredEvent) return;
            triggeredEvent = false;
            entity.infoEvents.OnMouseHover?.Invoke(entity, false, gameObject);
        }

        private void OnMouseEnter()
        {
            HandleEnter();
        }

        private void OnMouseOver()
        {
            
        }

        private void OnMouseExit()
        {
            HandleExit();
        }
        public void OnPointerEnter(PointerEventData eventData)
        {
            HandleEnter();
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            HandleExit();
        }


    }
}
