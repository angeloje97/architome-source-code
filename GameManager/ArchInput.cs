using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome.Settings.Keybindings;
using System.Threading.Tasks;

namespace Architome
{
    public class ArchInput : MonoBehaviour
    {
        public static ArchInput active;
        // Start is called before the first frame update
        KeyBindings bindings;
        [SerializeField]
        ArchInputMode inputMode;
        ArchInputMode desiredMode;

        IGGUIInfo gui;

        bool checkingBlockedInput;
        bool haltingInput;
        bool contextActive;
        bool pauseMenuActive;
        bool moduleActive;
        bool blockingInput;

        ArchSceneManager archSceneManager;


        public KeybindSet currentKeybindSet;

        public Dictionary<string, KeyCode> downDict, upDict, holdDict;

        public Dictionary<KeybindSetType, Dictionary<KeybindType, Dictionary<string, Action>>> events;


        public ArchInputMode Mode { get { return inputMode; } }

        public Action OnAction { get; set; }
        public Action OnSelect { get; set; }
        public Action OnActionMultiple { get; set; }
        public Action OnSelectMultiple { get; set; }
        public Action<float> OnScrollWheel { get; set; }
        public Action OnMiddleMouse { get; set; }

        public Action OnEscape { get; set; }




        void GetDependencies()
        {
            bindings = KeyBindings.active;
            gui = IGGUIInfo.active;
            var contextMenu = ContextMenu.current;
            archSceneManager = ArchSceneManager.active;


            if (archSceneManager)
            {
                archSceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }
            

            if (gui)
            {
                gui.OnModuleEnableChange += OnModuleEnableChange;
            }

            if (contextMenu)
            {
                contextMenu.OnContextActiveChange += OnContextActiveChange;
            }
        }




        void Start()
        {
            GetDependencies();
            HandlePauseMenu();
            
        }

        private void Awake()
        {
            active = this;
            events = new();
            SetKeybindSet(currentKeybindSet);
        }

        void BeforeLoadScene(ArchSceneManager archSceneManager)
        {
            if (!archSceneManager.sceneToLoad.Equals("PostDungeonResults")) return;
            inputMode = ArchInputMode.Inactive;

        }

        public async void HaltInput(Predicate<object> condition)
        {
            while (haltingInput)
            {
                await Task.Yield();
            }
            haltingInput = true;

            HandleBlocking();
            while (condition(null))
            {
                await Task.Yield();
            }

            haltingInput = false;
        }

        async void HandleBlocking()
        {
            if (blockingInput) return;
            blockingInput = true;
            desiredMode = inputMode;
            inputMode = ArchInputMode.Inactive;

            while (contextActive || haltingInput || moduleActive || pauseMenuActive)
            {
                await Task.Yield();
            }

            inputMode = desiredMode;
            blockingInput = false;
        }

        public void SetKeybindSet(KeybindSet set)
        {
            currentKeybindSet = set.Clone();
            currentKeybindSet.LoadBindingData();
            currentKeybindSet.UpdateSet();
            downDict = currentKeybindSet.downDict;
            upDict = currentKeybindSet.upDict;
            holdDict = currentKeybindSet.holdDict;
        }

        public async void OnContextActiveChange(ContextMenu context, bool isActive)
        {
            Debugger.Environment(6459, $"ArchInput listening to context menu isActive: {isActive}");

            contextActive = true;
            HandleBlocking();

            while (context.isChoosing)
            {
                await Task.Yield();
            }

            contextActive = false;
        }

        void HandlePauseMenu()
        {
            var pauseMenu = PauseMenu.active;
            if (!pauseMenu) return;

            pauseMenu.OnActiveChange += HandleActiveChange;

            async void HandleActiveChange(PauseMenu menu, bool state)
            {
                if (!state) return;
                pauseMenuActive = true;
                HandleBlocking();
                while (menu.isActive)
                {
                    await Task.Yield();
                }
                pauseMenuActive = false;
            }
        }

        async void OnModuleEnableChange(ModuleInfo changed)
        {
            if (checkingBlockedInput) return;

            checkingBlockedInput = true;


            var modules = gui.modules;
            
            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i].GetComponent<ModuleInfo>();
                if (module.isActive && module.blocksInput)
                {
                    moduleActive = true;
                    HandleBlocking();
                    while (module.isActive)
                    {
                        await Task.Yield();
                    }

                    i = 0;
                }
            }
            moduleActive = false;

            checkingBlockedInput = false;
        }

        // Update is called once per frame
        void Update()
        {
            HandleKeybindSet();
            if (bindings == null) return;
            //HandleCombatInputs();
            HandleGeneral();
        }

        void HandleGeneral()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscape?.Invoke();
            }

            if (Input.mouseScrollDelta.y != 0f)
            {
                OnScrollWheel?.Invoke(Input.mouseScrollDelta.y);
            }
        }

        void HandleCombatInputs()
        {
            if (inputMode != ArchInputMode.Adventure) return;

            HandleMouse();

            void HandleMouse()
            {
                

                if (Input.GetKeyDown(bindings.keyBinds["CameraRotator"]))
                {
                    OnMiddleMouse?.Invoke();
                }

                if (Input.GetKey(bindings.keyBinds["SelectMultiple"]))
                {
                    if (Input.GetKeyDown(bindings.keyBinds["Action"]))
                    {
                        OnActionMultiple?.Invoke();
                    }

                    if (Input.GetKeyDown(bindings.keyBinds["Select"]))
                    {
                        OnSelectMultiple?.Invoke();
                    }
                    return;
                }

                if (Input.GetKeyDown(bindings.keyBinds["Action"]))
                {
                    OnAction?.Invoke();
                }

                if (Input.GetKeyDown(bindings.keyBinds["Select"]))
                {
                    OnSelect?.Invoke();
                }


            }

        }

        void HandleKeybindSet()
        {
            if (currentKeybindSet == null) return;
            if (inputMode != currentKeybindSet.inputMode) return;

            var setType = currentKeybindSet.type;

            foreach(KeyValuePair<string, KeyCode> action in downDict)
            {
                if (Input.GetKeyDown(action.Value))
                {
                    InvokeEvent(setType, KeybindType.KeyDown, action.Key);
                }
            }

            foreach(KeyValuePair<string, KeyCode> action in upDict)
            {
                if (Input.GetKeyUp(action.Value))
                {
                    InvokeEvent(setType,KeybindType.KeyUp, action.Key);

                }
            }
            foreach(KeyValuePair<string, KeyCode> action in holdDict)
            {
                if (Input.GetKey(action.Value))
                {
                    InvokeEvent(setType, KeybindType.Hold, action.Key);

                }
            }

        }

        void InvokeEvent(KeybindSetType setName, KeybindType type, string name)
        {
            if (!events.ContainsKey(setName)) return;
            if (!events[setName].ContainsKey(type)) return;
            if (!events[setName][type].ContainsKey(name)) return;
            events[setName][type][name]?.Invoke();
        }

        public Action AddListener(Action action, KeybindSetType setName, KeybindType type, string alias)
        {
            if (!events.ContainsKey(setName)) events.Add(setName, new());
            if (!events[setName].ContainsKey(type)) events[setName].Add(type, new());
            if (!events[setName][type].ContainsKey(alias)) events[setName][type].Add(alias, null);

            events[setName][type][alias] += action;

            return () => {
                events[setName][type][alias] -= action;
            };
        }


    }

}