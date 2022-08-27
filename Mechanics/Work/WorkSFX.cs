using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class WorkSFX : MonoBehaviour
    {
        public WorkInfo station;
        public AudioManager audioManager;

        // Start is called before the first frame update

        public void GetDependencies()
        {
            station = GetComponentInParent<WorkInfo>();
            audioManager = GetComponentInParent<AudioManager>();

            if (!station) Destroy(gameObject);

            var sfxMixerGroup = GMHelper.Mixer().SoundEffect;
            audioManager = station.GetComponentsInChildren<AudioManager>().ToList()
                                  .Find(manager => manager.mixerGroup == sfxMixerGroup);

            if (!audioManager) Destroy(gameObject);

            station.taskEvents.OnStartTask += OnStartTask;
            station.taskEvents.OnTaskComplete += OnTaskComplete;
        }

        public void Start()
        {
            GetDependencies();

        }

        public async void OnStartTask(TaskEventData eventData)
        {
            var task = eventData.task;

            var workingClip = task.properties.workingSound;

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
            var completionSound = task.properties.completionSound;

            if (completionSound == null) return;

            audioManager.PlaySound(completionSound);
        }



    }

}
