using Language.Lua;
using PixelCrushers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public RequestHandler requestHandlerPrefab;

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

        public async void CreateUI()
        {
            var actionsTask = new Task(() => {
                HandleActions();
            });

            var togglesTasks = new Task(() => {
                HandleToggles();
            });

            actionsTask.Start();
            togglesTasks.Start();

            await Task.WhenAll(actionsTask, togglesTasks);

        }

        async void HandleActions()
        {
            navbar.AddToggle("Actions", actionsCG.gameObject);
            var actionsTransform = actionsCG.transform;
            foreach(var item in actions.requests)
            {
                var newRequestHandler = CreateRequestHandler(actionsTransform);
                await newRequestHandler.HandleRequest(item);
            }

        }

        [SerializeField] Toggles toggles;
        public CanvasGroup togglesCG;

        async void HandleToggles()
        {
            navbar.AddToggle("Toggles", togglesCG.gameObject);
            var togglesTransform = togglesCG.transform;
            foreach (var item in toggles.requests)
            {
                var newRequestHandler = CreateRequestHandler(togglesTransform);
                await newRequestHandler.HandleRequest(item);
            }
        }

        public RequestHandler CreateRequestHandler(Transform parent)
        {
            var newRequestHandler = Instantiate(requestHandlerPrefab, parent);
            return newRequestHandler;
        }

        #endregion


    }


}
