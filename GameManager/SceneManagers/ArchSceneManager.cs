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
    }
    public class ArchSceneManager : MonoBehaviour
    {
        public static ArchSceneManager active { get; private set; }

        [SerializeField] bool updateSceneInfos;
        public bool isLoading;
        public List<SceneInfo> sceneInfos;
        public Dictionary<ArchScene, SceneInfo> sceneInfoDict;
        public Dictionary<SceneEvent, Action<ArchSceneManager>> eventDict;

        public List<Task> tasksBeforeLoad;
        public List<Func<Task<bool>>> tasksBeforeConfirmLoad { get; set; }
        public List<Task> tasksBeforeActivateScene;

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
            if (active)
            {
                Destroy(gameObject);
                return;
            }
            active = this;

            CreateDictionary();
            DetermineScene();
        }

        void DetermineScene()
        {
            sceneToLoad = CurrentScene();
        }

        private void Start()
        {
            if (active == this)
            {
                ArchGeneric.DontDestroyOnLoad(gameObject);

            }
        }

        void CreateDictionary()
        {
            eventDict = new();
            sceneInfoDict = new();
            foreach (SceneEvent sceneEvent in Enum.GetValues(typeof(SceneEvent)))
            {
                eventDict.Add(sceneEvent, delegate (ArchSceneManager sceneManager) { }); 
            }

            foreach(var sceneInfo in sceneInfos)
            {
                if (sceneInfoDict.ContainsKey(sceneInfo.scene)) continue;
                sceneInfoDict.Add(sceneInfo.scene, sceneInfo);
            }
        }

        public void AddListener<T>(SceneEvent trigger, Action<ArchSceneManager> action, T caller) where T: Component
        {

            eventDict[trigger] += HandleAction;

            void HandleAction(ArchSceneManager sceneManager)
            {
                Debugger.UI(5300, $"Caller is {caller}");
                if(!caller)
                {
                    Debugger.UI(5301, $"Caller is null true, Removing Event");
                    eventDict[trigger] -= HandleAction;
                    return;
                }

                action(this);
            }
        }

        public void AddListener<T>(SceneEvent trigger, Action action, T caller) where T : Component
        {
            eventDict[trigger] += HandleTrigger;

            void HandleTrigger(ArchSceneManager sceneManager)
            {
                if(caller == null)
                {
                    eventDict[trigger] -= HandleTrigger;
                    return;
                }

                action();
            }
        }

        public void AddListeners<T>(List<(SceneEvent, Action<ArchSceneManager>)> triggerActions, T caller) where T: Component
        {
            foreach (var (trigger, action) in triggerActions)
            {
                AddListener(trigger, action, caller);

            }
        }

        public Action<ArchSceneManager> EventInvoker(SceneEvent trigger)
        {
            return eventDict[trigger];
        }

        async public void LoadScene(ArchScene archScene, int index = -1, bool async = true)
        {
            this.sceneToLoad = sceneInfoDict[archScene];

            var sceneName = index == -1 ? sceneToLoad.main : sceneToLoad.subScenes[index];

            tasksBeforeConfirmLoad = new();


            eventDict[SceneEvent.BeforeConfirmLoad]?.Invoke(this);
            foreach (var choice in tasksBeforeConfirmLoad)
            {
                if (!await choice()) return;
            }


            await Task.Delay(125);


            tasksBeforeLoad = new();
            eventDict[SceneEvent.BeforeLoadScene]?.Invoke(this);
            await Task.WhenAll(tasksBeforeLoad);

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

            eventDict[SceneEvent.BeforeActivateScene]?.Invoke(this);

            await Task.WhenAll(tasksBeforeActivateScene);

            scene.allowSceneActivation = true;

            eventDict[SceneEvent.OnLoadScene]?.Invoke(this);
            await Task.Delay(2500);
            isLoading = false;
            eventDict[SceneEvent.OnLoadSceneLate]?.Invoke(this);
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
