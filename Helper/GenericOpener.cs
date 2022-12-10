using Architome.Debugging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GenericOpener : MonoBehaviour
    {
        ModuleInfo igModule;
        PauseMenu pauseMenu;
        void Start()
        {
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void GetDependencies()
        {
            var debugger = IGDebugger.active;
            pauseMenu = PauseMenu.active;
            if (debugger)
            {
                igModule = debugger.GetComponent<ModuleInfo>();
            }
        }

        public void ToggleDebugger()
        {
            if (igModule == null) return;
            igModule.Toggle();
        }

        public void TogglePauseMenu()
        {
            if (pauseMenu == null) return;
            pauseMenu.ToggleMenu();
        }
    }
}
