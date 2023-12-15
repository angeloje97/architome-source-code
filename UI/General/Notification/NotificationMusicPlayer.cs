using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Architome
{
    public class NotificationMusicPlayer : MonoBehaviour
    {
        MusicPlayer currentPlayer;


        [SerializeField] Animator animator;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI title;
        [SerializeField] SizeFitter sizeFitter;

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

        void Update()
        {
        
        }
        async void HandleNewSong(AudioSource source, MonoBehaviour behavior)
        {
            await ArchAction.WaitUntil(() => playing, false);
            await Task.Delay(125);
            await PlayMusicNotification($"Now playing {source} ({source.clip.length}");
               
        }
        async Task PlayMusicNotification(string title)
        {
            playing = true;
            this.title.text = title;
            await sizeFitter.AdjustToSize(3, 125);

            canvasGroup.SetCanvas(true);

            animator.SetTrigger("PlayNotification");
            await ArchAction.WaitUntil(() => playing, false);

            canvasGroup.SetCanvas(false);

        }

        public void StopPlaying()
        {
            playing = false;
        }
    }
}
