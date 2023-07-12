using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
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
        void Start()
        {
            taskHandler = new();
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
                questManager.OnNewQuest += HandleQuest;
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

            sceneManager.AddListener(SceneEvent.BeforeConfirmLoad, () => {
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

            info.title.text = title;
            info.status.text = status;
            animationPlaying = true;
            info.canvasGroup.SetCanvas(true);
            info.animator.SetTrigger("PlayNotification");
            while (animationPlaying) await Task.Yield();
            await Task.Delay(2000);
            await info.canvasGroup.SetCanvasAsync(false, .0625f);
        }

        public async Task WaitLoading()
        {
            if (sceneManager == null) return;

            while (sceneManager.isLoading) await Task.Yield();

            await Task.Delay(2000);
        }
        public void StopAnimation()
        {
            animationPlaying = false;
        }
    }
}
