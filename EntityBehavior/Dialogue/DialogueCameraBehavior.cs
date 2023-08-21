using System;
using System.Collections;
using UnityEngine;

namespace Architome.Assets.Source.Scripts.EntityBehavior.Dialogue
{
    public class DialogueCameraBehavior : MonoBehaviour
    {
        DialogueSource dialogueSource;
        CameraAnchor cameraAnchor;
        void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            dialogueSource = GetComponentInParent<DialogueSource>();
            cameraAnchor = CameraAnchor.active;

            if (dialogueSource && cameraAnchor)
            {
                dialogueSource.OnStartConversation += async () => {
                    var isListening = true;

                    Action stopListening = () => { isListening = false; };

                    dialogueSource.OnDialogueDisabled += stopListening;

                    await cameraAnchor.SetTargetTemp(dialogueSource.transform, (object obj) => isListening);

                    dialogueSource.OnDialogueDisabled -= stopListening;
                };
            }
        }


    }
}