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
            { KeyCode.L, 29 },
            { KeyCode.M, 30 },
            { KeyCode.N, 31 },
            { KeyCode.O, 32 },
            { KeyCode.P, 33 },
            { KeyCode.Q, 34 },
            { KeyCode.R, 35 },
            { KeyCode.S, 36 },
            { KeyCode.T, 37 },
            { KeyCode.U, 38 },
            { KeyCode.V, 39 },
            { KeyCode.W, 40 },
            { KeyCode.X, 41 },
            { KeyCode.Y, 42 },
            { KeyCode.Z, 43 },
            { KeyCode.Backspace, 44 },
            { KeyCode.LeftBracket, 45 },
            { KeyCode.RightBracket, 46 },
            { KeyCode.CapsLock, 47 },
            { KeyCode.Caret, 48 },
            { KeyCode.Colon, 49 },
            { KeyCode.Comma, 50 },
            { KeyCode.LeftControl, 51 },
            { KeyCode.RightControl, 51 },
            { KeyCode.LeftCurlyBracket, 52 },
            { KeyCode.RightCurlyBracket, 53 },
            { KeyCode.Delete, 54 },
            { KeyCode.Dollar, 55 },
            { KeyCode.DoubleQuote, 56 },
            { KeyCode.End, 57 },
            { KeyCode.LeftWindows, 58 },
            { KeyCode.RightWindows, 58 },
            { KeyCode.Tilde, 60 },
            { KeyCode.Tab, 61 },
            { KeyCode.Return, 62 },
            { KeyCode.KeypadEnter, 62 },
            { KeyCode.Equals, 63 },
            { KeyCode.Escape, 64 },
            { KeyCode.Exclaim, 65 },
            { KeyCode.Space, 66 },
            { KeyCode.F1, 67 },
            { KeyCode.F2, 68 },
            { KeyCode.F3, 69 },
            { KeyCode.F4, 70 },
            { KeyCode.F5, 71 },
            { KeyCode.F6, 72 },
            { KeyCode.F7, 73 },
            { KeyCode.F8, 74 },
            { KeyCode.F9, 75 },
            { KeyCode.F10, 76 },
            { KeyCode.F11, 77 },
            { KeyCode.F12, 78 },
            { KeyCode.BackQuote, 79 },
            { KeyCode.Greater, 80 },
            { KeyCode.Slash, 81 },
            { KeyCode.Home, 82 },
            { KeyCode.Quote, 83 },
            { KeyCode.Insert, 84 },
            { KeyCode.LeftShift, 85 },
            { KeyCode.RightShift, 85 },
            { KeyCode.Semicolon, 86 },
            { KeyCode.Question, 87 },
            { KeyCode.Less, 88 },
            { KeyCode.Underscore, 89 },
            { KeyCode.Minus, 98 },
            { KeyCode.Hash, 99 },
            { KeyCode.PageDown, 100 },
            { KeyCode.Period, 101 },
            { KeyCode.Plus, 102 },
            { KeyCode.PageUp, 103 },
            { KeyCode.LeftParen, 104 },
            { KeyCode.RightParen, 105 },
            { KeyCode.Percent, 106 },
            { KeyCode.Mouse2, 112 },
            { KeyCode.Mouse3, 114 },
            { KeyCode.Mouse4, 114 },
            { KeyCode.Mouse5, 114 },
            { KeyCode.Mouse6, 114 },
            { KeyCode.Mouse0, 115 },
            { KeyCode.Mouse1, 118 },
            
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

        public static KeyCode TempKeyCode(KeyCode tempKeyCode)
        {
            return tempKeyCode;
        }
    }
}
