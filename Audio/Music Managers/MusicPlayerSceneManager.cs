using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MusicPlayerSceneManager : MusicManager
    {
        public static MusicPlayerSceneManager active;
        public MusicPlaylist musicPlaylist;
        public List<AudioClip> currentPlaylist;

        
        ArchSceneManager sceneManager;

        public bool ignoreFadeOut;


        private void Awake()
        {
            if (active)
            {
                enabled = false;
                return;
            }
            active = this;

        }
        void Start()
        {
            if (active != this) return;
            GetDependencies();
            OnLoadScene(ArchSceneManager.active);
        }

        void Update()
        {
            HandleEvents();
        }

        new void GetDependencies()
        {
            base.GetDependencies();
            sceneManager = ArchSceneManager.active;

            sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            sceneManager.AddListener(SceneEvent.OnLoadScene, OnLoadScene, this);

            musicPlayer.OnPlaySong += HandlePlayerPlaySong;
        }

        // Update is called once per frame
        

        async void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            if (ignoreFadeOut) return;
            await musicPlayer.FadeOut(.1f);


        }

        async void OnLoadScene(ArchSceneManager sceneManager)
        {
            var setCurrent = await SetCurrentManager(this);
            if (!setCurrent) return;
            PlaySceneMusic();
        }

        void HandlePlayerPlaySong(AudioSource audioSource, MonoBehaviour caller)
        {
            if (caller != this) return;
        }

        async void PlaySceneMusic()
        {
            currentPlaylist = musicPlaylist.CurrentScenePlaylist();

            await PlayPlaylist(currentPlaylist);
        }



    }
}
