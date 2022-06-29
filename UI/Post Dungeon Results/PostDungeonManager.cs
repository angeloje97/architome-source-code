using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class PostDungeonManager : MonoBehaviour
    {
        public static PostDungeonManager active;


        public Camera previewCamera;
        public GameManager manager;
        public List<EntityInfo> entities;
        public List<PartyInfo> parties;

        IGGUIInfo inGameUI;

        [Header("Modules")]
        public List<PostLevelProgress> postLevelProgress;

        public Action<PostDungeonManager> OnLoadScene;

        


        void GetDependencies()
        {
            inGameUI = IGGUIInfo.active;

            if (previewCamera)
            {
                previewCamera.gameObject.SetActive(false);
            }



            if (inGameUI)
            {
                inGameUI.SetUI(false);
            }

            manager = GameManager.active;

            parties = new();
            entities = new();

            if (manager == null) return;

            foreach (var party in manager.playableParties)
            {
                parties.Add(party);

                foreach (var entity in party.members)
                {
                    var info = entity.GetComponent<EntityInfo>();
                    if (info == null) continue;

                    var movement = info.Movement();
                    movement.SetValues(false);
                    entities.Add(info);
                }
            }

            var destroyOnLoad = new GameObject("Destroy On Load");
            manager.transform.SetParent(destroyOnLoad.transform);
            manager.transform.SetParent(null);
            Destroy(destroyOnLoad);

        }
        private void Awake()
        {
            active = this;
            GetDependencies();
            HandleManager();
            HandleLevelProgress();
        }
        private void Start()
        {

        }
        public void AddPostLevelProgress(PostLevelProgress progress)
        {
            if (postLevelProgress == null) postLevelProgress = new();

            for (int i = 0; i < postLevelProgress.Count; i++)
            {
                if (postLevelProgress[i] == null)
                {
                    postLevelProgress.RemoveAt(i);
                    i--;
                }
            }

            if (postLevelProgress.Contains(progress)) return;

            postLevelProgress.Add(progress);
        }

        void HandleLevelProgress()
        {
            if (postLevelProgress == null) return;
            for (int i = 0; i < postLevelProgress.Count; i++)
            {
                var levelProgress = postLevelProgress[i];
                if (i >= entities.Count)
                {
                    levelProgress.SetActive(false);
                    continue;
                }

                var entity = entities[i];
                levelProgress.SetEntity(entity);
            }
        }

        void HandleManager()
        {
        }

        public void LoadScene(string sceneName)
        {
            var archSceneManager = ArchSceneManager.active;
            if (archSceneManager == null) return;
            archSceneManager.LoadScene(sceneName, true);
        }
    }
}
