using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class MenuModule : MonoBehaviour
    {
        // Start is called before the first frame update
        [System.Serializable]
        public struct Info
        {
            public AudioClip onClose;
            public AudioClip onOpen;
            public bool inheritSize;
        }

        public Info info;

        public MainMenuUI menuUI;
        public CanvasGroup canvasGroup;
        public Transform cameraPosition;


        public bool isActive;

        void GetDependencies()
        {
            menuUI = GetComponentInParent<MainMenuUI>();
            canvasGroup = GetComponent<CanvasGroup>();


        }

        private void OnValidate()
        {
            GetDependencies();
            Show(isActive);
        }



        public void Show(bool val)
        {
            var clip = val ? info.onOpen : info.onClose;

            if (menuUI)
            {
                if (clip != null)
                {
                    menuUI.PlayAudioClip(clip);
                }

                menuUI.OnOpenMenu?.Invoke(this);
            }

            canvasGroup.alpha = val ? 1f : 0f;
            canvasGroup.interactable = val;
            canvasGroup.blocksRaycasts = val;

            isActive = val;
        }
    }
}
