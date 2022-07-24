using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using UnityEngine.EventSystems;

namespace Architome
{
    public class GearStatSingle : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI statName;
            public TextMeshProUGUI statValue;
        }

        [SerializeField]Info info;

        public Action<GearStatSingle, bool> OnMouseOver;

        public void OnPointerEnter(PointerEventData eventData)
        {
            OnMouseOver?.Invoke(this, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            OnMouseOver?.Invoke(this, false);
        }


        public void UpdateSingle(string statValue)
        {
            info.statValue.text = statValue;
        }

        public void SetSingle(string statName)
        {
            info.statName.text = statName;
        }

        public void SetNull()
        {
            transform.SetAsLastSibling();
        }

    }
}
