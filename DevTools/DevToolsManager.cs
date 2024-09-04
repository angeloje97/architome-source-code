using Language.Lua;
using PixelCrushers;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome.DevTools
{
    public class DevToolsManager : MonoBehaviour
    {
        public static DevToolsManager active;



        [Header("Components")]
        public NavBarController navbarController;
        public NavBar navbar;
        public ArchButton buttonPrefab;
        public Toggle togglePrefab;

        public void Awake()
        {
            SingletonManger.HandleSingleton(this.GetType(), gameObject, true, true, () => {
                active = this;
            });
        }

        private void OnValidate()
        {
            actions ??= GetComponentInChildren<Actions>();
            toggles ??= GetComponentInChildren<Toggles>();
        }

        private void Start()
        {
            GetDependencies();
            
        }

        void GetDependencies()
        {

        }

        [SerializeField] Actions actions;
        public CanvasGroup actionsCG;

        #region Create UI

        public void CreateUI()
        {
            HandleActions();
            HandleToggles();
        }

        void HandleActions()
        {
            navbar.AddToggle("Actions", actionsCG.gameObject);

            foreach(var item in actions.requests)
            {
                var label = item.name;

                var button = Instantiate(buttonPrefab, actionsCG.transform);

                button.OnClick += (archButton) => {
                    item.Invoke();
                };
            }

        }

        [SerializeField] Toggles toggles;
        public CanvasGroup togglesCG;

        void HandleToggles()
        {
            navbar.AddToggle("Toggles", togglesCG.gameObject);

            foreach(var item in toggles.requests)
            {
                var label = item.name;
                var newToggle = Instantiate(togglePrefab, togglesCG.transform);
                newToggle.onValueChanged.AddListener((bool newValue) => {
                    item.SetState(newValue);

                });
            }
        }

        #endregion

        
    }

    [Serializable]
    public class ComponentTypes
    {
        public InputField defaultComponent;
        public Toggle booleanComponent;
        

        public Transform CreateComponent(Type type, Transform targetParent, Action<object> onValueChange)
        {
            if(type == typeof(bool))
            {
                var boolComponent = UnityEngine.Object.Instantiate(booleanComponent, targetParent);

                boolComponent.onValueChanged.AddListener((bool newValue) => {
                    onValueChange?.Invoke(newValue);
                });

                return boolComponent.transform;
            }

            var defaultComponent = UnityEngine.Object.Instantiate(this.defaultComponent, targetParent);

            defaultComponent.onValueChanged.AddListener((string newValue) => {
                onValueChange?.Invoke(newValue);
            });

            return defaultComponent.transform;
        }

        public void HandleComponentData(ComponentData data)
        {
            foreach(KeyValuePair<string, Type> typeKey in data.typeKeys)
            {
                var component = CreateComponent(typeKey.Value, data.targetParent, (object newValue) => {
                    data.UpdateKey(typeKey.Key, newValue);
                });
            }
        }
    }

    public class ComponentData
    {
        public Transform targetParent;
        

        public Dictionary<string, Type> typeKeys { get; private set; }
        public Dictionary<string, object> componentValues { get; private set; }

        public void UpdateKey(string key, object value)
        {
            if (!componentValues.ContainsKey(key))
            {
                componentValues.Add(key, value);
                return;
            }

            componentValues[key] = value;
        }

    }
}
