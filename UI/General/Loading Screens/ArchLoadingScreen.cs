using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class ArchLoadingScreen : MonoBehaviour
    {
        public static ArchLoadingScreen active;

        [Serializable]
        public struct Components
        {
            //public Image loadingBar;
            public CanvasGroup canvasGroup;
        }

        [Serializable]
        public struct LoadingBar
        {
            public CanvasGroup canvasGroup;
            public Image progressBar;
            public TextMeshProUGUI status;

            public bool handleMapLoadingScreen;
        }

        [Serializable]
        public struct LoadingStatus
        {
            public Animator animator;
            public CanvasGroup canvasGroup;

            public void HandleLoad(bool loading)
            {
                if (animator == null) return;
                animator.SetBool("IsPlaying", loading);
                ArchUI.SetCanvas(canvasGroup, loading);
            }
        }


        [SerializeField] Components comps;
        [SerializeField] LoadingBar loadingBar;
        [SerializeField] LoadingStatus loadingStatus;

        ArchSceneManager sceneManager;

        public bool loading;

        [Header("Inspector Properties")]
        [SerializeField] bool enableCanvasGroup;
        [SerializeField] bool enableLoadingBar;

        void Start()
        {
            GetDependencies();
            SetCanvasGroup(false);
            HandleMapGeneration();

        }
        private void Awake()
        {
            active = this;

        }

        private void OnValidate()
        {
            SetCanvasGroup(enableCanvasGroup);
            SetLoadingBar(enableLoadingBar);

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
                sceneManager.OnLoadScene += OnLoadScene;
                sceneManager.BeforeLoadScene += BeforeLoadScene;
            }


        }

        private void OnDestroy()
        {
            if (sceneManager)
            {
                sceneManager.OnLoadScene -= OnLoadScene;
                sceneManager.BeforeLoadScene -= BeforeLoadScene;
            }
        }


        async void HandleMapGeneration()
        {
            var mapInfo = MapInfo.active;
            if (mapInfo == null) return;
            if (mapInfo.generationComplete) return;
            var entityGenerator = MapEntityGenerator.active;
            if (entityGenerator == null) return;

            Debugger.UI(8414, $"Setting loading bar to {entityGenerator}");
            SetLoadingBar(true);
            SetCanvasGroup(true);
            loadingStatus.HandleLoad(false);
            var loadingBarActive = true;
            

            entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => {
                loadingBarActive = false;
                var text = $"Generated Entities";
                loadingBar.progressBar.fillAmount = 1;
                loadingBar.status.text = text;
            };

            entityGenerator.OnGenerateEntity += (MapEntityGenerator generator, EntityInfo entity) => {
                float current = generator.entitiesSpawned;
                float goal = generator.expectedEntities;
                var text = $"Generating Entities ({(int) ((current/goal)*100)}%)";
                loadingBar.progressBar.fillAmount = .666f + ((current/goal) * .333f);
                loadingBar.status.text = text;

            };

            var roomGenerator = MapRoomGenerator.active;
            roomGenerator.OnSpawnRoom += (MapRoomGenerator roomGenerator, RoomInfo room) => {
                float goal = roomGenerator.roomsToGenerate;
                float current = roomGenerator.roomsGenerated;
                var text = $"Generating rooms ({(int) ((current/goal)*100)}%)";


                Debugger.UI(8415, $"{current} / {goal}");
                loadingBar.progressBar.fillAmount = (current / goal) * .33f;
                loadingBar.status.text = text;
            };

            var mapAdjustMent = MapAdjustments.active;

            mapAdjustMent.WhileLoading += (MapAdjustments adjustment, float progress) => {
                loadingBar.status.text = $"Adjusting Pathfinding Graph ({(int) (progress*100)}%)";
                loadingBar.progressBar.fillAmount = .333f + (progress * .333f);
            };
                        

            while (loadingBarActive)
            {
                await Task.Yield();
            }


            await Task.Delay(1000);
            SetLoadingBar(false);
            SetCanvasGroup(false);

        }
        void SetCanvasGroup(bool active)
        {
            if (comps.canvasGroup == null) return;

            transform.SetAsLastSibling();
            ArchUI.SetCanvas(comps.canvasGroup, active);
            comps.canvasGroup.blocksRaycasts = false;
            comps.canvasGroup.interactable = false;
        }

        void SetLoadingBar(bool active)
        {
            if (loadingBar.canvasGroup == null) return;

            ArchUI.SetCanvas(loadingBar.canvasGroup, active);
            loadingBar.progressBar.fillAmount = 0f;
            loadingBar.status.text = "";
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            loading = true;
            SetCanvasGroup(true);
            loadingStatus.HandleLoad(true);
        }

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            loading = false;
            SetCanvasGroup(false);
            loadingStatus.HandleLoad(false);

            ArchAction.Yield(() => {
                if (this == null) return;
                HandleMapGeneration();
            });
        }
    }
}
