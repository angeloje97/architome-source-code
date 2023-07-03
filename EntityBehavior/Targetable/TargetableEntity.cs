using Architome.Enums;
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
        ArchInput archInput;

        readonly static HashSet<ArchInputMode> modes = new HashSet<ArchInputMode>() {
            ArchInputMode.Adventure,
            ArchInputMode.Colony,
        };

        void Start()
        {
            var entity = GetComponent<EntityInfo>();

            if (entity && this.entity == null)
            {
                this.entity = entity;
            }

            entity = GetComponentInParent<EntityInfo>();

            if(entity && this.entity == null)
            {
                this.entity = entity;
            }

            archInput = ArchInput.active;
        }

        public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;
        }

        void HandleEnter()
        {
            if (!modes.Contains(archInput.Mode)) return;
            hovering = true;
            if (entity == null) return;
            if (Mouse.IsMouseOverUI()) return;

            entity.infoEvents.OnMouseHover?.Invoke(entity, true, gameObject);
            triggeredEvent = true;
        }

        public void HandleExit()
        {
            if (!hovering) return;
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
