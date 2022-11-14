using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class WorldModuleCore : MonoBehaviour
    {
        public static WorldModuleCore active { get; private set; }


        public Action<ArchChest> OnChestOpen;

        public List<ModuleInfo> activeModules;


        [Serializable]
        public struct Prefabs
        {
            public GameObject chestModule;
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


        public void HandleChest(ArchChest chest)
        {
            return;
            OnChestOpen?.Invoke(chest);
            if (prefabs.chestModule == null) return;

            var chestModule = CreateModule(prefabs.chestModule, new Vector3(Screen.width / 2, Screen.height / 2, 0)).GetComponent<ChestModule>();

            chestModule.SetChest(chest);


        }
    }

}