using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class NotificationMusicPlayer : MonoBehaviour
    {
        MusicPlayer currentPlayer;

        [SerializeField]
        Animator animator;
        bool playing;

        void Start()
        {
            GetDependencies();
        }

        void GetDependencies() 
        {
            currentPlayer = MusicPlayer.active;
            currentPlayer.OnPlaySong += HandleNewSong;
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        async void HandleNewSong(AudioSource source, MonoBehaviour behavior)
        {
            await ArchAction.WaitUntil(() => playing, false);
            PlayMusicNotification($"Now playing {source} ({source.clip.length}");

        }


        void PlayMusicNotification(string title)
        {
            playing = true;
        }

        public void StopPlaying()
        {
            playing = false;
        }


    }
}
