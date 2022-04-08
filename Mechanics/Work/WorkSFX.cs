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

            audioManager.PlaySoundLoop(workingClip);

        }

        public void OnEndTask(TaskEventData eventData)
        {
            var workingClip = eventData.task.properties.workingSound;

            if (workingClip == null) return;

            var workingSource = audioManager.AudioSourceFromClip(workingClip);

            if (workingSource == null) return;



            workingSource.Stop();
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
