using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;
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

        IGGUIInfo gui;

        bool checkingBlockedInput;


        public ArchInputMode Mode { get { return inputMode; } }

        //Events
        public Action<int> OnAbilityKey { get; set; }
        public Action<int> OnAlternateAction { get; set; }
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

            if (gui)
            {
                gui.OnModuleEnableChange += OnModuleEnableChange;
            }
        }
        void Start()
        {
            GetDependencies();
        }

        private void Awake()
        {
            active = this;
        }

        async void OnModuleEnableChange(ModuleInfo changed)
        {
            if (checkingBlockedInput) return;

            checkingBlockedInput = true;

            var original = inputMode;

            var modules = gui.modules;

            for (int i = 0; i < modules.Count; i++)
            {
                var module = modules[i].GetComponent<ModuleInfo>();
                if (module.isActive && module.blocksInput)
                {
                    inputMode = ArchInputMode.Inactive;

                    while (module.isActive)
                    {
                        await Task.Yield();
                    }

                    i = 0;
                }
            }

            inputMode = original;

            checkingBlockedInput = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (bindings == null) return;
            HandleCombatInputs();
            HandleGeneral();
        }

        void HandleGeneral()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnEscape?.Invoke();
            }
        }

        void HandleCombatInputs()
        {
            if (inputMode != ArchInputMode.Combat) return;

            HandleAbilities();
            HandleAlternateActions();
            HandleMouse();

            void HandleAbilities()
            {
                for (int i = 0; i < 11; i++)
                {
                    if (Input.GetKeyDown(bindings.keyBinds[$"Ability{i}"]))
                    {
                        OnAbilityKey?.Invoke(i);
                    }
                }
            }

            void HandleAlternateActions()
            {
                for (int i = 0; i < 5; i++)
                {
                    if (Input.GetKeyDown(bindings.keyBinds[$"AlternateAction{i}"]))
                    {
                        OnAlternateAction?.Invoke(i);
                    }
                }
            }

            void HandleMouse()
            {
                if (Input.mouseScrollDelta.y != 0f)
                {
                    OnScrollWheel?.Invoke(Input.mouseScrollDelta.y);
                }

                if (Input.GetKeyDown(KeyCode.Mouse2))
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
    }

}