using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;
using Architome.Settings.Keybindings;

namespace Architome
{
    public class KeyBindings : MonoBehaviour
    {

        [Serializable]
        public struct KeyCodeSprites
        {
            public KeyCode keyCode;
            public int spriteIndex;

        }

        public List<KeybindSet> availableSets;

        public static KeyBindings active;
        public string testKey;
        public List<String2> combatBindings;
        public Dictionary<string, KeyCode> keyBinds = new Dictionary<string, KeyCode>()
        {
            ["Ability0"] = KeyCode.Q,
            ["Ability1"] = KeyCode.W,
            ["Ability2"] = KeyCode.E,
            ["Ability3"] = KeyCode.R,
            ["Ability4"] = KeyCode.T,
            ["Ability5"] = KeyCode.A,
            ["Ability6"] = KeyCode.S,
            ["Ability7"] = KeyCode.D,
            ["Ability8"] = KeyCode.F,
            ["Ability9"] = KeyCode.G,
            ["Ability10"] = KeyCode.X,


            ["AlternateAction0"] = KeyCode.Alpha1,
            ["AlternateAction1"] = KeyCode.Alpha2,
            ["AlternateAction2"] = KeyCode.Alpha3,
            ["AlternateAction3"] = KeyCode.Alpha4,
            ["AlternateAction4"] = KeyCode.Alpha5,


            ["NextCamera"] = KeyCode.Tab,
            ["ToggleFreeCam"] = KeyCode.Y,

            ["SelectMultiple"] = KeyCode.LeftShift,
            ["SelectObject"] = KeyCode.Mouse0,
            ["Select"] = KeyCode.Mouse0,
            ["CameraRotator"] = KeyCode.Mouse2,

            ["Action"] = KeyCode.Mouse1,

            ["Escape"] = KeyCode.Escape
        };
        public bool save;
        public bool load;

        public Action<KeyBindings> OnLoadBindings { get; set; }


        public static Dictionary<string, KeyCode> partyCombatDefault = new Dictionary<string, KeyCode>()
        {
            ["Ability0"] = KeyCode.Q,
            ["Ability1"] = KeyCode.W,
            ["Ability2"] = KeyCode.E,
            ["Ability3"] = KeyCode.R,
            ["Ability4"] = KeyCode.T,
            ["Ability5"] = KeyCode.A,
            ["Ability6"] = KeyCode.S,
            ["Ability7"] = KeyCode.D,
            ["Ability8"] = KeyCode.F,
            ["Ability9"] = KeyCode.G,
            ["Ability10"] = KeyCode.X,


            ["AlternateAction0"] = KeyCode.Alpha1,
            ["AlternateAction1"] = KeyCode.Alpha2,
            ["AlternateAction2"] = KeyCode.Alpha3,
            ["AlternateAction3"] = KeyCode.Alpha4,
            ["AlternateAction4"] = KeyCode.Alpha5,


            ["NextCamera"] = KeyCode.Tab,
            ["ToggleFreeCam"] = KeyCode.Y,
            ["ToggleSheath"] = KeyCode.Z,

            ["SelectMultiple"] = KeyCode.LeftShift,
            ["SelectObject"] = KeyCode.Mouse0,
            ["Select"] = KeyCode.Mouse0,
            ["CameraRotator"] = KeyCode.Mouse2,

            ["Action"] = KeyCode.Mouse1,

            ["Escape"] = KeyCode.Escape
        };

        public static HashSet<string> nonEditable = new()
        {
            "SelectObject",
            "Select",
            "Escape",
        };

        public Dictionary<KeyCode, int> keyCodeIndexes = new();

        public void Awake()
        {
            active = this;

            foreach (var keyCodeSprite in keyCodeSprites)
            {
                keyCodeIndexes.Add(keyCodeSprite.keyCode, keyCodeSprite.spriteIndex);
            }

            LoadKeyBindings();

        }
        

