using System.Collections;
using Architome.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

namespace Architome
{
    public enum SceneEvent
    {
        BeforeLoadScene,
        BeforeActivateScene,
        BeforeConfirmLoad,
        OnLoadScene,
        OnLoadSceneLate,
        BeforeRevealScene,
        OnRevealScene,
    }
    public class ArchSceneManager : MonoBehaviour
    {
        public static ArchSceneManager active { get; private set; }

        [SerializeField] bool updateSceneInfos;
        public bool isLoading;
        public List<SceneInfo> sceneInfos;
        public Dictionary<ArchScene, SceneInfo> sceneInfoDict;
        public ArchEventHandler<SceneEvent, ArchSceneManager> events;

        public List<Func<Task>> tasksBeforeLoad { get; set; }
        public List<Task> tasksBeforeLoadPriority { get; set; }
        public List<Func<Task<bool>>> tasksBeforeConfirmLoad { get; set; }
        public List<Task> tasksBeforeActivateScene { get; set; }
        public Action<AsyncOperation> OnLoadStart { get; set; }
        public Action<AsyncOperation> WhileLoading { get; set; }
        public Action<AsyncOperation> OnLoadEnd { get; set; }


        [SerializeField] SceneInfo currentScene;
        public SceneInfo sceneToLoad 
        {
            get
            {
                return currentScene;
            }
            private set
            {
                currentScene = value;
            }
        }

        public float progressValue;


        private void Awake()
        {

            SingletonManger.HandleSingleton(GetType(), gameObject, true, onSuccess: () => {
                active = this;
                CreateDictionary();
                DetermineScene();
            });

        }

        void DetermineScene()
        {
            sceneToLoad = CurrentScene();
        }

        private async void Start()
        {
            await Task.Delay(1000);
            await TasksBeforeRevealScene();
        }

        void CreateDictionary()
        {
            events = new(this);
            sceneInfoDict = new();
            foreach(var sceneInfo in sceneInfos)
            {
                if (sceneInfoDict.ContainsKey(sceneInfo.scene)) continue;
                sceneInfoDict.Add(sceneInfo.scene, sceneInfo);
            }
        }

        public void AddListener<T>(SceneEvent trigger, Action<ArchSceneManager> action, T caller) where T: Component
        {
            events.AddListener(trigger, action, caller);
            
        }

        public Action AddListener<T>(SceneEvent trigger, Action action, T caller) where T : Component
        {
            return events.AddListener(trigger, action, caller);
        }

        public void AddListener(SceneEvent trigger, Action<ArchSceneManager, List<Func<Task>>> action, Component caller)
        {
            events.AddListenerTask(trigger, action, caller);
        }

        async public Task LoadSceneAsync(ArchScene archScene, int index = -1)
        {
            LoadScene(archScene, index, true);
            await DoneLoading();
        }

        async public Task DoneLoading()
        {
            await Task.Delay(130);
            while (isLoading) await Task.Yield();
        }

        async public void LoadScene(ArchScene archScene, int index = -1, bool async = true)
        {
            if (isLoading) return;
            this.sceneToLoad = sceneInfoDict[archScene];

            var sceneName = index == -1 ? sceneToLoad.main : sceneToLoad.subScenes[index];



            tasksBeforeConfirmLoad = new();
            events.Invoke(SceneEvent.BeforeConfirmLoad, this);
            foreach (var choice in tasksBeforeConfirmLoad)
            {
                if (!await choice()) return;
            }


            await Task.Delay(125);


            tasksBeforeLoad = new();
            tasksBeforeLoadPriority = new();
            events.Invoke(SceneEvent.BeforeLoadScene, this);

            await Task.WhenAll(tasksBeforeLoadPriority);


            foreach(var task in tasksBeforeLoad)
            {
                await task();
            }

            if (!async)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }
            isLoading = true;
            var scene = SceneManager.LoadSceneAsync(sceneName);
            scene.allowSceneActivation = false;
            OnLoadStart?.Invoke(scene);

            while (scene.progress < .9f)
            {
                progressValue = scene.progress / .9f;
                WhileLoading?.Invoke(scene);

                await Task.Yield();
            }

            OnLoadEnd?.Invoke(scene);
            

            tasksBeforeActivateScene = new();

            events.Invoke(SceneEvent.BeforeActivateScene, this);

            await Task.WhenAll(tasksBeforeActivateScene);

            scene.allowSceneActivation = true;

            events.Invoke(SceneEvent.OnLoadScene, this);
            await Task.Delay(2500);
            isLoading = false;
            events.Invoke(SceneEvent.OnLoadSceneLate, this);

            await TasksBeforeRevealScene();
        }

        public async Task TasksBeforeRevealScene()
        {
            var tasks = events.Invoke(SceneEvent.BeforeRevealScene, this);

            Debugger.System(6901, $"Tasks after invoking event {tasks.Count}");

            foreach (var task in tasks) await task();
            events.Invoke(SceneEvent.OnRevealScene, this);
        }

        public bool IsScene(ArchScene scene)
        {
            return sceneToLoad.scene == scene;
        }

        public SceneInfo CurrentScene()
        {
            var sceneName = SceneManager.GetActiveScene().name;

            foreach(var sceneInfo in sceneInfos)
            {
                if (sceneInfo.main.Equals(sceneName))
                {
                    return sceneInfo;
                }

                if (sceneInfo.subScenes != null)
                {
                    foreach(var subScene in sceneInfo.subScenes)
                    {
                        if (subScene.Equals(sceneName))
                        {
                            return sceneInfo;
                        }
                    }
                }
            }

            return null;
        }

        public void OnValidate()
        {
            if (!updateSceneInfos) return;
            updateSceneInfos = false;
            sceneInfos ??= new();

            foreach(var sceneInfo in sceneInfos)
            {
                sceneInfo.Update();
            }
        }


        [Serializable]
        public class SceneInfo
        {
            [HideInInspector] public string name;
            public ArchScene scene;

            public bool enableSaveScene;

            public string main;

            public List<string> subScenes;
            public void Update()
            {
                name = scene.ToString();
            }
            public bool Equals(SceneInfo other)
            {

                if (other.main == main)
                {
                    return true;
                }

                return false;
            }
        }
    }
}
