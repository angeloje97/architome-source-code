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
        public Queue<Func<Task>> animationQueue;

        public Action OnQuestStart, OnQuestCompleted, OnQuestFailed;

        [Serializable]
        public struct Info {
            public Animator animator;
            public TextMeshProUGUI title, status;
        }

        public Info info;

        bool busy;
        bool animationPlaying;
        void Start()
        {
            animationQueue = new();
            GetDependencies();
        }


        void GetDependencies()
        {
            var questManager = QuestManager.active;
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
        async void AddFunction(Func<Task> newTask)
        {
            animationQueue.Enqueue(newTask);

            if (busy) return;
            busy = true;
            while (animationQueue.Count > 0)
            {
                var task = animationQueue.Dequeue();
                await task();
            }
            busy = false;
        }

        void PlayFailed(Quest quest)
        {

            AddFunction(async () => {
                OnQuestFailed?.Invoke();
                await PlayNotification(quest.questName, "Failed");

            });
        }

        void PlayCompleted(Quest quest)
        {
            AddFunction(async () => {
                OnQuestCompleted?.Invoke();
                await PlayNotification(quest.questName, "Completed");
            });
        }

        void PlayStarted(Quest quest)
        {
            AddFunction(async () => {
                OnQuestStart?.Invoke();
                await PlayNotification(quest.questName, "Started");
            });
        }

        public async Task PlayNotification(string title, string status)
        {
            info.title.text = title;
            info.status.text = status;
            animationPlaying = true;
            info.animator.SetTrigger("PlayNotification");
            while (animationPlaying) await Task.Yield();
        }

        public void StopAnimation()
        {
            animationPlaying = false;
        }
    }
}
