using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Architome.Enums;

namespace Architome
{
    public class ActionBarSlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        // Start is called before the first frame update

        public AbilityInfo ability;
        public ActionBarBehavior actionBarBehavior;
        public AbilityType2 spellType;

        public GameObject iconMain;

        public void OnDrop(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) { return; }

            HandleAbility(eventData.pointerDrag);
            HandleItem(eventData.pointerDrag);
            HandleActionBarIcon(eventData.pointerDrag);



            void HandleAbility(GameObject spell)
            {
                if (!spell.GetComponent<AbilityInfoUI>()) { return; }
                Debugger.InConsole(1273, $"Trying to Drag spell");
                var info = spell.GetComponent<AbilityInfoUI>();
                info.HandleActionBarSlot(this);

                //if (info.currentSlot.slotType != spellType) { return; }

                //if (actionBarBehavior.abilityInfo != null ||
                //    (actionBarBehavior.abilityInfo && actionBarBehavior.abilityInfo.currentCharges == 0)) { return; }

                //actionBarBehavior.SetActionBar(info.abilityInfo);



                //var newObject = Instantiate(spell, transform);

                //var newInfo = newObject.GetComponent<AbilityInfoUI>();
                //newInfo.currentActionBarSlot = this;
                //var dragInfo = newObject.GetComponent<DragAndDrop>();
                //dragInfo.isDragging = false;
                //var cGroup = dragInfo.GetComponent<CanvasGroup>();
                //cGroup.alpha = 0;
                //cGroup.blocksRaycasts = true;
            }

            void HandleActionBarIcon(GameObject icon)
            {
                if (!icon.GetComponent<ActionBarIcon>()) return;

                var actionBarIcon = icon.GetComponent<ActionBarIcon>();
                actionBarIcon.HandleSlot(this);

            }

            void HandleItem(GameObject item)
            {
                if (!item.GetComponent<ItemInfo>()) { return; }
                var info = item.GetComponent<ItemInfo>();

            }
        }




        public void OnPointerEnter(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) { return; }
            HandleAbility();
            HandleIcon();
            void HandleAbility()
            {
                if (!eventData.pointerDrag.GetComponent<AbilityInfoUI>()) return;
                var info = eventData.pointerDrag.GetComponent<AbilityInfoUI>();
                info.currentActionBarHover = this;
                var rectTransform = eventData.pointerDrag.GetComponent<RectTransform>();
                rectTransform.sizeDelta = GetComponent<RectTransform>().sizeDelta * 1.25f;
            }
            void HandleIcon()
            {
                if (!eventData.pointerDrag.GetComponent<ActionBarIcon>()) { return; }
                var iconInfo = eventData.pointerDrag.GetComponent<ActionBarIcon>();
                iconInfo.hoverSlot = this;
            }
        }
        public void OnPointerExit(PointerEventData eventData)
        {
            if (eventData.pointerDrag == null) { return; }
            HandleAbility();
            HandleIcon();

            void HandleAbility()
            {
                if (!eventData.pointerDrag.GetComponent<AbilityInfoUI>()) return;
                var info = eventData.pointerDrag.GetComponent<AbilityInfoUI>();
                info.currentActionBarHover = null;
                if (info.currentSlot == null) { return; }
                var rectTransform = info.GetComponent<RectTransform>();

                rectTransform.sizeDelta = info.currentSlot.GetComponent<RectTransform>().sizeDelta * 1.25f;
            }
            void HandleIcon()
            {
                if (!eventData.pointerDrag.GetComponent<ActionBarIcon>()) { return; }
                var iconInfo = eventData.pointerDrag.GetComponent<ActionBarIcon>();
                if (iconInfo.hoverSlot)
                {
                    iconInfo.hoverSlot = null;
                }

            }
        }

        private void OnValidate()
        {
            if (GetComponent<ActionBarBehavior>())
            {
                actionBarBehavior = GetComponent<ActionBarBehavior>();
            }
        }

        public void GetDependencies()
        {
            if (actionBarBehavior)
            {
                actionBarBehavior.OnChargeChange += OnChargeChange;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        public void OnChargeChange(AbilityInfo ability, int charge)
        {
            iconMain.GetComponent<DragAndDrop>().enabled = charge == ability.coolDown.maxCharges;
        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}