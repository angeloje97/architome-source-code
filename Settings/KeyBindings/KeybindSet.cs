using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Architome.Settings.Keybindings.KeybindSet;

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
        Any,
        Single,
        Party,
    }

    [Serializable]
    public class SerializedKeybindSet
    {
        public KeybindSetType type;
        public ArchInputMode inputMode;
        public List<KeybindData> keybindsData;

        public SerializedKeybindSet(KeybindSet source)
        {
            type = source.type;
            inputMode = source.inputMode;
            keybindsData = source.keybindsData.ToList();
        }

    }

    [CreateAssetMenu(fileName = "New Keybind Set", menuName = "Architome/Settings/New Keybind Set")]
    public class KeybindSet : ScriptableObject
    {
        [HideInInspector]
        new public string name;
        public KeybindSetType type;
        public ArchInputMode inputMode;

        public List<KeybindData> keybindsData;
        public List<KeybindData> tempBindings;
        public Dictionary<string, int> keybindIndeces;

        public Dictionary<string, KeyCode> downDict, upDict, holdDict;

        bool cloned;

        private void OnValidate()
        {
            name = type.ToString();
        }

        public void UpdateSet()
        {
            downDict = new();
            upDict = new();
            holdDict = new();
            keybindIndeces = new();
            tempBindings = new();
             

            var length = keybindsData.Count;

            for(int i = 0; i < length; i++)
            {
                var data = keybindsData[i];

                keybindIndeces.Add(data.alias, i);
                tempBindings.Add(data.Clone());
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

        public KeybindSet Clone(bool uniqueClone = false)
        {
            if (!uniqueClone && cloned) return this;
            var clone = Instantiate(this);
            clone.cloned = true;
            return clone;
        }


        public void LoadBindingData()
        {
            if (!cloned) return;
            var keybindingSave = KeyBindingsSave._current;
            if (keybindingSave == null) return;

            var serializedData = keybindingSave.SerializedSet(this);

            type = serializedData.type;
            inputMode = serializedData.inputMode;
            keybindsData = serializedData.keybindsData.ToList();
        }

        public void ApplyEdits()
        {
            if (!cloned) return;
            Debugger.UI(1424, $"Saving cloned keybinding: {this}");
            keybindsData = tempBindings.ToList();

            var currentKeyBindSave = KeyBindingsSave._current;
            if (currentKeyBindSave == null) return;
            currentKeyBindSave.SaveSet(this);
        }

        public void RevertEdits()
        {
            if (!cloned) return;
            tempBindings = keybindsData.ToList();
            ApplyEdits();
        }

        public void SetBinding(string alias, KeyCode newBinding)
        {
            if (!cloned) return;
            if (keybindIndeces == null) return;
            Debugger.UI(6195, $"Keybind indeces exists: {keybindIndeces.Count} {keybindIndeces[alias]}");
            tempBindings[keybindIndeces[alias]].SetKeyCode(newBinding);
        }

        public List<KeybindData> EditableKeybinds(Action<KeybindData, int> action = null)
        {
            var keybinds = new List<KeybindData>();
            var length = keybindsData.Count;

            int index = 0;
            foreach(var binding in keybindsData)
            {
                if (binding.immutable) continue;
                keybinds.Add(binding);
                action?.Invoke(binding, index);
                index++;

            }

            return keybinds;
        }

        public KeyCode KeyCodeFromName(string keybindName)
        {
            foreach(var data in keybindsData)
            {
                if (data.alias.Equals(keybindName))
                {
                    return data.keyCode;
                }
            }
            return KeyCode.Question;
        }



        [Serializable]
        public class KeybindData
        {
            public string alias;
            public bool immutable;
            public KeyCode keyCode;
            public KeybindType keybindType;

            public void SetKeyCode(KeyCode keyCode)
            {
                this.keyCode = keyCode;
            }

            public KeybindData Clone()
            {
                var clone = new KeybindData();
                var fields = typeof(KeybindData).GetFields();
                foreach(var field in fields)
                {
                    field.SetValue(clone, field.GetValue(this));
                }

                return clone;
            }
        }

        

    }
}
