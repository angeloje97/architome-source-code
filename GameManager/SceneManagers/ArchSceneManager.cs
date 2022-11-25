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
        public List<SceneInfo> sceneInfos;
        public Dictionary<ArchScene, SceneInfo> sceneInfoDict;

        public List<Task> tasksBeforeLoad;
        public List<Task<bool>> tasksBeforeConfirmLoad;
        public List<Task> tasksBeforeActivateScene;

        public Action<ArchSceneManager> BeforeLoadScene;
        public Action<ArchSceneManager> BeforeActivateScene;
        public Action<ArchSceneManager> OnLoadScene;
        public Action<ArchSceneManager> BeforeConfirmLoad;

        public Action<AsyncOperation> OnLoadStart;
        public Action<AsyncOperation> WhileLoading;
        public Action<AsyncOperation> OnLoadEnd;


        public string sceneToLoad;
        public float progressValue;

        private void Awake()
        {
            active = this;
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
