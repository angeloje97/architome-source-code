using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class SpellBookSlot : MonoBehaviour
    {
        // Start is called before the first frame update
        public AbilityType2 slotType;
        public Transform border;
        public AbilityInfo ability;
        AbilityInfoUI currentAbilityUI;
        

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetAbilityUI(AbilityInfoUI abilityUI)
        {
            //if (!abilityUI.GetComponent<AbilityInfoUI>()) { return; }
            if (currentAbilityUI != null)
            {
                Destroy(currentAbilityUI.gameObject);
            }



            ability = abilityUI.abilityInfo;
            currentAbilityUI = abilityUI;

            foreach (var augmentSlot in transform.parent.GetComponentsInChildren<AugmentSlot>())
            {
                augmentSlot.SetAbility(ability);
            }


            abilityUI.currentSlot = this;
            abilityUI.transform.position = transform.position;
            abilityUI.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta * 1.25f;

            abilityUI.transform.SetParent(transform);
            border.SetAsLastSibling();

        }
    }
}