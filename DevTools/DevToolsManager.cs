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

        Dictionary<Type, Component> _typeComponent;

        public Dictionary<Type, Component> typeComponent
        {
            get
            {
                if (_typeComponent == null)
                {
                    _typeComponent = new()
                    {
                        { typeof(bool), booleanComponent }
                    };
                }

                return _typeComponent;
            }
        }

        public Transform CreateComponent(ComponentData data)
        {
            if (!typeComponent.ContainsKey(data.typeCreatedBy))
            {
                var result = UnityEngine.Object.Instantiate(defaultComponent, data.targetParent);
                return result.transform;
            }
            else
            {
                var result = UnityEngine.Object.Instantiate(typeComponent[data.typeCreatedBy], data.targetParent);
                return result.transform;
            }
        }
    }

    public class ComponentData
    {
        public Type typeCreatedBy;
        public Transform targetParent;
        public Component instance;
    }
}
