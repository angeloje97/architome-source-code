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
        }
    }
}