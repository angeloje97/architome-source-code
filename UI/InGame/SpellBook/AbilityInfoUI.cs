using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Architome.Enums;
using UnityEditor.PackageManager;
using System;

namespace Architome
{
    [RequireComponent(typeof(ToolTipElement))]
    public class AbilityInfoUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
    {
        public AbilityInfo abilityInfo;
        public SpellBookSlot currentSlot;
        public ActionBarSlot currentActionBarSlot;
        public ActionBarSlot currentActionBarHover;

        [SerializeField] Image image;
        [SerializeField] DragAndDrop dragAndDrop;

        ToolTipElement toolTipHandler;
        
        void Start()
        {
            toolTipHandler = GetComponent<ToolTipElement>();

            if (toolTipHandler)
            {
                toolTipHandler.OnCanShowCheck += HandleCanShowCheck;
            }
        }

        void HandleCanShowCheck(ToolTipElement element)
        {
            if(abilityInfo == null)
            {
                element.checks.Add(false);
                return;
            }

            element.data = abilityInfo.ToolTipData();
        }



        public void OnPointerUp(PointerEventData eventData)
        {
            Return();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            var moduleInfo = GetComponentInParent<ModuleInfo>();
            if (moduleInfo == null) { return; }
            if (!GetComponent<DragAndDrop>().enabled) { return; }

            if (moduleInfo.itemBin)
            {
                transform.SetParent(moduleInfo.itemBin);
            }
        }

        public void Return()
        {
            if (currentSlot == null) return;


            if (currentActionBarSlot &&
                currentActionBarHover == null)
            {
                DestroySelf();
            }

            transform.position = currentSlot.transform.position;
            GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta * 1.25f;

            transform.SetParent(currentSlot.transform);
            currentSlot.border.SetAsLastSibling();
        }

        public async void SetAbility(AbilityInfo ability)
        {
            abilityInfo = ability;

            await abilityInfo.UntilInitiationComplete();

            if (!ability.CanShow())
            {
                DestroySelf();
                return;
            }

            image.sprite = abilityInfo.Icon();
                

            if (abilityInfo.abilityType2 == AbilityType2.Passive ||
                abilityInfo.abilityType2 == AbilityType2.AutoAttack)
            {
                dragAndDrop.enabled = false;
            }

        }

        public void HandleActionBarSlot(ActionBarSlot slot)
        {
            if (slot.spellType != abilityInfo.abilityType2) { return; }
            if (slot.actionBarBehavior.abilityInfo &&
                slot.actionBarBehavior.abilityInfo.coolDown.charges == 0) { return; }

            slot.actionBarBehavior.SetActionBar(abilityInfo);

        }

        void DestroySelf()
        {
            Destroy(gameObject);
        }

        
    }

}