        public List<KeyCodeSprites> keyCodeSprites = new();
        private void Start()
        {

        }

        public void HandleSave()
        {
            if (!save) return;
            save = false;

            KeyBindingsSave._current.combatBindings = combatBindings;
            KeyBindingsSave._current.Save();
        }

        void HandleLoad()
        {
            if (!load) return;
            load = false;

            LoadKeyBindings();
        }

        public void Update()
        {
            HandleSave();
            HandleLoad();
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

        private void OnValidate()
        {

            if (testKey.Length == 0) return;

            KeyCode parsedKeyCode = (KeyCode)Enum.Parse(typeof(KeyCode), testKey);


            Debugger.InConsole(0452, $"{parsedKeyCode}");
            UpdateKeyCodeSprites();
        }

        public int SpriteIndex(KeyCode key)
        {
            
            if (key == KeyCode.Alpha0) return 0;
            if (key == KeyCode.Keypad0) return 0;
            if (!keyCodeIndexes.ContainsKey(key)) return -1;
            if (keyCodeIndexes[key] == 0) return -1;

            return keyCodeIndexes[key];
        }
        public int SpriteIndex(string keyName)
        {
            return SpriteIndex(keyBinds[keyName]);
        }

        public int SpriteIndex(string keybindSet, string keyName)
        {
            if (availableSets == null) return -1;

            foreach(var set in availableSets)
            {
                if(set.name == keybindSet)
                {
                    var keyCode = set.KeyCodeFromName(keyName);
                    if (keyCode == KeyCode.Question) return -1;
                    return SpriteIndex(keyCode);
                }
            }

            return -1;
        }

        public void SaveKeyBindings()
        {
            var combatBindings = new List<String2>();


            foreach (var bindingPair in keyBinds)
            {
                combatBindings.Add(new(bindingPair.Key, bindingPair.Value.ToString()));
            }

            this.combatBindings = combatBindings;

            KeyBindingsSave._current.combatBindings = combatBindings;

            KeyBindingsSave._current.Save();

        }

        void UpdateKeyCodeSprites()
        {
            //foreach (KeyCode code in Enum.GetValues(typeof(KeyCode)))
            //{
            //    bool contains = false;
            //    foreach (var keyCodeSprite in keyCodeSprites)
            //    {
            //        if (keyCodeSprite.keyCode == code)
            //        {
            //            contains = true;
            //            break;
            //        }
            //    }

            //    if (!contains)
            //    {
            //        keyCodeSprites.Add(new() { keyCode = code });
            //    }
            //}
        }

        public void LoadKeyBindings()
        {
            var bindingsSave = KeyBindingsSave._current.LoadBindings();
            
            if (bindingsSave == null)
            {
                SaveKeyBindings();

                bindingsSave = KeyBindingsSave._current.LoadBindings();

                if (bindingsSave == null)
                {
                    throw new Exception("Cannot load bindings");
                }
            }

            combatBindings = new();


            foreach (var binding in bindingsSave.combatBindings)
            {
                if (!keyBinds.ContainsKey(binding.x)) continue;
                if (nonEditable.Contains(binding.x)) continue;

                combatBindings.Add(new(binding.x, binding.y));
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), binding.y);

                keyBinds[binding.x] = keyCode;
            }

            foreach(KeyValuePair<string, KeyCode> binding in partyCombatDefault)
            {
                if (keyBinds.ContainsKey(binding.Key)) continue;
                Debugger.UI(7469, $"Missing Key: {binding.Key}");
                combatBindings.Add(new(binding.Key, binding.Value.ToString()));
                keyBinds.Add(binding.Key, binding.Value);
            }

            OnLoadBindings?.Invoke(this);
        }

        public void MapBindings()
        {

            foreach (var binding in combatBindings)
            {
                if (!keyBinds.ContainsKey(binding.x)) continue;
                keyBinds[binding.x] = (KeyCode)Enum.Parse(typeof(KeyCode), binding.y);
            }
        }
        
    }

}