using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class NotificationMusicPlayer : MonoBehaviour
    {
        MusicPlayer currentPlayer;
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

        void HandleNewSong(AudioSource source, MonoBehaviour behavior)
        {

        }
    }
}
