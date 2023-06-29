using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class DialogueEntryBehavior : MonoBehaviour
    {
        [Serializable]
        public struct Info
        {
            public TextMeshProUGUI name, content;
        }

        [SerializeField] Info info;

        public void SetEntry(DialogueEntry entry)
        {
            info.name.text = entry.speaker;
            info.content.text = entry.text;
        }
    }
}
