using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class WorldModuleCore : MonoBehaviour
    {
        public static WorldModuleCore active { get; private set; }


        public Action<ItemContainer> OnOpenContainer;

        public List<ModuleInfo> activeModules;


        [Serializable]
        public struct Prefabs
        {
            public GameObject containerModule;
        }

        [SerializeField] Prefabs prefabs;

        public ModuleInfo CreateModule(GameObject module, Vector3 location)
        {
            if (!module.GetComponent<ModuleInfo>()) return null;
            activeModules ??= new();

            var moduleInfo = Instantiate(module, transform).GetComponent<ModuleInfo>();


            moduleInfo.transform.position = location;

            activeModules.Add(moduleInfo);

            return moduleInfo;

        }

        private void Awake()
        {
            active = this;
        }

        void HandleIGGUI()
        {

        }


        public async void HandleItemContainer(ItemContainer container)
        {
            if (prefabs.containerModule == null) return;
            OnOpenContainer?.Invoke(container);

            var containerModule = CreateModule(prefabs.containerModule, new Vector3(Screen.width / 2, Screen.height / 2, 0)).GetComponent<ItemContainerModule>();

            container.OnForceClose += OnForceClose;

            container.OnOpenContainer?.Invoke(container);
            container.events.OnOpenContainer?.Invoke(container);

            containerModule.SetItemContainer(container);
            await containerModule.UntilClose();


            container.OnForceClose -= OnForceClose;
            container.OnCloseContainer?.Invoke(container);
            container.events.OnCloseContainer?.Invoke(container);

            void OnForceClose(ItemContainer itemContainer)
            {
                containerModule.Close();
            }
        }
    }

}