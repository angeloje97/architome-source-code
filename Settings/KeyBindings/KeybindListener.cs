using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace Architome.Settings.Keybindings
{
    public class KeybindListener : MonoBehaviour
    {
        #region Common Data
        [SerializeField] List<ListenerEvent> listenerInfo;
        ArchInput archInput;

        List<ListenerEvent> blockedEvents;

        public bool listening;
        #endregion

        #region Initiation

        void Start()
        {
            GetDependencies();
        }

        private async void OnEnable()
        {
            while (archInput == null) await Task.Yield();
            AddListeners();
        }

        void GetDependencies()
        {
            archInput = ArchInput.active;
        }
        #endregion

        private void OnDisable()
        {
            RemoveListeners();
        }

        private void OnValidate()
        {
            listenerInfo ??= new();
            foreach (var info in listenerInfo)
            {
                info.Validate();
            }
        }

        void AddListeners()
        {
            if (listening) return;

            listenerInfo ??= new();
            if (archInput == null) return;

            var keyBindings = KeyBindings.active;
            
            foreach(var listener in listenerInfo)
            {
                listener.AddListener(archInput);

                HandleKeyName(listener);

                keyBindings.AddListener(BindingEvents.OnSave, (KeybindSet set) => {
                    HandleKeyName(listener);
                }, this);
            }
            listening = true;

            void HandleKeyName(ListenerEvent listener)
            {
                if (listener.setTypes.Count <= 0) return;

                var keyName = keyBindings.KeyCodeFromSetName(listener.setTypes[0], listener.bindingName).ToString();
                keyName = ArchString.Replace(keyName, "(Alpha|ALPHA)", "");
                listener.OnObtainKeyName?.Invoke(keyName);
                listener.OnObtainListener?.Invoke(listener);
            }
        }


        void RemoveListeners()
        {
            if (!listening) return;
            listenerInfo ??= new();

            foreach(var listener in listenerInfo)
            {
                listener.RemoveListener();
            }
            listening = true;
        }

        public void BlockListener(int index)
        {
            listenerInfo[index].Block();
        }

        public void EnableListener(int index)
        {
            listenerInfo[index].Enable();
        }


        #region Listener Event
        [Serializable]
        public class ListenerEvent 
        {
            [HideInInspector] public string name;
            

            public List<KeybindSetType> setTypes;
            public bool anySet;

            public KeybindType bindingType;
            public string bindingName;
            public bool blockedByDefault;

            public UnityEvent OnBinding;
            public UnityEvent<string> OnObtainKeyName;
            public UnityEvent<ListenerEvent> OnObtainListener;


            bool blocked = false;
            float blockTimer;

            Action Unsubscribe;

            public void AddListener(ArchInput inputSystem)
            {
                Unsubscribe = null;
                setTypes ??= new();


                foreach(var setType in setTypes)
                {
                    Unsubscribe += inputSystem.AddListener(() => {
                        if (blocked) return;
                        OnBinding?.Invoke();
                    }, setType, bindingType, bindingName);
                }

                if (blockedByDefault)
                {
                    blocked = true;
                }
            }

            public async void Block()
            {
                if (blockedByDefault) return;
                blockTimer = .10f;

                if (blocked) return;
                blocked = true;

                while(blockTimer > 0f)
                {
                    blockTimer -= Time.deltaTime;
                    await Task.Yield();
                }

                blocked = false;

            }

            public async void Enable()
            {
                if (!blockedByDefault) return;
                blockTimer = .10f;

                if (!blocked) return;
                blocked = false;

                while (blockTimer > 0f)
                {
                    blockTimer -= Time.deltaTime;
                    await Task.Yield();
                }

                blocked = true;

            }

            public void Validate()
            {
                if (anySet)
                {
                    setTypes ??= new();
                    foreach(KeybindSetType setType in Enum.GetValues(typeof(KeybindSetType)))
                    {
                        if (setTypes.Contains(setType)) continue;
                        setTypes.Add(setType);
                    }
                }

                name = bindingName;
            }


            public void RemoveListener()
            {
                if (Unsubscribe == null) return;
                Unsubscribe();
            }
        }
        #endregion
    }
}
