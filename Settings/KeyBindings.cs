using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;


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

            ["Action"] = KeyCode.Mouse1,

            ["Escape"] = KeyCode.Escape
        };
        public bool save;
        public bool load;


        public static Dictionary<string, KeyCode> keyBindsDefault = new Dictionary<string, KeyCode>()
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

            ["Action"] = KeyCode.Mouse1,

            ["Escape"] = KeyCode.Escape
        };

        public Dictionary<KeyCode, int> keyCodeIndexes = new();

        public void Awake()
        {
            active = this;

            foreach (var keyCodeSprite in keyCodeSprites)
            {
                keyCodeIndexes.Add(keyCodeSprite.keyCode, keyCodeSprite.spriteIndex);
            }
        }
        

        public List<KeyCodeSprites> keyCodeSprites = new();
        private void Start()
        {

            combatBindings = new();

            var save = KeyBindingsSave._current.LoadBindings();


            if (save == null)
            {
                UpdateCombatBindings();

                KeyBindingsSave._current.combatBindings = combatBindings;

                KeyBindingsSave._current.Save();

                save = KeyBindingsSave._current;
            }

            combatBindings = save.combatBindings;
        }

        public void HandleSave()
        {
            if (!save) return;
            save = false;

            KeyBindingsSave._current.combatBindings = combatBindings;
            KeyBindingsSave._current.Save();
        }

        void UpdateCombatBindings()
        {
            foreach (var bindingPair in keyBinds)
            {
                combatBindings.Add(new(bindingPair.Key, bindingPair.Value.ToString()));
            }
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

        public void SaveKeyBindings()
        {
            combatBindings = new();
            foreach (var bindingPair in keyBinds)
            {
                combatBindings.Add(new(bindingPair.Key, bindingPair.Value.ToString()));
            }

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
            
            if (bindingsSave == null) return;
            combatBindings = new();

            foreach (var binding in bindingsSave.combatBindings)
            {
                if (!keyBinds.ContainsKey(binding.x)) continue;

                combatBindings.Add(new(binding.x, binding.y));
                KeyCode keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), binding.y);

                keyBinds[binding.x] = keyCode;
            }
        }
        
    }

}