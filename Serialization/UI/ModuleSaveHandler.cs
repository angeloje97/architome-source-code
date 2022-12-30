using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Serialization
{
    public class ModuleSaveHandler : MonoBehaviour
    {
        [SerializeField] bool useSave;

        ModuleInfo currentModule;

        private void Start()
        {
            if (!useSave) return;
            GetDependencies();
            AcquirePosition();
        }

        void AcquirePosition()
        {
            SaveSystem.Operate((SaveGame save) => {
                save.UI.Modules.LoadModule(currentModule);
            });
        }

        void GetDependencies()
        {
            currentModule = GetComponent<ModuleInfo>();
            var sceneManager = ArchSceneManager.active;


            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveSystem.Operate((SaveGame save) => {
                save.UI.Modules.SaveModule(currentModule);
            });
        }

    }
}
