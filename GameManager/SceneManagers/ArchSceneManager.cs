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

        public Action<ArchSceneManager> BeforeLoadScene;

        public Action<AsyncOperation> OnLoadStart;
        public Action<AsyncOperation> WhileLoading;
        public Action<AsyncOperation> OnLoadEnd;


        public string sceneTolLoad;

        private void Awake()
        {
            active = this;
        }

        async public void LoadScene(string sceneName, bool async = true)
        {
            tasksBeforeLoad = new();

            this.sceneTolLoad = sceneName;

            BeforeLoadScene?.Invoke(this);

            await Task.WhenAll(tasksBeforeLoad);


            if (!async)
            {
                SceneManager.LoadScene(sceneName);
                return;
            }

            var operation = SceneManager.LoadSceneAsync(sceneName);

            OnLoadStart?.Invoke(operation);
            
            while (!operation.isDone)
            {
                WhileLoading?.Invoke(operation);

                await Task.Yield();
            }

            OnLoadEnd?.Invoke(operation);


        }

    }
}
