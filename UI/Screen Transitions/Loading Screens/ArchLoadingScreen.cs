using System;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Architome.Enums;

namespace Architome
{
    public class ArchLoadingScreen : MonoBehaviour
    {
        public static ArchLoadingScreen active;

        #region Components

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
        #endregion

        #region Common Data

        ArchSceneManager sceneManager;

        public bool loading { get; set; }
        public bool loadingBarActive { get; set; }

        [Header("Inspector Properties")]
        [SerializeField] bool enableCanvasGroup;
        [SerializeField] bool enableLoadingBar;

        #endregion

        void Start()
        {
            GetDependencies();
            SetCanvasGroup(false);
            HandleMapGeneration();

        }
        private void Awake()
        {
            if (active && active != this)
            {
                Destroy(gameObject);
                return;
            }
            active = this;

        }

        private void OnValidate()
        {
            SetCanvasGroup(enableCanvasGroup);
            SetLoadingBar(enableLoadingBar);

        }

        void GetDependencies()
        {
            sceneManager = ArchSceneManager.active;
            if (sceneManager)
            {

                sceneManager.AddListener(SceneEvent.OnLoadScene, OnLoadScene, this);
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
                sceneManager.AddListener(SceneEvent.BeforeRevealScene, BeforeRevealScene, this);
            }
        }

        void BeforeRevealScene(ArchSceneManager sceneManager, List<Func<Task>> funcs)
        {
            funcs.Add(async () => {
                while (loadingBarActive) await Task.Yield();
            });
        }

        public void SetLoadingBar(string text, float percentage, float offset)
        {
            loadingBar.progressBar.fillAmount = (percentage*.333f) + offset;
            var percentText = (int) Mathf.Clamp(percentage*100, 0, 100);
            var statusText = $"{text} ({percentText}%)";
            loadingBar.status.text = statusText;
        }

        public void SetLoadingBarText(string text)
        {
            loadingBar.status.text = text;
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
            this.loadingBarActive = true;

            entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => {
                loadingBarActive = false;
                SetLoadingBarText("Dungeon Generation Complete");
            };

            entityGenerator.OnGenerateEntity += (MapEntityGenerator generator, EntityInfo entity) => {
                float current = generator.entitiesSpawned;
                float goal = generator.expectedEntities;
                var text = $"Generating Entities";
                float percentage = current/goal;
                SetLoadingBar(text, percentage, .666f);
            };

            var roomGenerator = MapRoomGenerator.active;
            roomGenerator.OnSpawnRoom += (MapRoomGenerator roomGenerator, RoomInfo room) => {
                float goal = roomGenerator.roomsToGenerate;
                float current = roomGenerator.roomsGenerated;
                var text = $"Generating rooms";
                var percentage = current / goal;
                SetLoadingBar(text, percentage, 0f);
            };

            var mapAdjustMent = MapAdjustments.active;

            mapAdjustMent.WhileLoading += (MapAdjustments adjustment, float progress) => {
                loadingBar.status.text = $"Adjusting Pathfinding Graph";
                var text = "Adjusting Pathfinding Graph";
                SetLoadingBar(text, progress, .33f);
            };
                        

            while (loadingBarActive)
            {
                await Task.Yield();
            }

            await Task.Delay(500);
            SetLoadingBar(false);
            SetCanvasGroup(false);
            await Task.Delay(500);
            this.loadingBarActive = false;

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

            ArchAction.Delay(() => {
                if (this == null) return;
                HandleMapGeneration();
            }, 1f);
        }

        public async Task FinishLoadingBar()
        {
            while (loadingBarActive) await Task.Yield();
        }
    }
}
