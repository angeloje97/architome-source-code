using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Architome.Settings.Keybindings;

namespace Architome
{
    public enum BindingEvents
    {
        OnLoad,
        OnSave
    }
    public class KeyBindings : MonoBehaviour
    {
        public List<KeybindSet> availableSets;

        public static KeyBindings active;


        public static HashSet<string> nonEditable = new()
        {
            "SelectObject",
            "Select",
            "Escape",
        };

        public Dictionary<KeyCode, int> keyCodeIndexes = new();

        public Dictionary<BindingEvents, Action<KeybindSet>> eventDict;

        public void Awake()
        {
            active = this;
            KeyBindingsSave._current.LoadBindings();

            UpdateBindings();

        }

        void UpdateBindings()
        {
            for(int i = 0; i < availableSets.Count; i++)
            {
                availableSets[i] = availableSets[i].Clone();
                availableSets[i].LoadBindingData();
            }
        }

        private void Start()
        {

        }
        public static async Task LetGoKeys()
        {
            var keys = Enum.GetValues(typeof(KeyCode));
            while(true)
            {
                var pressing = false;
                foreach (KeyCode key in keys)
                {
                    if (Input.GetKey(key)) pressing = true;
                    break;
                }

                if (!pressing)
                {
                    break;
                }

                await Task.Yield();
            }

        }
        public static async Task LetGoKeys(List<KeyCode> keyCodes)
        {
            await Task.Yield();

            while (true)
            {
                var pressing = false;
                foreach (var key in keyCodes)
                {
                    if (Input.GetKey(key))
                    {
                        pressing = true;
                        break;
                    }
                }

                if (!pressing)
                {
                    break;
                }

                await Task.Yield();
            }

            await Task.Yield();

            Debugger.UI(5439, $"User has let go of key");
        }
        public static async Task PressAnyKey(List<KeyCode> keyCodes)
        {
            await Task.Yield();

            while (true)
            {
                var pressing = false;

                foreach(var key in keyCodes)
                {
                    if(Input.GetKeyDown(key) || Input.GetKeyUp(key))
                    {
                        pressing = true;
                    }
                }

                if (pressing)
                {
                    break;
                }
            }

            await Task.Yield();
        }
        
        public void InvokeEvent(BindingEvents eventType, KeybindSet set)
        {
            eventDict ??= new();

            if (!eventDict.ContainsKey(eventType)) return;
            UpdateBindings();
            eventDict[eventType]?.Invoke(set);
        }

        public void AddListener<T>(BindingEvents eventType, Action<KeybindSet> action, T caller) where T : Component
        {
            eventDict ??= new();
            if (!eventDict.ContainsKey(eventType)) eventDict.Add(eventType, null);

            eventDict[eventType] += MiddleWare;

            void MiddleWare(KeybindSet set)
            {
                if(caller == null)
                {
                    eventDict[eventType] -= MiddleWare;
                    return;
                }

                action(set);
            }
        }

        public int SpriteIndex(KeyCode key)
        {
            
            if (key == KeyCode.Alpha0) return 0;
            if (key == KeyCode.Keypad0) return 0;
            var keyCodeIndexes = KeyBindingsSave.keyCodeSpriteIndex;
            if (!keyCodeIndexes.ContainsKey(key)) return -1;
            if (keyCodeIndexes[key] == 0) return -1;

            return keyCodeIndexes[key];
        }

        public KeyCode KeyCodeFromSetName(KeybindSetType setType, string keyName)
        {
            foreach(var set in availableSets)
            {
                if(set.type == setType)
                {
                    return set.KeyCodeFromName(keyName);
                }
            }

            return KeyCode.Question;
        }
        public int SpriteIndex(KeybindSetType keybindSet, string keyName)
        {
            if (availableSets == null) return -1;

            foreach(var set in availableSets)
            {
                if (set.type != keybindSet && keybindSet != KeybindSetType.Any) continue;
                var keyCode = set.KeyCodeFromName(keyName);
                if (keyCode == KeyCode.Question) continue;
                return SpriteIndex(keyCode);
            }

            return -1;
        }
    }

}