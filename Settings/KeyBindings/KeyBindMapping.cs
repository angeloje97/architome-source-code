using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome.Settings
{
    public class KeyBindMapping : MonoBehaviour
    {

        public static KeyBindMapping active;
        [Serializable]
        public struct Prefabs
        {
            public GameObject map;
        }

        [System.Serializable]
        public struct Info
        {
            public Transform mapParent;
        }

        public Prefabs prefabs;
        public Info info;
        public KeyBindings keyBindings;
        public HashSet<KeyCode> blackList = new() {
            KeyCode.Escape,
            KeyCode.Mouse0
        };

        //Combat Bindings
        public List<String2> combatBindings;
        public List<KeyBindMap> keyBindMaps;
        public Dictionary<string, KeyBindMap> mapDict;

        public Action<KeyBindMapping> OnRevertChanges;
        public Action<KeyBindMapping> OnApplyChanges;


        void GetDependencies()
        {
            keyBindings = KeyBindings.active;

            if (keyBindings == null)
            {
                this.enabled = false;
                return;
            }

            combatBindings = keyBindings.combatBindings.ToList();

            ClearMaps();
            SpawnMaps();
        }
        private void Start()
        {
            GetDependencies();
        }

        private void Awake()
        {
            active = this;
        }

        void ClearMaps()
        {
            foreach (Transform child in info.mapParent)
            {
                Destroy(child.gameObject);
            }
        }
        void SpawnMaps()
        {
            if (prefabs.map == null) return;
            if (info.mapParent == null) return;

            mapDict = new();

            for (int i = 0; i < combatBindings.Count; i++)
            {
                var binding = combatBindings[i];
                var mapper = Instantiate(prefabs.map, info.mapParent).GetComponent<KeyBindMap>();

                if (mapper == null)
                {
                    throw new Exception($"No script of type ${typeof(KeyBindMap)}");
                }

                mapper.SetMap(binding.x, binding.y, i, this );

                keyBindMaps.Add(mapper);
                mapDict.Add(mapper.keyName, mapper);
            }

            CheckConflicts();
        }

        public void CheckConflicts()
        {
            var keyMapDict = new Dictionary<KeyCode, KeyBindMap>();
            Debugger.InConsole(43812, $"{keyBindMaps.Count}");

            foreach (var keyBindMap in keyBindMaps)
            {
                if(keyBindMap.currentBinding == KeyCode.None)
                {
                    Debugger.InConsole(9450, $"{keyBindMap} has key code none");
                    keyBindMap.SetConflict(true);
                }
                else if (keyMapDict.ContainsKey(keyBindMap.currentBinding)) //Conflicting KeyBind
                {
                    keyBindMap.SetConflict(true);
                    keyMapDict[keyBindMap.currentBinding].SetConflict(true);
                }
                else
                {
                    keyMapDict.Add(keyBindMap.currentBinding, keyBindMap);
                    keyBindMap.SetConflict(false);
                }
            }
        }

        public void UpdateMap(KeyBindMap map)
        {
            if (!mapDict.ContainsKey(map.keyName)) return;

            if (combatBindings.Count <= map.index) return;

            combatBindings[map.index] = new() { x = map.keyName, y = map.keyCodeString };

            CheckConflicts();

        }

        public bool IsConflicted()
        {
            foreach (var keyBindMap in keyBindMaps)
            {
                if (keyBindMap.conflicted) return true;
            }

            return false;
        }

        async public void ResetToDefault()
        {

            var choice = await PromptHandler.active.GeneralPrompt(new()
            {
                title = "Key Bindings",
                question = "Are you sure you want to reset to default?",
                option1 = "Confirm",
                option2 = "Cancel",
                blocksScreen = true
            });


            if (choice.optionPicked != 0) return;


            foreach (var binding in KeyBindings.keyBindsDefault)
            {
                if (!mapDict.ContainsKey(binding.Key)) continue;
                var map = mapDict[binding.Key];

                ArchAction.Yield(() => { map.SetConflict(false); });

                map.SetKeyString(binding.Value.ToString());
                UpdateMap(map);
            }

            ArchAction.Yield(() => ApplyBindings());
        }

        public void RevertBindings()
        {
            combatBindings = keyBindings.combatBindings.ToList();
            OnRevertChanges?.Invoke(this);

            
            foreach (var binding in combatBindings)
            {
                if (!mapDict.ContainsKey(binding.x)) continue;

                mapDict[binding.x].SetKeyString(binding.y);
            }
        }

        async public void ApplyBindings()
        {
            if (IsConflicted())
            {
                var choice = await PromptHandler.active.GeneralPrompt(new() {
                    title = "Key Bindings",
                    question = "There are conflicting keybindings. Are you sure you want to current settings?",
                    option1 = "Apply",
                    option2 = "Cancel",
                    blocksScreen = true
                });

                if (choice.optionString == "Cancel")
                {
                    return;
                }
            }

            keyBindings.combatBindings = combatBindings.ToList();

            keyBindings.MapBindings();
            keyBindings.SaveKeyBindings();
            keyBindings.LoadKeyBindings();


            OnApplyChanges?.Invoke(this);
        }


    }
}
