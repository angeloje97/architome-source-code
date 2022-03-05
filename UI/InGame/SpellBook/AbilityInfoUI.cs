using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Architome.Enums;

public class AbilityInfoUI : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public AbilityInfo abilityInfo;
    public SpellBookSlot currentSlot;
    public ActionBarSlot currentActionBarSlot;
    public ActionBarSlot currentActionBarHover;

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
        if(moduleInfo == null) { return; }
        if(!GetComponent<DragAndDrop>().enabled) { return; }

        if(moduleInfo.itemBin)
        {
            transform.SetParent(moduleInfo.itemBin);
        }
    }

    public void Return()
    {
        if(currentSlot != null)
        {
            if (currentActionBarSlot &&
                currentActionBarHover == null)
            { 
                Destroy(gameObject); 
            }

            currentSlot.SetAbilityUI(gameObject);
        }
    }

    public void SetAbility(AbilityInfo ability)
    {
        abilityInfo = ability;

        
        if(abilityInfo.catalystInfo.catalystIcon &&
            GetComponent<Image>())
        {
            GetComponent<Image>().sprite = abilityInfo.catalystInfo.catalystIcon;
        }

        if (abilityInfo.abilityType2 == AbilityType2.Passive ||
            abilityInfo.abilityType2 == AbilityType2.AutoAttack)
        {
            if(GetComponent<DragAndDrop>())
            {
                GetComponent<DragAndDrop>().enabled = false;
            }
        }
    }

    public void HandleActionBarSlot(ActionBarSlot slot)
    {
        if(slot.spellType != abilityInfo.abilityType2) { return; }
        if(slot.actionBarBehavior.abilityInfo &&
            slot.actionBarBehavior.abilityInfo.currentCharges == 0) { return; }

        slot.actionBarBehavior.SetActionBar(abilityInfo);

    }
}
