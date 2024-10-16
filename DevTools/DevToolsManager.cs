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
        #region Common Data
        public static DevToolsManager active;

        [Header("Components")]
        public NavBarController navbarController;
        public NavBar navbar;
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
            CreateUI();
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
            await Task.Delay(2500);
            var gameState = currentGameState;
            actionsToggleController.SetInteractable(actions.availableStates.Contains(gameState));
            togglesToggleController.SetInteractable(toggles.availableStates.Contains(gameState));
        }

        #endregion

        #endregion



        #region Create UI

        public async void CreateUI()
        {
            var actionsTask = new Task(async () => {
                await HandleActions();
            });

            var togglesTasks = new Task(async () => {
                await HandleToggles();
            });

            actionsTask.Start();
            togglesTasks.Start();

            await Task.WhenAll(actionsTask, togglesTasks);
        }

        #region Action

        [SerializeField] Actions actions;
        public CanvasGroup actionsCG;
        public NavBar.ToggleController actionsToggleController;

        async Task HandleActions()
        {
            actionsToggleController = navbar.AddToggle("Actions", actionsCG.gameObject);
            var actionsTransform = actionsCG.transform;
            foreach(var item in actions.requests)
            {
                var newRequestHandler = CreateRequestHandler(actionsTransform);
                await newRequestHandler.HandleRequest(item);
            }

        }
        #endregion

        #region Toggles

        [SerializeField] Toggles toggles;
        public CanvasGroup togglesCG;
        public NavBar.ToggleController togglesToggleController;

        async Task HandleToggles()
        {
            togglesToggleController = navbar.AddToggle("Toggles", togglesCG.gameObject);
            var togglesTransform = togglesCG.transform;
            foreach (var item in toggles.requests)
            {
                var newRequestHandler = CreateRequestHandler(togglesTransform);
                await newRequestHandler.HandleRequest(item);
            }
        }

        #endregion

        public RequestHandler CreateRequestHandler(Transform parent)
        {
            var newRequestHandler = Instantiate(requestHandlerPrefab, parent);
            return newRequestHandler;
        }

        #endregion


    }


}
