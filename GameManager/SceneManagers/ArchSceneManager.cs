using System.Collections;
using Architome.Enums;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System;

namespace Architome
{
    public class ArchSceneManager : MonoBehaviour
    {
        public static ArchSceneManager active { get; private set; }

        [SerializeField] bool updateSceneInfos;
        public bool isLoading;
        public List<SceneInfo> sceneInfos;
        public Dictionary<ArchScene, SceneInfo> sceneInfoDict;

        public List<Task> tasksBeforeLoad;
        public List<Task<bool>> tasksBeforeConfirmLoad;
        public List<Task> tasksBeforeActivateScene;

        public Action<ArchSceneManager> BeforeLoadScene { get; set; }
        public Action<ArchSceneManager> BeforeActivateScene { get; set; }
        public Action<ArchSceneManager> OnLoadScene { get; set; }
        public Action<ArchSceneManager> BeforeConfirmLoad { get; set; }

        public Action<AsyncOperation> OnLoadStart { get; set; }
        public Action<AsyncOperation> WhileLoading { get; set; }
        public Action<AsyncOperation> OnLoadEnd { get; set; }


        public string sceneToLoad;
        public float progressValue;

        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
        }

        private void Start()
        {
            if (active == this)
            {
                transform.SetParent(null);
                DontDestroyOnLoad(gameObject);

            }
        }

        async public void LoadScene(string sceneName, bool async = true)
        {
            this.sceneToLoad = sceneName;

            tasksBeforeConfirmLoad = new();

            BeforeConfirmLoad?.Invoke(this);
            foreach (var choice in tasksBeforeConfirmLoad)
            {
                if (!await choice) return;
            }

            //await Task.WhenAll(tasksBeforeConfirmLoad);

            await Task.Delay(125);


            tasksBeforeLoad = new();
            BeforeLoadScene?.Invoke(this);
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

            BeforeActivateScene?.Invoke(this);

            await Task.WhenAll(tasksBeforeActivateScene);

            scene.allowSceneActivation = true;

            OnLoadScene?.Invoke(this);
            await Task.Delay(2500);
            isLoading = false;
        }

        public string CurrentScene()
        {
            return SceneManager.GetActiveScene().name;
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
