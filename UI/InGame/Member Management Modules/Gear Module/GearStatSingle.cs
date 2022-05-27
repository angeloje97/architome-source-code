using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace Architome
{
    public class GearStatSingle : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI statName;
            public TextMeshProUGUI statValue;
        }

        [SerializeField]Info info;

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
