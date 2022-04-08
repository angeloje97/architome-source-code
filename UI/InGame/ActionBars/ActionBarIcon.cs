using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Architome
{
    public class ActionBarIcon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDropHandler
    {
        // Start is called before the first frame update
        public ActionBarSlot actionBarSlot;
        public ActionBarSlot hoverSlot;
        public ActionBarBehavior actionBarBehavior;


        public int childIndex;

        public void GetDependencies()
        {
            if (actionBarBehavior)
            {
                actionBarBehavior.OnNewAbility += OnNewAbility;
            }
        }

        void Start()
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (GetComponentInParent<ModuleInfo>() && GetComponentInParent<ModuleInfo>().itemBin)
            {
                transform.SetParent(GetComponentInParent<ModuleInfo>().itemBin);
            }
            hoverSlot = actionBarSlot;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (actionBarBehavior)
            {
                ReturnToActionBar();
                if (hoverSlot == null)
                {
                    ResetBar();
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (!eventData.pointerDrag) { return; }

            var abilityInfoUI = eventData.pointerDrag.GetComponent<AbilityInfoUI>();
            var abilityIcon = eventData.pointerDrag.GetComponent<ActionBarIcon>();

            if (abilityInfoUI != null)
            {
                abilityInfoUI.HandleActionBarSlot(actionBarSlot);
            }

            if (abilityIcon != null)
            {
                abilityIcon.HandleSlot(actionBarSlot);
            }
        }


        void ReturnToActionBar()
        {
            transform.position = actionBarBehavior.transform.position;
            transform.SetParent(actionBarBehavior.transform);
            transform.SetSiblingIndex(childIndex);

        }
        private void OnValidate()
        {
            if (actionBarBehavior)
            {
                childIndex = transform.GetSiblingIndex();
            }
        }
        // Update is called once per frame
        void Update()
        {

        }

        public void OnNewAbility(AbilityInfo ability)
        {
            if (GetComponent<DragAndDrop>())
            {
                GetComponent<DragAndDrop>().enabled = ability != null;
                SetCanvasGroup(true);
                ReturnToActionBar();
            }
        }

        public void SetCanvasGroup(bool val)
        {
            var canvasGroup = GetComponent<CanvasGroup>();

            if (canvasGroup == null) { return; }

            canvasGroup.blocksRaycasts = val;
            canvasGroup.interactable = val;
        }

        public void HandleSlot(ActionBarSlot slot)
        {
            if (slot == actionBarSlot) { return; }
            if (slot.spellType != actionBarBehavior.abilityInfo.abilityType2) { return; }

            if (slot.actionBarBehavior.abilityInfo)
            {
                if (slot.actionBarBehavior.abilityInfo.coolDown.charges > 0)
                {
                    var temp = slot.actionBarBehavior.abilityInfo;
                    slot.actionBarBehavior.SetActionBar(actionBarBehavior.abilityInfo);
                    actionBarBehavior.SetActionBar(temp);
                }
            }
            else
            {
                slot.actionBarBehavior.SetActionBar(actionBarBehavior.abilityInfo);
                ResetBar();
            }


        }


        public void ResetBar()
        {
            ReturnToActionBar();

            if (GetComponent<CanvasGroup>())
            {
                GetComponent<CanvasGroup>().blocksRaycasts = true;
            }

            actionBarBehavior.ResetBar();
        }
    }

}