using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Architome.Settings.Keybindings
{
    public enum KeybindType
    {
        KeyDown,
        KeyUp,
        Hold,
    }

    public enum KeybindSetType
    {
        Single,
        SingleCombat,
        Party,
        PartyCombat,
    }

    [CreateAssetMenu(fileName = "New Keybind Set", menuName = "Architome/Settings/New Keybind Set")]
    public class KeybindSet : ScriptableObject
    {
        [HideInInspector]
        new public string name;
        public KeybindSetType type;

        public List<KeybindData> keybindsData;

        public Dictionary<string, KeyCode> downDict, upDict, holdDict;

        private void OnValidate()
        {
            name = type.ToString();
        }

        public void UpdateSet()
        {
            downDict = new();
            upDict = new();
            holdDict = new();

            foreach(var data in keybindsData)
            {
                switch (data.keybindType) 
                {
                    case KeybindType.KeyDown:
                        downDict.Add(data.alias, data.keyCode);
                        break;
                    case KeybindType.KeyUp:
                        upDict.Add(data.alias, data.keyCode);
                        break;
                    case KeybindType.Hold:
                        holdDict.Add(data.alias, data.keyCode);
                        break;
                }
            }
        }

        public KeyCode KeyCodeFromName(string keybindName)
        {
            foreach(var data in keybindsData)
            {
                if (data.Equals(keybindName))
                {
                    return data.keyCode;
                }
            }
            return KeyCode.Question;
        }


        [Serializable]
        public struct KeybindData
        {
            public string alias;
            public bool immutable;
            public KeyCode keyCode;
            public KeybindType keybindType;
        }

    }
}
