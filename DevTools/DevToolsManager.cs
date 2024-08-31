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
}
