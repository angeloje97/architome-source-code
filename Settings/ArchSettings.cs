using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

namespace Architome.Settings
{
    public class ArchSettings : MonoBehaviour
    {
        [Header("States")]
        public bool dirty;
        public List<NavBar> reactiveNavBars;

        public Action OnLeaveDirty;



        bool hasCanvasController;
        bool chosenApply;

        public void HandleDirtyConflicts()
        {
            if (reactiveNavBars != null)
            {
                foreach (var navBar in reactiveNavBars)
                {
                    navBar.OnCanNavigateChange += (NavBar navBar, List<bool> checks) =>
                    {
                        HandleDirty(navBar, checks);
                    };
                }

            }

            var mainMenuUI = MainMenuUI.active;
            var pauseMenu = PauseMenu.active;
            var canvasController = GetComponentInParent<CanvasController>();
            var archSceneManager = ArchSceneManager.active;


            if (mainMenuUI)
            {
                mainMenuUI.OnCanChangeModuleCheck += HandleCanChangeModuleCheck;
            }

            if (pauseMenu)
            {
                pauseMenu.OnTryOpenPause += HandleTryOpenPause;
            }

            if (canvasController)
            {
                canvasController.OnCanCloseCheck += HandleCloseCanvasController;
                hasCanvasController = true;
            }

            if (archSceneManager)
            {
                archSceneManager.AddListener(SceneEvent.BeforeConfirmLoad, HandleBeforeConfirmLoad, this);
            }

        }

        protected void HandleDirtyChange(Action<bool> onDirtyChange)
        {
            var original = dirty;

            _= World.UpdateComponent((float deltaTime) => {
                if (original == dirty) return;
                original = dirty;
                onDirtyChange?.Invoke(dirty);
            }, this);
        }
        void HandleBeforeConfirmLoad(ArchSceneManager sceneManager)
        {
            if (!dirty) return;

            sceneManager.tasksBeforeConfirmLoad.Add(async () => { return await ConfirmLeave("Scene Manager");});
        }

        async void HandleCloseCanvasController(CanvasController controller)
        {
            if (!dirty) return;
            controller.checks.Add(false);
            controller.haltChange = true;
            var confirmLeave = await ConfirmLeave("Canvas Controller");
            controller.haltChange = false;
            Debugger.UI(7691, $"Confirm Leave {confirmLeave}");
            if (!confirmLeave) return;

            controller.SetCanvas(false);


        }

        void HandleTryOpenPause(PauseMenu pauseMenu)
        {

        }

        public virtual async void HandleCanChangeModuleCheck(MainMenuUI mainMenu, List<bool> checks)
        {
            if (!dirty) return;
            checks.Add(false);
            var desiredModule = mainMenu.desiredModule;

            var confirmLeave = await ConfirmLeave("Module");

            if (confirmLeave)
            {
                //if(dirty) HandleLeaveDirty();
                //dirty = false;
                if (desiredModule)
                {
                    mainMenu.OpenModule(desiredModule);
                }
                else
                {
                    mainMenu.OpenModule(null);
                }

            }

        }

        protected async Task<bool> ConfirmLeave(string sender)
        {
            Debugger.UI(2338, $"Sender for confirm leave prompt is {sender}");
            var promptHandler = PromptHandler.active;
            if (promptHandler == null) return true;

            var userChoice = await promptHandler.GeneralPrompt(PromptData());

            Debugger.UI(2335, $"The user picked {userChoice.optionPicked.text}");

            if(userChoice.optionPicked.text == "Cancel")
            {
                return false;
            }

            return true;
        }

        public virtual async void HandleDirty(NavBar navBar, List<bool> checks)
        {
            if (!dirty) return;

            navBar.stallNavigation = true;

            var confirmLeave = await ConfirmLeave("Nav Bar");

            if (!confirmLeave)
            {
                checks.Add(false);
            }

            navBar.stallNavigation = false;

        }

        public virtual void HandleLeaveDirty()
        {

        }
        public virtual PromptInfoData PromptData()
        {
            chosenApply = false;
            return new()
            {
                title = "Settings",
                question = "Are you sure you want to continue without applying?",
                options = new() {
                    new("Apply Changes", (OptionData data) => {
                        chosenApply = true;
                        HandleChooseApply(); 
                    }),
                    new("Discard Changes", (OptionData data) => {
                        HandleLeaveDirty();
                        dirty = false;
                    }),
                    new("Cancel") {isEscape = true}
                },
                blocksScreen = true,
            };
        }
        public virtual void HandleChooseApply()
        {

        }

        public virtual void OnDirtyChange(bool dirty)
        {

        }
    }
}
