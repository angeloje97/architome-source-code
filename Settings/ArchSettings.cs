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

        public void HandleDirtyConflicts()
        {
            if (reactiveNavBars != null)
            {
                foreach(var navBar in reactiveNavBars)
                {
                    navBar.OnCanNavigateChange += (NavBar navBar, List<bool> checks) => {
                        HandleDirty(navBar, checks);
                    };
                }

            }

            var mainMenuUI = MainMenuUI.active;
            var pauseMenu = PauseMenu.active;
            var canvasController = GetComponentInParent<CanvasController>();


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
            }

        }

        async void HandleCloseCanvasController(CanvasController controller)
        {
            if (!dirty) return;
            controller.checks.Add(false);

            var confirmLeave = await ConfirmLeave();

            if (!confirmLeave) return;

            HandleLeaveDirty();
            dirty = false;
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

            var confirmLeave = await ConfirmLeave();

            if (confirmLeave)
            {
                HandleLeaveDirty();
                dirty = false;
                mainMenu.OpenModule(desiredModule.gameObject);
            }

        }

        async Task<bool> ConfirmLeave()
        {
            var promptHandler = PromptHandler.active;
            if (promptHandler == null) return true;

            var userChoice = await promptHandler.GeneralPrompt(PromptData());

            if(userChoice.optionPicked.text == "Cancel")
            {
                return false;
            }

            return true;
        }
        public virtual async void HandleDirty(NavBar navBar, List<bool> checks)
        {
            if (!dirty) return;
            var promptHandler = PromptHandler.active;

            if (promptHandler == null)
            {
                dirty = false;
                HandleLeaveDirty();
                return;
            }
            checks.Add(false);

            var desiredIndex = navBar.ActiveIndex();
            navBar.UpdateFromIndex(navBar.previousIndex);

            var confirmLeave = await ConfirmLeave();

            if(!confirmLeave)
            {
                return;
            }

            dirty = false;
            navBar.UpdateFromIndex(desiredIndex);
            HandleLeaveDirty();
            return;

        }

        public virtual void HandleLeaveDirty()
        {

        }


        public virtual PromptInfoData PromptData()
        {
            return new()
            {
                title = "Settings",
                question = "Are you sure you want to continue without applying?",
                options = new() { 
                    new("Leave Settings"),
                    new("Cancel") {isEscape = true}
                },
                blocksScreen = true,
            };
        }

    }
}
