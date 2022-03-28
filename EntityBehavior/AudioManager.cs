using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using System;
public class AudioManager : MonoBehaviour
{
    // Start is called before the first frame update
    

    public List<AudioSource> audioSources;
    public AudioSource presetAudio;
    public AudioMixerGroup mixerGroup;
    public Action<AudioManager> OnEmptyAudio;


    public bool audioRoutineIsActive;

    void Start()
    {
        audioSources = new List<AudioSource>();
    }

    public void OnValidate()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator CheckAudioRoutine(float length = 1f)
    {

        yield return new WaitForSeconds(length);

        audioRoutineIsActive = IsPlaying();

        while(audioRoutineIsActive)
        {
            yield return new WaitForSeconds(1f);
            audioRoutineIsActive = IsPlaying();
        }

        ClearAudios();
        OnEmptyAudio?.Invoke(this);

    }

    public bool IsPlaying()
    {
        foreach(var source in audioSources)
        {
            if(source.isPlaying)
            {
                return true;
            }
        }

        return false;
    }

    public void ClearAudios()
    {
        for(int i = 0; i< audioSources.Count; i++)
        {
            var source = audioSources[i];

            if(!source.isPlaying)
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
        if(audioSources.Count > 0)
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
        
        var newAudioSource = gameObject.AddComponent<AudioSource>();
        if (mixerGroup)
        {
            newAudioSource.outputAudioMixerGroup = mixerGroup;
        }
        newAudioSource.spatialBlend = .5f;
        audioSources.Add(newAudioSource);
        newAudioSource.PlayOneShot(clip);
        
        if(!audioRoutineIsActive)
        {
            audioRoutineIsActive = true;
            var clipLength = clip.length;
           StartCoroutine(CheckAudioRoutine(clipLength + .25f));
        }

        return newAudioSource;
    }

    public AudioSource PlayRandomSound(List<AudioClip> clips)
    {
        if(clips.Count == 0) { return null; }

        var random = UnityEngine.Random.Range(0, clips.Count);

        return PlaySound(clips[random]);
    }

    public AudioSource PlaySoundLoop(AudioClip clip)
    {
        var audioSource = PlaySound(clip);
        audioSource.Stop();
        audioSource.clip = clip;
        audioSource.loop = true;
        audioSource.Play();

        return audioSource;
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

    



}
