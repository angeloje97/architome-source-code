using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Settings.Keybindings;
using System.Linq;

namespace Architome
{
    [Serializable]
    public class KeyBindingsSave
    {
        static KeyBindingsSave current;
        public static KeyBindingsSave _current
        {
            get
            {
                if (current == null)
                {
                    current = new();
                }

                return current;
            }
        }

        public List<String2> combatBindings;

        public Dictionary<string, SerializedKeybindSet> keybindSetDatas;
        public List<SerializedKeybindSet> keybindSetList;

        public void Save()
        {
            SerializationManager.SaveConfig("KeyBindings", this);
        }

        public KeyBindingsSave LoadBindings()
        {
            var obj = SerializationManager.LoadConfig("KeyBindings");
            if (obj == null) return null;

            var bindings = (KeyBindingsSave)obj;

            CopyValues(bindings);
            UpdateKeybindSetList();
            return bindings;
        }

        void UpdateKeybindSetList()
        {
            if (keybindSetDatas != null)
            {
                keybindSetList = keybindSetDatas.Select((KeyValuePair<string, SerializedKeybindSet> pair) =>
                {
                    return pair.Value;
                }).ToList();

            }

        }

        public void SaveSet(KeybindSet set)
        {
            keybindSetDatas ??= new();
            if (!keybindSetDatas.ContainsKey(set.name))
            {
                keybindSetDatas.Add(set.name, new(set));
            }
            else
            {
                keybindSetDatas[set.name] = new(set);

            }
            UpdateKeybindSetList();

            Save();
        }

        public SerializedKeybindSet SerializedSet(KeybindSet set)
        {
            if (keybindSetDatas == null) keybindSetDatas = new();
            if (!keybindSetDatas.ContainsKey(set.name)) return new(set);
            return keybindSetDatas[set.name];
        }

        void CopyValues(KeyBindingsSave bindings)
        {
            foreach (var field in bindings.GetType().GetFields())
            {
                field.SetValue(this, field.GetValue(bindings));
            }
        }
    }
}
