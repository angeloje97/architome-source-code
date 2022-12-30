using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class MusicPlayerSceneManager : MonoBehaviour
    {
        public static MusicPlayerSceneManager active;
        public MusicPlaylist musicPlaylist;
        public List<AudioClip> currentPlaylist;

        MusicPlayer musicPlayer;
        ArchSceneManager sceneManager;


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
            PlaySceneMusic();
        }
        void GetDependencies()
        {
            musicPlayer = GetComponent<MusicPlayer>();
            sceneManager = ArchSceneManager.active;

            sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            sceneManager.AddListener(SceneEvent.OnLoadSceneLate, OnLoadScene, this);

            musicPlayer.OnPlaySong += HandlePlayerPlaySong;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        async void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            await musicPlayer.FadeOut(.1f);
        }

        void OnLoadScene(ArchSceneManager sceneManager)
        {
            PlaySceneMusic();
        }

        void HandlePlayerPlaySong(AudioSource audioSource, MonoBehaviour caller)
        {
            if (caller != this) return;
        }

        async void PlaySceneMusic()
        {
            currentPlaylist = musicPlaylist.CurrentScenePlaylist();

            foreach(var song in currentPlaylist)
            {
                var success = await musicPlayer.PlaySong(song, this, .25f);
                if (!success) return;
            }
        }



    }
}
