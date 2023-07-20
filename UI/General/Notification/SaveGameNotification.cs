using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class SaveGameNotification : MonoBehaviour
    {
        [SerializeField] Animator animator;
        [SerializeField] SaveSystem system;
        [SerializeField] CanvasGroup canvasGroup;

        private void Awake()
        {
            SingletonManger.HandleSingleton(GetType(), gameObject);
        }

        private void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            canvasGroup.SetCanvas(false);

            system = SaveSystem.active;

            system.AddListener(SaveEvent.OnSave, async (SaveSystem system, SaveGame currentSave) => {
                animator.SetBool("IsPlaying", true);
                await canvasGroup.SetCanvasAsync(true, .5f);
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
                await Task.Delay(1000);
                await canvasGroup.SetCanvasAsync(false, .5f);
                animator.SetBool("IsPlaying", false);

            }, this);
        }
    }
}
