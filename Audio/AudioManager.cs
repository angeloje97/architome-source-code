using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Threading.Tasks;
using CafofoStudio;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoActor
    {
        #region CommonData
        public List<AudioSource> audioSources;
        public AudioSource presetAudio;
        public AudioMixerGroup mixerGroup;
        public AudioMixerType audioMixerType;
        public List<Action> Actions;

        public bool audioRoutineIsActive;

        public bool singleAudioSource;

        [Header("Audio Source Settings")]
        public float spatialBlend = .5f;
        
        public float pitchRandomRange;


        public Action<AudioManager> OnEmptyAudio { get; set; }
        #endregion

        #region Initiation

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

        #endregion
        public void UpdateMixerGroup()
        {
            var audioMixerGroups = AudioMixerGroups.active;
            mixerGroup = audioMixerGroups.MixerGroup(audioMixerType);
        }

        void CopyPresetFor(AudioSource source)
        {
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
                if (!source) break;
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

        #region Creating AudioSources
        public AudioSource PlayAudioClip(AudioClip audioClip)
        {
            return PlaySound(audioClip);
        }
        public AudioSource PlaySound(AudioClip clip, float volume = 1f, bool createChild = false)
        {
            audioSources ??= new();

            var pitch = UnityEngine.Random.Range(1f - pitchRandomRange, 1f + pitchRandomRange);

            if (!createChild)
            {
                foreach (AudioSource source in audioSources)
                {
                    if (singleAudioSource)
                    {
                        if (source.isPlaying)
                        {
                            source.Stop();
                        }

                        source.clip = clip;
                        source.loop = false;
                        source.volume = volume;
                        source.pitch = pitch;
                        source.Play();
                        return source;
                    }

                    if (!source.isPlaying)
                    {
                        source.clip = clip;
                        source.loop = false;
                        source.volume = volume;
                        source.pitch = pitch;
                        source.Play();
                        //source.PlayOneShot(clip);
                        return source;
                    }
                }
            }

            var newAudioSource = NewAudioSource(createChild);
            if (mixerGroup)
            {
                newAudioSource.outputAudioMixerGroup = mixerGroup;
            }

            newAudioSource.volume = volume;
            newAudioSource.spatialBlend = spatialBlend;
            //var randomPitchOffset = UnityEngine.Random.Range(-pitchRandomRange, pitchRandomRange);

            newAudioSource.pitch = pitch;
            
            audioSources.Add(newAudioSource);

            newAudioSource.clip = clip;
            newAudioSource.Play();

            CheckAudioRoutine(clip.length + .25f);


            return newAudioSource;
        }

        public AudioSource NewAudioSource(bool createChild = false)
        {
            if (createChild)
            {
                var childObject = new GameObject();
                childObject.transform.SetParent(transform, false);

                var newAudioSource = ArchGeneric.CopyComponent(presetAudio, childObject);
                return newAudioSource;
            }
            else
            {
                var newAudioSource = ArchGeneric.CopyComponent(presetAudio, gameObject);
                return newAudioSource;
            }
        }
        public AudioSource PlayRandomSound(List<AudioClip> clips)
        {
            if (clips.Count == 0) { return null; }
            return PlaySound(ArchGeneric.RandomItem(clips));
        }
        public AudioSource PlaySoundLoop(AudioClip clip, float maxLength = 0f, float volume = 1)
        {
            var audioSource = PlaySound(clip, volume);
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
        #endregion

        #region Background Functions
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

        public async Task UntilAllDone()
        {
            while (true)
            {
                await Task.Delay(500);
                foreach(var audioSource in audioSources)
                {
                    if (audioSource.isPlaying) continue;
                }

                break;
            }
        }
        #endregion

    }

}