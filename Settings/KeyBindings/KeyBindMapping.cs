using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UIElements;
using Architome.Settings.Keybindings;
using UnityEngine.UI;

namespace Architome.Settings
{
    public class KeyBindMapping : ArchSettings
    {
        public static KeyBindMapping active;


        [Serializable]
        public struct Prefabs
        {
            public KeyBindMap map;
            public GameObject setParent;
        }

        [Serializable]
        public struct Info
        {
            public Transform mapParent;
            
            public Dictionary<KeybindSet, KeybindSet> originalSets;
            public List<KeybindSet> editableSets;
            public ScrollRect scrollRect;
            public int selectedSetIndex;

            public Dictionary<KeybindSet, CanvasGroup> parentKeybindSet;
            public KeyBindingsSave currentKeybindSave;
        }


        public Prefabs prefabs;
        public Info info;
        public KeyBindings keyBindings;

        public HashSet<KeyCode> blackList = new() {
            KeyCode.Escape,
            KeyCode.Mouse0
        };

        public Dictionary<KeybindSet, Dictionary<string, KeyBindMap>> keybindSetMaps;
        List<KeybindSet.KeybindData> currentDataList;

        public Action<KeyBindMapping> OnRevertChanges;
        public Action<KeyBindMapping> OnApplyChanges { get; set; }

        public bool pickingKey;
        private void Start()
        {
            GetDependencies();
            HandleDirtyConflicts();
        }
        private void Awake()
        {
            active = this;
        }
        void GetDependencies()
        {
            keyBindings = KeyBindings.active;
            info.currentKeybindSave = KeyBindingsSave._current;


            if (keyBindings == null)
            {
                this.enabled = false;
                return;
            }


            ClearMaps();
            SpawnMaps();

            var mainMenuUI = MainMenuUI.active;

            if (mainMenuUI)
            {
                mainMenuUI.OnEscapeIsFree += HandleEscapeIsFree;
            }
        }


        void HandleEscapeIsFree(MainMenuUI mainMenu, List<bool> checks)
        {
            if (pickingKey)
            {
                checks.Add(false);
            }
        }

        public override void HandleLeaveDirty()
        {
            RevertBindings();
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
            if (prefabs.setParent == null) return;
            if (info.mapParent == null) return;

            keybindSetMaps = new();
            info.parentKeybindSet = new();
            info.originalSets = new();


            for(int i = 0; i < info.editableSets.Count; i++)
            {
                var clone = info.editableSets[i].Clone();
                clone.LoadBindingData();
                clone.UpdateSet();

                info.originalSets.Add(clone, info.editableSets[i]);
                info.editableSets[i] = clone;


                keybindSetMaps.Add(clone, new());
                var setParent = Instantiate(prefabs.setParent, info.mapParent);
                info.parentKeybindSet.Add(clone, setParent.GetComponent<CanvasGroup>());
                setParent.name = $"{clone.name} Parent";



                clone.EditableKeybinds((KeybindSet.KeybindData data, int index) =>
                {
                    var mapper = Instantiate(prefabs.map, setParent.transform);

                    mapper.SetMap(data.alias, data.keyCode, index, this, clone);
                    keybindSetMaps[clone].Add(data.alias, mapper);
                });

                CheckConflicts(clone);
            }
        }

        public void CheckConflicts(KeybindSet set)
        {
            var keyMapDict = new Dictionary<KeyCode, KeyBindMap>();
            

            foreach(KeyValuePair<string, KeyBindMap> pair in keybindSetMaps[set])
            {
                var keyBindMap = pair.Value;
                if (keyBindMap.keyCode == KeyCode.None)
                {
                    Debugger.InConsole(9450, $"{keyBindMap} has key code none");
                    keyBindMap.SetConflict(true);
                }
                else if (keyMapDict.ContainsKey(keyBindMap.keyCode)) //Conflicting KeyBind
                {
                    keyBindMap.SetConflict(true);
                    keyMapDict[keyBindMap.keyCode].SetConflict(true);
                }
                else
                {
                    keyMapDict.Add(keyBindMap.keyCode, keyBindMap);
                    keyBindMap.SetConflict(false);
                }
            }
        }

        public void UpdateMap(KeybindSet set, KeyBindMap map, bool checkConflicts = true)
        {
            set.SetBinding(map.keyName, map.keyCode);

            currentDataList = set.tempBindings;
            Debugger.UI(5431, $"Setting {set}'s {map.keyName} to {map.keyCode}");
            if (checkConflicts)
            {
                CheckConflicts(set);
            }

            dirty = true;
        }

        public bool IsConflicted()
        {
            foreach(var set in info.editableSets)
            {
                var keyBindMaps = keybindSetMaps[set];
                foreach (KeyValuePair<string, KeyBindMap> pair in keyBindMaps)
                {
                    if (pair.Value.conflicted) return true;
                }
            }

            return false;
        }

        async public void ResetToDefault()
        {
            var currentSet = info.editableSets[info.selectedSetIndex];
            var originalSet = info.originalSets[currentSet];

            var choice = await PromptHandler.active.GeneralPrompt(new()
            {
                title = "Key Bindings",
                question = "Are you sure you want to reset to default?",
                options = new() 
                { 
                    new("Confirm", (option) => HandleConfirm()),
                    new("Cancel") {isEscape = true}
                },
                blocksScreen = true
            });



            void HandleConfirm()
            {
                var editableBindingData = currentSet.EditableKeybinds((KeybindSet.KeybindData data, int index) => {
                    
                    var map = keybindSetMaps[currentSet][data.alias];
                    map.keyCode = data.keyCode;
                    ArchAction.Yield(() => { map.SetConflict(false); });

                    UpdateMap(currentSet, map, false);


                });

                ArchAction.Yield(() => ApplyBindings());
                dirty = false;

            }

        }

        public void ChangeSet(int index)
        {
            info.selectedSetIndex = index;

        }

        public void RevertBindings()
        {
            var editableSet = info.editableSets[info.selectedSetIndex];

            editableSet.EditableKeybinds((KeybindSet.KeybindData data, int index) => {
                var map = keybindSetMaps[editableSet][data.alias];
                map.SetKeyString(data.keyCode);
            });

            OnRevertChanges?.Invoke(this);


            CheckConflicts(editableSet);

            dirty = false;
        }

        public override void HandleChooseApply()
        {
            ApplyBindings(true);
        }

        async public void ApplyBindings(bool ignoreConflictions = false)
        {
            if (!ignoreConflictions)
            {
                if (IsConflicted())
                {
                    var choice = await PromptHandler.active.GeneralPrompt(new() {
                        title = "Key Bindings",
                        question = "There are conflicting keybindings. Are you sure you want to current settings?",
                        options = new() { 
                            new("Apply", (option) => Apply()),
                            new("Cancel"),
                        },
                        blocksScreen = true
                    });

                    return;
                }
            }

            Apply();

            void Apply()
            {
                var currentSet = info.editableSets[info.selectedSetIndex];
                
                currentSet.ApplyEdits();


                OnApplyChanges?.Invoke(this);

                dirty = false;
            }

            
        }


    }
}
