using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
using System.Threading.Tasks;


namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioManager : MonoBehaviour
    {

        public List<AudioSource> audioSources;
        public AudioSource presetAudio;
        public AudioMixerGroup mixerGroup;
        public Action<AudioManager> OnEmptyAudio;

        public bool audioRoutineIsActive;

        [Header("Audio Source Settings")]
        public float spatialBlend = .5f;
        
        public float pitchRandomRange;


        void Start()
        {
            audioSources = new List<AudioSource>();
        }

        public void OnValidate()
        {
            spatialBlend = .5f;
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

        public IEnumerator CheckAudioRoutine(float length = 1f)
        {

            yield return new WaitForSeconds(length);

            audioRoutineIsActive = IsPlaying();

            while (audioRoutineIsActive)
            {
                yield return new WaitForSeconds(1f);
                audioRoutineIsActive = IsPlaying();
            }

            ClearAudios();
            OnEmptyAudio?.Invoke(this);

        }

        public AudioSource AudioSourceFromClip(AudioClip clip)
        {
            return audioSources.Find(source => source.clip == clip);
        }

        public bool IsPlaying()
        {
            foreach (var source in audioSources)
            {
                if (source.isPlaying)
                {
                    return true;
                }
            }

            return false;
        }

        public void ClearAudios()
        {
            for (int i = 0; i < audioSources.Count; i++)
            {
                var source = audioSources[i];

                if (!source.isPlaying)
                {
                    audioSources.RemoveAt(i);
                    Destroy(source);
                    i--;
                }
            }
        }

        public AudioSource PlaySound(AudioClip clip)
        {
            if (audioSources == null) audioSources = new List<AudioSource>();
            if (audioSources.Count > 0)
            {
                foreach (AudioSource source in audioSources)
                {
                    if (!source.isPlaying)
                    {
                        source.PlayOneShot(clip);
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

            newAudioSource.spatialBlend = .5f;
            var randomPitchOffset = UnityEngine.Random.Range(-pitchRandomRange, pitchRandomRange);

            newAudioSource.pitch += randomPitchOffset;
            
            audioSources.Add(newAudioSource);
            newAudioSource.PlayOneShot(clip);

            if (!audioRoutineIsActive)
            {
                audioRoutineIsActive = true;
                var clipLength = clip.length;
                StartCoroutine(CheckAudioRoutine(clipLength + .25f));
            }

            return newAudioSource;
        }

        public AudioSource PlayRandomSound(List<AudioClip> clips)
        {
            if (clips.Count == 0) { return null; }

            var random = UnityEngine.Random.Range(0, clips.Count);

            return PlaySound(clips[random]);
        }

        public AudioSource PlaySoundLoop(AudioClip clip, float maxLength = 0f)
        {
            var audioSource = PlaySound(clip);
            audioSource.Stop();
            audioSource.clip = clip;
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

            return PlaySoundLoop(clips[UnityEngine.Random.Range(0, clips.Count)], maxLength);
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