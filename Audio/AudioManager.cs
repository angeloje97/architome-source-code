using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Threading.Tasks;
using CafofoStudio;


namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {

        public List<AudioSource> audioSources;
        public AudioSource presetAudio;
        public AudioMixerGroup mixerGroup;
        public Action<AudioManager> OnEmptyAudio;
        public List<Action> Actions;

        public bool audioRoutineIsActive;

        [Header("Audio Source Settings")]
        public float spatialBlend = .5f;
        
        public float pitchRandomRange;


        void Start()
        {
            audioSources = new List<AudioSource>();
            Actions = new();
        }

        

        public void OnValidate()
        {
            //spatialBlend = .5f;
            if (presetAudio == null)
            {
                presetAudio = GetComponent<AudioSource>();
            }
        }

        // Update is called once per frame
        void Update()
        {
        }

        void CopyPresetFor(AudioSource source)
        {
            //if (presetAudio == null) return;
            //foreach (var field in typeof(AudioSource).GetFields())
            //{
            //    field.SetValue(source, field.GetValue(presetAudio));
            //}

            //foreach (var property in typeof(AudioSource).GetProperties())
            //{
            //    property.SetValue(source, property.GetValue(presetAudio));
            //}
        }

        public async void CheckAudioRoutine(float length = 1f)
        {
            if (audioRoutineIsActive) return;
            audioRoutineIsActive = true;

            await Task.Delay((int)(length * 1000));

            audioRoutineIsActive = await IsPlaying();

            int time = 0;

            while (audioRoutineIsActive)
            {
                await Task.Delay(1000);

                time++;

                audioRoutineIsActive = await IsPlaying();
            }

            await ClearAudios();
            OnEmptyAudio?.Invoke(this);

        }

        public void AddAction(Action action)
        {
            if (Actions == null)
            {
                Actions = new();
            }

            Actions.Add(action);
        }

        public void InitiateActions()
        {
            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i]();

                Actions.RemoveAt(i);
                i--;
            }
        }

        public AudioSource AudioSourceFromClip(AudioClip clip)
        {
            return audioSources.Find(source => source.clip == clip);
        }
        public async Task<bool> IsPlaying()
        {
            foreach (var source in audioSources)
            {
                await Task.Yield();
                if (source.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }
        public async Task ClearAudios()
        {
            for (int i = 0; i < audioSources.Count; i++)
            {
                await Task.Yield();
                var source = audioSources[i];

                if (source == null)
                {
                    audioSources.RemoveAt(i);
                    i--;
                    continue;
                }

                if (!source.isPlaying)
                {
                    audioSources.RemoveAt(i);
                    Destroy(source);
                    i--;
                }
            }
        }
        public void PlayAudioClip(AudioClip audioClip)
        {
            PlaySound(audioClip);
        }
        public AudioSource PlaySound(AudioClip clip, float volume = 1f)
        {
            if (audioSources == null) audioSources = new List<AudioSource>();
            if (audioSources.Count > 0)
            {
                foreach (AudioSource source in audioSources)
                {
                    if (!source.isPlaying)
                    {
                        source.clip = clip;
                        source.loop = false;
                        source.volume = volume;
                        source.Play();
                        //source.PlayOneShot(clip);
                        return source;
                    }
                }
            }

            //var newAudioSource = gameObject.AddComponent<AudioSource>();
            var newAudioSource = ArchGeneric.CopyComponent(presetAudio, gameObject);
            if (mixerGroup)
            {
                newAudioSource.outputAudioMixerGroup = mixerGroup;
            }

            newAudioSource.volume = volume;
            newAudioSource.spatialBlend = spatialBlend;
            var randomPitchOffset = UnityEngine.Random.Range(-pitchRandomRange, pitchRandomRange);

            newAudioSource.pitch += randomPitchOffset;
            
            audioSources.Add(newAudioSource);

            newAudioSource.clip = clip;
            newAudioSource.Play();
            //newAudioSource.PlayOneShot(clip);

            CheckAudioRoutine(clip.length + .25f);

            //if (!audioRoutineIsActive)
            //{
            //    audioRoutineIsActive = true;
            //    var clipLength = clip.length;
            //    StartCoroutine(CheckAudioRoutine(clipLength + .25f));
            //}

            return newAudioSource;
        }
        public AudioSource PlayRandomSound(List<AudioClip> clips)
        {
            if (clips.Count == 0) { return null; }
            return PlaySound(ArchGeneric.RandomItem(clips));
        }
        public AudioSource PlaySoundLoop(AudioClip clip, float maxLength = 0f)
        {
            var audioSource = PlaySound(clip);
            audioSource.Stop();
            audioSource.loop = true;
            audioSource.Play();

            if (maxLength != 0)
            {
                ArchAction.Delay(() => { audioSource.Stop(); }, maxLength);

            }

            return audioSource;
        }
        public AudioSource PlayRandomLoop(List<AudioClip> clips, float maxLength = 0f)
        {
            if (clips.Count == 0) return null;

            return PlaySoundLoop(ArchGeneric.RandomItem(clips), maxLength);
        }
        public void StopLoops()
        {
            //foreach(var source in audioSources)
            //{
            //    if(source.loop && source.isPlaying)
            //    {
            //        source.Stop();
            //    }
            //}
            foreach (var source in audioSources)
            {

            }
        }
        public void StopAll()
        {
            foreach (var source in audioSources)
            {
                source.Stop();
            }
        }

    }

}