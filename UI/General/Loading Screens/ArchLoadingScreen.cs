using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class ArchLoadingScreen : MonoBehaviour
    {
        public static ArchLoadingScreen active;

        [System.Serializable]
        public struct Components
        {
            public Image loadingBar;
            public CanvasGroup canvasGroup;
        }

        [SerializeField] Components comps;

        ArchSceneManager sceneManager;
        public bool loading;

        [Header("Inspector Properties")]
        [SerializeField] bool enableCanvasGroup;

        void Start()
        {
            GetDependencies();
            SetCanvasGroup(false);
        }
        private void Awake()
        {
            active = this;

        }

        private void OnValidate()
        {
            SetCanvasGroup(enableCanvasGroup);

        }
        // Update is called once per frame
        void Update()
        {
        
        }

        void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;
            if (sceneManager)
            {
                sceneManager.OnLoadStart += OnLoadStart;
                sceneManager.WhileLoading += WhileLoading;
                sceneManager.OnLoadEnd += OnLoadEnd;
            }
            
        }

        void SetCanvasGroup(bool active)
        {
            if (comps.canvasGroup == null) return;

            transform.SetAsLastSibling();
            ArchUI.SetCanvas(comps.canvasGroup, active);
        }

        void OnLoadStart(AsyncOperation operation)
        {
            loading = true;
            SetCanvasGroup(true);
            
        }

        void WhileLoading(AsyncOperation operation)
        {
            if (comps.loadingBar == null) return;
            if (sceneManager == null) return;
            Debugger.InConsole(91275, $"Loading progress {operation.progress}");

            comps.loadingBar.fillAmount = sceneManager.progressValue;
        }

        void OnLoadEnd(AsyncOperation operation)
        {
            loading = false;
            SetCanvasGroup(false);
        }
    }
}
