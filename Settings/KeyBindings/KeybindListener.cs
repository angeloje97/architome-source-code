using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Architome.Settings.Keybindings
{
    public class KeybindListener : MonoBehaviour
    {
        [SerializeField] List<ListenerEvent> listenerInfo;
        ArchInput archInput;

        void Start()
        {
            GetDependencies();
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        void GetDependencies()
        {
            archInput = ArchInput.active;
        }


        void AddListeners()
        {
            listenerInfo ??= new();
            if (archInput == null) return;
            foreach(var listener in listenerInfo)
            {
                listener.AddListener(archInput);
            }
        }

        void RemoveListeners()
        {
            listenerInfo ??= new();

            foreach(var listener in listenerInfo)
            {
                listener.RemoveListener();
            }
        }

        [Serializable]
        public class ListenerEvent 
        {
            public KeybindSetType setType;
            public KeybindType bindingType;
            public string bindingName;

            public UnityEvent OnBinding;
            public UnityEvent<string> OnObtainKeyName;

            Action Unsubscribe;

            public void AddListener(ArchInput inputSystem)
            {
                Unsubscribe = inputSystem.AddListener(() => {
                    OnBinding?.Invoke();
                }, setType, bindingType, bindingName);
            }


            public void RemoveListener()
            {
                if (Unsubscribe == null) return;
                Unsubscribe();
            }
        }
    }
}
