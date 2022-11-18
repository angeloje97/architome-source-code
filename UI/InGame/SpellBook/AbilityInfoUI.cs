using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Architome.Enums;
using static Architome.CatalystInfo.CatalystEffects;

namespace Architome
{
    public class AbilityInfoUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public AbilityInfo abilityInfo;
        public SpellBookSlot currentSlot;
        public ActionBarSlot currentActionBarSlot;
        public ActionBarSlot currentActionBarHover;

        ToolTip toolTip;
        ToolTipManager manager;
        

        ToolTipManager Manager 
        {
            get 
            {
                if (manager == null)
                {
                    manager = ToolTipManager.active;
                }

                return manager;
            } 
        }


        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

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

        public void SetAbility(AbilityInfo ability)
        {
            abilityInfo = ability;

            if (!ability.CanShow())
            {
                DestroySelf();
                return;
            }


            if (abilityInfo.catalystInfo.catalystIcon &&
                GetComponent<Image>())
            {
                GetComponent<Image>().sprite = abilityInfo.catalystInfo.catalystIcon;
            }

            if (abilityInfo.abilityType2 == AbilityType2.Passive ||
                abilityInfo.abilityType2 == AbilityType2.AutoAttack)
            {
                if (GetComponent<DragAndDrop>())
                {
                    GetComponent<DragAndDrop>().enabled = false;
                }
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (abilityInfo == null) return;
            var manager = Manager;

            if (manager == null) return;

            toolTip = manager.GeneralHeader();

            if (toolTip == null) return;

            toolTip.adjustToMouse = true;

            //var attributes = abilityInfo.PropertiesDescription() + abilityInfo.BuffList();

            //var buffDescription = abilityInfo.BuffDescriptions();

            //if (buffDescription.Length > 0)
            //{
            //    attributes += $"\n {buffDescription}";
            //}

            //toolTip.SetToolTip(new() {
            //    icon = abilityInfo.Icon(),
            //    name = abilityInfo.Name(),
            //    description = abilityInfo.Description(),
            //    attributes = attributes,
            //    value = abilityInfo.ResourceDescription()
            //});

            toolTip.SetToolTip(abilityInfo.ToolTipData(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (toolTip == null) return;

            toolTip.DestroySelf();
        }

        
    }

}