using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Architome
{
    public class WorkSFX : MonoBehaviour
    {
        public WorkInfo station;
        public AudioManager audioManager;

        public AudioSource workingAudioSource;
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
            station.taskEvents.OnEndTask += OnEndTask;
            station.taskEvents.OnTaskComplete += OnTaskComplete;
        }

        public void Start()
        {
            GetDependencies();

        }

        public void OnStartTask(TaskEventData eventData)
        {
            var task = eventData.task;

            var workingClip = task.properties.workingSound;

            if(workingClip == null) { return; }

            workingAudioSource = audioManager.PlaySoundLoop(workingClip);

        }

        public void OnEndTask(TaskEventData eventData)
        {
            var station = eventData.task.properties.station;

            if(workingAudioSource == null) { return; }

            workingAudioSource.Stop();
            workingAudioSource = null;

            ArchAction.IntervalFor(() =>
            {
                foreach (var stationTask in station.tasks)
                {
                    if (stationTask.states.isBeingWorkedOn)
                    {
                        return;
                    }
                }

                audioManager.StopLoops();
            }, .25f, 3);
        }

        public void OnTaskComplete(TaskEventData eventData)
        {
            var workStation = eventData.task.properties.station;

            var task = eventData.task;
            var completionSound = task.properties.completionSound;

            if (completionSound == null) return;


            audioManager.PlaySound(completionSound);
            

        }



    }

}
