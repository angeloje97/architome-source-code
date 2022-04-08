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

        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void SetAbilityUI(GameObject abilityUI)
        {
            if (!abilityUI.GetComponent<AbilityInfoUI>()) { return; }


            abilityUI.GetComponent<AbilityInfoUI>().currentSlot = this;
            abilityUI.transform.position = transform.position;
            abilityUI.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta * 1.25f;
            abilityUI.transform.SetParent(transform);
            border.SetAsLastSibling();
        }
    }
}