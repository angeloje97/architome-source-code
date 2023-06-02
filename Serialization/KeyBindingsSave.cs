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

        public readonly static Dictionary<KeyCode, int> keyCodeSpriteIndex = new()
        {
            { KeyCode.Alpha0, 0 },
            { KeyCode.Alpha1, 1 },
            { KeyCode.Alpha2, 2 },
            { KeyCode.Alpha3, 3 },
            { KeyCode.Alpha4, 4 },
            { KeyCode.Alpha5, 5 },
            { KeyCode.Alpha6, 6 },
            { KeyCode.Alpha7, 7 },
            { KeyCode.Alpha8, 8 },
            { KeyCode.Alpha9, 9 },
            { KeyCode.LeftAlt, 10 },
            { KeyCode.RightAlt, 10 },
            { KeyCode.Ampersand, 11 },
            { KeyCode.DownArrow, 12 },
            { KeyCode.LeftArrow, 13 },
            { KeyCode.RightArrow, 14 },
            { KeyCode.UpArrow, 15 },
            { KeyCode.Asterisk, 16 },
            { KeyCode.At, 17 },
            { KeyCode.A, 18 },
            { KeyCode.B, 19 },
            { KeyCode.C, 20 },
            { KeyCode.D, 21 },
            { KeyCode.E, 22 },
            { KeyCode.F, 23 },
            { KeyCode.G, 24 },
            { KeyCode.H, 25 },
            { KeyCode.I, 26 },
            { KeyCode.J,  27 },
            { KeyCode.K, 28 },
        };

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
