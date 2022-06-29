using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class IGGUIInfo : MonoBehaviour
    {
        public static IGGUIInfo active;
        // Start is called before the first frame update
        public List<GameObject> properties;
        public List<GameObject> modules;
        public Action<ModuleInfo> OnModuleEnableChange;



        PromptHandler promptHandler;
        WorldModuleCore worldModuleCore;
        public List<Transform> extraModules;
        public void GetProperties()
        {
            properties = new List<GameObject>();

            foreach (Transform child in transform)
            {
                properties.Add(child.gameObject);
            }

            ArchInput.active.OnEscape += OnEscape;

            worldModuleCore = WorldModuleCore.active;
            promptHandler = PromptHandler.active;
        }
        void Start()
        {
            GetProperties();
        }

        private void Awake()
        {
            active = this;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnEscape()
        {
            if (SetExtra())
            {
                return;
            }

            //if (SetPrompts())
            //{
            //    return;
            //}

            //if (SetWorldModules())
            //{
            //    return;
            //}

            if (SetModules(false))
            {
                return;
            }

            if (SetWidgets(false))
            {
                return;
            }

        }

        bool SetWorldModules()
        {
            bool changed = false;

            foreach (var module in worldModuleCore.GetComponentsInChildren<ModuleInfo>())
            {
                changed = true;
                module.SetActive(false);
            }

            return changed;
        }

        bool SetPrompts()
        {
            bool changed = false;

            foreach (var prompt in promptHandler.GetComponentsInChildren<PromptInfo>())
            {
                if (!prompt.isActive) continue;
                changed = true;
                prompt.EndOptions();
            }

            return changed;
        }

        bool SetExtra()
        {
            foreach (var extra in extraModules)
            {
                bool changed = false;
                foreach (var module in extra.GetComponentsInChildren<ModuleInfo>())
                {
                    if (module.forceActive) return true;
                    if (module.isActive)
                    {
                        changed = true;
                        module.SetActive(false);
                    }

                    if (changed)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void SetUI(bool val)
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null) return;

            ArchUI.SetCanvas(canvasGroup, val);
        }

        public ActionBarsInfo ActionBarInfo()
        {
            foreach (GameObject property in properties)
            {
                if (property.GetComponent<ActionBarsInfo>()) { return property.GetComponent<ActionBarsInfo>(); }
            }

            return null;
        }

        public void ToggleModule(int index)
        {
            if (index >= modules.Count) { return; }
            if (modules[index].GetComponent<CanvasGroup>() == null) { return; }

            var moduleInfo = modules[index].GetComponent<ModuleInfo>();
            moduleInfo.SetActive(!moduleInfo.isActive);


        }
        
        public bool BlockingInput()
        {
            foreach (var module in modules)
            {
                var info = module.GetComponent<ModuleInfo>();

                if (info.isActive && info.blocksInput)
                {
                    return true;
                }
            }

            return false;
        }

        public void SetModule(int index, bool val)
        {
            if (index >= modules.Count) { return; }

            if (!modules[index].GetComponent<ModuleInfo>()) { return; }

            modules[index].GetComponent<ModuleInfo>().SetActive(val);
        }

        public bool SetModules(bool val)
        {
            var changed = false;
            foreach (var moduleObject in modules)
            {
                var module = moduleObject.GetComponent<ModuleInfo>();

                if (module == null) continue;

                if (module.isActive == val) continue;

                if (module.GetType() != typeof(ModuleInfo)) continue;

                module.SetActive(val);
                
                changed = true;
                
            }

            return changed;
        }

        public bool SetWidgets(bool val)
        {
            var changed = false;

            foreach (var widgetObject in modules)
            {
                var widget = widgetObject.GetComponent<WidgetInfo>();

                if (widget == null) continue;

                if (widget.isActive == val) continue;

                if (widget.GetType() != typeof(WidgetInfo)) continue;

                widget.SetActive(val);
                changed = true;
            }

            return changed;
        }


    }
}

