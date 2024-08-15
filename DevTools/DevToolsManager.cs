using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.DevTools
{
    public class DevToolsManager : MonoBehaviour
    {
        public static DevToolsManager active;


        [Header("Components")]
        public NavBarController navbarController;
        public NavBar navbar;

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
            HandleActions();
            HandleToggles();
        }

        void GetDependencies()
        {

        }

        Actions actions;
        public CanvasGroup actionsCG;


        void HandleActions()
        {
            navbar.AddToggle("Actions", actionsCG.gameObject);
        }

        Toggles toggles;
        public CanvasGroup togglesCG;

        void HandleToggles()
        {
            navbar.AddToggle("Toggles", togglesCG.gameObject);
        }

    }
}
