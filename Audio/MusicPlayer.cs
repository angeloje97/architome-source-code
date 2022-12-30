using DungeonArchitect;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{

    public enum MusicPlayerState
    {
        Playing,
        FadingOut,
        FadingIn,
        NotPlaying,
    }
    [RequireComponent(typeof(AudioManager))]
    public class MusicPlayer : MonoBehaviour
    {
        public static MusicPlayer active;

        public MusicPlayerState currentState;

        public AudioManager audioManager;
        public AudioSource currentSource;

        public bool isPlaying;

        public Action<AudioSource, MonoBehaviour> OnPlaySong;

        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
            currentState = MusicPlayerState.NotPlaying;
            ArchGeneric.DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            ArchGeneric.DontDestroyOnLoad(gameObject);
            GetDependencies();
        }

        void GetDependencies()
        {
            audioManager = GetComponent<AudioManager>();

        }

        public async Task FadeOut(float time)
        {
            if (currentSource == null) return;
            if (currentSource.clip == null) return;
            var startVolume = currentSource.volume;
            var targetVolume = 0f;
            float current = 0f;

            currentState = MusicPlayerState.FadingOut;
            while(current < time)
            {
                await Task.Yield();
                current += Time.deltaTime;

                var lerpPercent = current / time;

                currentSource.volume = Mathf.Lerp(startVolume, targetVolume, lerpPercent);
            }

            currentSource.volume = 0f;
            currentState = MusicPlayerState.NotPlaying;
            currentSource.Stop();
            currentState = MusicPlayerState.NotPlaying;
            isPlaying = false;
        }

        public async Task<AudioSource> FadeIn(AudioClip clip, float time)
        {
            while (audioManager == null) await Task.Yield();

            currentSource = audioManager.PlayAudioClip(clip);
            currentSource.volume = 0f;
            isPlaying = true;

            float targetVolume = 1f;
            float startingVolume = 0f;

            float current = 0f;
            currentState = MusicPlayerState.FadingIn;
            while(current < time)
            {
                await Task.Yield();
                current += Time.deltaTime;

                currentSource.volume = Mathf.Lerp(startingVolume, targetVolume, current / time);
            }
            currentState = MusicPlayerState.Playing;

            currentSource.volume = targetVolume;
            return currentSource;

            
        }

        public async Task<bool> PlaySong<T>(AudioClip audioClip, T caller, float transitionTime) where T: MonoBehaviour
        {
            await FadeOut(transitionTime);


            currentSource = await FadeIn(audioClip, 0f);
            OnPlaySong?.Invoke(currentSource, caller);

            while (currentSource.isPlaying)
            {
                await Task.Delay(500);

                if (!isPlaying) return false;
            }

            return true;

        }

        
        public static async void PlaySongTransition<T>(AudioClip clip, T caller, float transitionTime = .5f) where T : MonoBehaviour
        {
            var musicPlayer = active;
            if (musicPlayer == null) return;

            await musicPlayer.FadeOut(transitionTime / 2);
            var audioSource = await musicPlayer.FadeIn(clip, transitionTime / 2);
            musicPlayer.OnPlaySong?.Invoke(audioSource, caller);


            

        }

        public static void PlayImmeidate(AudioClip clip)
        {

        }

        

        
    }
}
