using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public class ArchInput : MonoBehaviour
    {
        public static ArchInput active;
        // Start is called before the first frame update
        KeyBindings bindings;
        ArchInputMode inputMode;

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


        void GetDependencies()
        {
            bindings = KeyBindings.active;
        }
        void Start()
        {
            GetDependencies();
        }

        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (bindings == null) return;
            HandleCombatInputs();
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