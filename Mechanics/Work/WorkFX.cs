using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class WorkFX : MonoBehaviour
    {
        public WorkInfo station;
        public AudioManager audioManager;
        public ParticleManager particleManager;
        public NotificationManager notifications;

        // Start is called before the first frame update

        public void GetDependencies()
        {

            audioManager = GetComponentInChildren<AudioManager>();
            station = GetComponent<WorkInfo>();
            notifications = NotificationManager.active;
            
            
            station.taskEvents.OnStartTask += OnStartTask;
            station.taskEvents.OnTaskComplete += OnTaskComplete;
            HandleCantWork();
        }

        public void Start()
        {
            GetDependencies();

        }

        public async void OnStartTask(TaskEventData eventData)
        {
            var task = eventData.task;

            var workingClip = task.effects.workingSound;

            if(workingClip == null) { return; }

            var audioSource = audioManager.PlaySoundLoop(workingClip);


            while (eventData.task.states.isBeingWorkedOn)
            {
                await Task.Yield();
            }

            audioSource.Stop();
        }

        public void OnTaskComplete(TaskEventData eventData)
        {

            var task = eventData.task;
            var completionSound = task.effects.completionSound;

            if (completionSound == null) return;

            audioManager.PlaySound(completionSound);
        }

        void HandleCantWork()
        {
            bool delayActive = false;
            string recentMessage = "";
            station.taskEvents.OnCantWorkOnTask += async delegate (TaskEventData eventData, string message)
            {
                if (message.Trim() == recentMessage.Trim())
                {
                    Debugger.Environment(5491, $"Same message: {message}");
                    return;
                }

                recentMessage = message;

                while (delayActive)
                {
                    await Task.Yield();
                }


                delayActive = true;


                await notifications.CreateNotification(new(NotificationType.Warning) {
                    name = eventData.workInfo.workName,
                    description = message,
                    dismissable = true,
                });

                ArchAction.Delay(() => {
                    
                    delayActive = false;
                    recentMessage = "";
                }, 1f);
            };
        }



    }

}
