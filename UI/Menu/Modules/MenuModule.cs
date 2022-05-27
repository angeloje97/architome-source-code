using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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

        public List<GameObject> sections;


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

        private void Start()
        {
            SetSection(0);
        }

        public void SetSection(int index)
        {
            if (index >= sections.Count) return;

            for (int i = 0; i < sections.Count; i++)
            {
                sections[i].gameObject.SetActive(i == index);
            }
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
