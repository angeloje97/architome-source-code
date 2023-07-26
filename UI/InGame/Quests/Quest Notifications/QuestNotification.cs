using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Architome
{


    public class QuestNotification : MonoBehaviour
    {
        public Action OnQuestStart, OnQuestCompleted, OnQuestFailed;


        TaskQueueHandler taskHandler;
        ArchSceneManager sceneManager;

        [Serializable]
        public struct Info {
            public Animator animator;
            public TextMeshProUGUI title, status;
            public CanvasGroup canvasGroup;
        }

        public Info info;
        bool animationPlaying;
        bool closeEarly;
        void Start()
        {
            taskHandler = new(TaskType.Sequential);
            GetDependencies();
            HandleSceneTransition();
        }
        void GetDependencies()
        {
            var questManager = QuestManager.active;
            sceneManager = ArchSceneManager.active;
            info.canvasGroup.SetCanvas(false);
            if (questManager)
            {
                questManager.AddListener(QuestEvents.OnNew, HandleQuest, this);
                if (questManager.quests != null)
                {
                    foreach(var quest in questManager.quests)
                    {
                        HandleQuest(quest);
                    }
                }

            }

            void HandleQuest(Quest quest)
            {
                quest.OnActive += PlayStarted;
                quest.OnCompleted += PlayCompleted;
                quest.OnQuestFail += PlayFailed;
            }
        }
        void HandleSceneTransition()
        {
            if (sceneManager == null) return;

            sceneManager.AddListener(SceneEvent.BeforeLoadScene, () => {
                if (taskHandler.busy)
                {
                    sceneManager.tasksBeforeLoadPriority.Add(taskHandler.UntilTasksFinished());
                }
            }, this);

        }
        void PlayFailed(Quest quest)
        {

            taskHandler.AddTask(async () => {
                OnQuestFailed?.Invoke();
                await PlayNotification(quest.questName, "Quest Failed");

            });
        }
        void PlayCompleted(Quest quest)
        {
            taskHandler.AddTask(async () => {
                OnQuestCompleted?.Invoke();
                await PlayNotification(quest.questName, "Quest Completed");
            });
        }
        void PlayStarted(Quest quest)
        {
            taskHandler.AddTask(async () => {
                OnQuestStart?.Invoke();
                await PlayNotification(quest.questName, "Quest Started");
            });
        }
        public async Task PlayNotification(string title, string status)
        {
            await WaitLoading();

            animationPlaying = true;
            info.title.text = title;
            info.status.text = status;
            info.canvasGroup.SetCanvas(true);
            info.animator.SetTrigger("PlayNotification");
            while (animationPlaying && !closeEarly) await Task.Yield();
            animationPlaying = false;
            var timer = 2f;

            float fadeTimer = .25f;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                if (closeEarly)
                {
                    fadeTimer = .0625f;
                    break;
                }
                await Task.Yield();
            }

            closeEarly = false;

            await info.canvasGroup.SetCanvasAsync(false, fadeTimer);
        }

        public void CloseEarly()
        {
            if (!animationPlaying) return;
            closeEarly = true;
        }

        public async Task WaitLoading()
        {
            if (sceneManager == null) return;
            if (sceneManager.isLoading)
            {
                var ready = false;
                var endAction = sceneManager.AddListener(SceneEvent.OnRevealScene, () => {
                    ready = true;
                }, this);

                while (!ready) await Task.Yield();
                endAction();
            }
        }
        public void StopAnimation()
        {
            animationPlaying = false;
        }
    }
}
