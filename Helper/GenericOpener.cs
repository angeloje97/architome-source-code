using Architome.Debugging;
using Architome.DevTools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GenericOpener : MonoBehaviour
    {
        ModuleInfo igModule;
        PauseMenu pauseMenu;
        ModuleInfo devToolsModule;

        [SerializeField]
        List<ModuleInfo> modules;

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

            var devTools = DevToolsManager.active;

            if (devTools)
            {
                devToolsModule = devTools.GetComponent<ModuleInfo>();
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

        public void ToggleDevTools()
        {
            if (devToolsModule == null) return;
            devToolsModule.Toggle();
        }

        public void ToggleModule(int index)
        {
            if (modules == null) return;
            if (modules.Count <= index) return;

            modules[index].Toggle();
        }

        public void SetModule(int index, bool value)
        {
            if (modules == null) return;
            if (modules.Count <= index) return;

            modules[index].SetActive(value);
        }

        public void SetAllModules(bool value)
        {
            foreach(var module in modules)
            {
                module.SetActive(value);
            }
        }
    }
}
