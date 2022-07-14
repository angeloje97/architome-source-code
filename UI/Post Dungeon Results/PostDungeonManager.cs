using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using TMPro;

namespace Architome
{
    public class PostDungeonManager : MonoBehaviour
    {
        public static PostDungeonManager active;
        public bool update;

        public Camera previewCamera;
        public GameManager manager;
        public List<EntityInfo> entities;
        public List<PartyInfo> parties;

        IGGUIInfo inGameUI;

        [Header("Modules")]
        public List<PostLevelProgress> postLevelProgress;
        public List<PostQuestProgress> postQuestProgress;
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
        }
        private void Awake()
        {
            active = this;
            GetDependencies();
            HandleLevelProgress();
            HandleQuestProgress();
        }
        private void Start()
        {

        }
        private void OnValidate()
        {
            if (!update) return;
            update = false;

            postLevelProgress = GetComponentsInChildren<PostLevelProgress>().ToList();
            postQuestProgress = GetComponentsInChildren<PostQuestProgress>().ToList();
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
        public async Task ProgressLevels()
        {
            var tasks = new List<Task>();

            foreach (var levelProgress in postLevelProgress)
            {
                tasks.Add(levelProgress.Progress());
            }

            await Task.WhenAll(tasks);
        }
        async void HandleQuestProgress()
        {
            var questManager = QuestManager.active;
            DisableAllPostQuests();

            await ProgressLevels();

            if (questManager == null)
            {
                return;
            }
            var quests = questManager.quests;

            if (quests == null)
            {
                DisableAllPostQuests();
                return;
            }

            for (int i = 0; i < postQuestProgress.Count; i++)
            {
                if (i >= quests.Count)
                {
                    postQuestProgress[i].SetActive(false);
                    continue;
                }

                postQuestProgress[i].SetActive(true);
                postQuestProgress[i].SetQuest(quests[i]);

                if (quests[i].info.state == QuestState.Failed)
                {

                }

                await Task.Delay(25);
                await postQuestProgress[i].QuestProgress();
                await ProgressLevels();
            }

            void DisableAllPostQuests()
            {
                foreach (var questProgress in postQuestProgress)
                {
                    questProgress.SetActive(false);
                }
            }
        }
        public void LoadScene(string sceneName)
        {
            var archSceneManager = ArchSceneManager.active;
            if (archSceneManager == null) return;
            SaveEntities();
            archSceneManager.LoadScene(sceneName, true);
        }
        void SaveEntities()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;
            
            foreach (var entity in entities)
            {
                currentSave.SaveEntity(entity);
            }
        }
    }
}
