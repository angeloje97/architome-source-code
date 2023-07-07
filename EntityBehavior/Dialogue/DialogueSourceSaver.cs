using Language.Lua;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace Architome
{
    public class DialogueSourceSaver : MonoBehaviour
    {
        [SerializeField] string referenceName;
        [SerializeField] DialogueSource source;
        [SerializeField] DialogueHistory currentHistory;
        void Start()
        {
            GetDependencies();
            HandleLoad();
        }

        void GetDependencies()
        {
            currentHistory = DialogueHistory.active;
            source = GetComponent<DialogueSource>();

            var saveHandler = SaveSystem.active;
            var sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, () => {
                    
                }, this);
            }

            if (saveHandler)
            {
                saveHandler.AddListener(SaveEvent.BeforeSave, (SaveSystem sytem, SaveGame currentSave) => {
                    HandleSave();
                }, this);
            }
        }

        void HandleLoad()
        {
            if (currentHistory == null) return;
            if (source == null) return;

            source.initialDataSet = currentHistory.LoadDialogue(referenceName, source.initialDataSet);
        }

        void HandleSave()
        {
            if (currentHistory == null) return;
            if (source == null) return;

            currentHistory.SaveDialogue(referenceName, source.initialDataSet);

        }
    }
}
