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
        public MusicPlayerState tempState;
        

        public AudioManager audioManager;
        public AudioSource tempAudioSource;
        public AudioSource currentSource;

        public bool isPlaying;
        public bool playingTemp;


        public Action<AudioSource, MonoBehaviour> OnPlaySong { get; set; }
        public Action<MusicPlayerState> OnChangeState { get; set; }
        public Action<MusicPlayerState> OnTempStateChange { get; set; }

        private void Awake()
        {
            if (active)
            {
                Destroy(gameObject);
                return;
            }

            active = this;
            currentState = MusicPlayerState.NotPlaying;
            tempState = MusicPlayerState.NotPlaying;
        }

        void Start()
        {
            if(active != this) return;
            ArchGeneric.DontDestroyOnLoad(gameObject);
            GetDependencies();
            HandleEvents();
        }
        void GetDependencies()
        {
            audioManager = GetComponent<AudioManager>();

        }

        async void HandleEvents()
        {
            var currentStateCheck = currentState;
            var tempStateCheck = tempState;

            while (this)
            {
                await Task.Delay(1000);
                if(currentStateCheck != currentState)
                {
                    currentStateCheck = currentState;
                    OnChangeState?.Invoke(currentState);
                }

                if(tempStateCheck != tempState)
                {
                    tempStateCheck = tempState;
                    OnTempStateChange?.Invoke(tempState);
                }
            }
        }

        public async Task FadeOut(float time)
        {
            if (currentSource == null) return;
            if (currentSource.clip == null) return;
            var startVolume = currentSource.volume;
            var targetVolume = 0f;

            currentState = MusicPlayerState.FadingOut;

            await ArchCurve.Smooth((float interpolation) => {
                if (currentSource == null)
                {
                    currentState = MusicPlayerState.NotPlaying;
                    isPlaying = false;
                    return;
                }
                currentSource.volume = Mathf.Lerp(startVolume, targetVolume, interpolation);
            }, CurveType.Linear, time);

            currentSource.volume = 0f;
            currentState = MusicPlayerState.NotPlaying;
            currentSource.Stop();
            isPlaying = false;
        }
        public async Task<AudioSource> FadeIn(AudioClip clip, float time, float targetVolume = 1f)
        {
            while (audioManager == null) await Task.Yield();

            currentSource = audioManager.PlayAudioClip(clip);
            currentSource.volume = 0f;
            isPlaying = true;

            float startingVolume = 0f;

            currentState = MusicPlayerState.FadingIn;

            await ArchCurve.Smooth((float interpolation) => {
                currentSource.volume = Mathf.Lerp(startingVolume, targetVolume, interpolation);

            }, CurveType.Linear, time);


            currentState = MusicPlayerState.Playing;

            currentSource.volume = targetVolume;
            return currentSource;

            
        }
        public async Task PlayTemp(AudioClip clip, Predicate<object> continueCondition, float volume = 1f )
        {

            tempAudioSource = audioManager.PlaySound(clip, volume);
            _ = FadeOut(.10f);

            playingTemp = true;

            tempState = MusicPlayerState.Playing;

            while (true)
            {
                await Task.Delay(250);


                var canContinue = continueCondition(null);

                Debugger.Environment(5014, $"Temp Music Can Continue: {canContinue}");


                if (!canContinue) break;

                if (!tempAudioSource.isPlaying) break;
            }

            tempState = MusicPlayerState.FadingOut;

            if (tempAudioSource && tempAudioSource.isPlaying)
            {
                var targetVolume = 0f;
                var startingVolume = tempAudioSource.volume;
                var startTime = 0f;
                var totalTime = .25f;

                while(startTime < totalTime)
                {
                    await Task.Yield();
                    startTime += Time.deltaTime;

                    tempAudioSource.volume = Mathf.Lerp(startingVolume, targetVolume, startTime / totalTime);
                }
            }

            playingTemp = false;
            tempState = MusicPlayerState.NotPlaying;

        }
        public async Task<bool> PlaySong<T>(AudioClip audioClip, T caller, float transitionTime, float targetVolume) where T: MonoBehaviour
        {
            await FadeOut(transitionTime);

            while (playingTemp)
            {
                await Task.Delay(500);
            }

            currentSource = await FadeIn(audioClip, 0f, targetVolume);
            OnPlaySong?.Invoke(currentSource, caller);

            while (currentSource.isPlaying)
            {

                await World.Delay(.5f);
                if (playingTemp) return true;
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
        public static async void PlayImmeidate<T>(AudioClip clip, T caller) where T: MonoBehaviour
        {
            var musicPlayer = active;
            if (musicPlayer == null) return;

            _ = musicPlayer.FadeOut(.25f);
            var audioSource = await musicPlayer.FadeIn(clip, 0f);
            musicPlayer.OnPlaySong?.Invoke(audioSource, caller);
        }

        

        
    }
}
