using Architome.Enums;
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
    public class DevToolsManager : MonoActor
    {
        //busy playing apex
        #region Common Data
        public static DevToolsManager active;

        [Header("Components")]
        public NavBarController navbarController;
        public NavBar navbar;

        [Header("Prefabs")]
        public RequestHandler requestHandlerPrefab;

        GameState currentGameState
        {
            get
            {
                var gameManager = GameManager.active;

                if (gameManager)
                {
                    return gameManager.GameState;
                }

                return GameState.Menu;
            }
        }

        #endregion

        #region Initialization
        protected override void Awake()
        {
            base.Awake();
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
            CleanChildNodes();
            CreateUI();
            return;
        }

        #region GameStateChange Handler

        void GetDependencies()
        {
            var scenemanager = ArchSceneManager.active;

            scenemanager.events.AddListener(SceneEvent.OnLoadScene, () => {
                HandleGameManagerState();
            }, this);

            HandleGameManagerState();
        }
        async void HandleGameManagerState()
        {
            await UntilToolsCreated();
            var gameState = currentGameState;
            actionsToggleController.SetInteractable(actions.availableStates.Contains(gameState));
            togglesToggleController.SetInteractable(toggles.availableStates.Contains(gameState));
        }

        #endregion

        #endregion

        #region Create UI

        public void CreateUI()
        {
            HandleToggles();
            HandleActions();
        }
        public RequestHandler CreateRequestHandler(Transform parent)
        {
            var newRequestHandler = Instantiate(requestHandlerPrefab, parent);
            return newRequestHandler;
        }

        void CleanChildNodes()
        {
            foreach(Transform child in actionsCG.transform)
            {
                Destroy(child.gameObject);
            }

            foreach(Transform child in togglesCG.transform)
            {
                Destroy(child.gameObject);
            }
        }

        #region Action

        [SerializeField] Actions actions;
        public CanvasGroup actionsCG;
        public NavBar.ToggleController actionsToggleController;
        bool actionsCreated;

        void HandleActions()
        {
            actionsToggleController = navbar.AddToggle("Actions", actionsCG.gameObject);
            var actionsTransform = actionsCG.transform;
            foreach(var item in actions.requests)
            {
                item.sourceType = actions.type;
                var newRequestHandler = CreateRequestHandler(actionsTransform);
                newRequestHandler.HandleRequest(item);
            }

            actionsCreated = true;

        }
        #endregion

        #region Toggles

        [SerializeField] Toggles toggles;
        public CanvasGroup togglesCG;
        public NavBar.ToggleController togglesToggleController;

        bool togglesCreated;

        void HandleToggles()
        {
            togglesToggleController = navbar.AddToggle("Toggles", togglesCG.gameObject);
            var togglesTransform = togglesCG.transform;
            foreach (var item in toggles.requests)
            {
                var newRequestHandler = CreateRequestHandler(togglesTransform);
                item.sourceType = toggles.type;
                newRequestHandler.HandleRequest(item);
            }
            togglesCreated = true;

            togglesToggleController.onValueChange += (state) => {

            };
        }

        public async Task UntilToolsCreated()
        {
            while (!togglesCreated || !actionsCreated) await Task.Delay(500);
        }

        #endregion


        #endregion

    }
}
