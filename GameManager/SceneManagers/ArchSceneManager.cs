using System.Collections;
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

        public List<Task> tasksBeforeLoad;
        public List<Task> tasksBeforeConfirmLoad;
        public List<Task> tasksBeforeActivateScene;

        public Action<ArchSceneManager> BeforeLoadScene;
        public Action<ArchSceneManager> BeforeActivateScene;
        public Action<ArchSceneManager> OnLoadScene;
        public Action<ArchSceneManager> BeforeConfirmLoad;

        public Action<AsyncOperation> OnLoadStart;
        public Action<AsyncOperation> WhileLoading;
        public Action<AsyncOperation> OnLoadEnd;


        public bool confirmLoad;
        public string sceneToLoad;
        public float progressValue;

        private void Awake()
        {
            active = this;
        }

        async public void LoadScene(string sceneName, bool async = true)
        {
            confirmLoad = true;
            this.sceneToLoad = sceneName;

            tasksBeforeConfirmLoad = new();

            BeforeConfirmLoad?.Invoke(this);

            await Task.WhenAll(tasksBeforeConfirmLoad);

            if (!confirmLoad) return;

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

    }
}
