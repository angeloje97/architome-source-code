using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class MusicManager : MonoBehaviour
    {
        protected MusicPlayer musicPlayer;

        protected static MusicManager currentManager;
        public static bool locked;

        MusicManager managerCheck;


        public Action<MusicManager, MusicManager> OnMusicMangerChange;

        [Range(0, 1)]
        public float targetVolume = 1f;

        public bool queued;
        public bool requeued;


        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void HandleEvents()
        {
            if(managerCheck != currentManager)
            {
                managerCheck = currentManager;
            }
        }

        protected void GetDependencies()
        {
            musicPlayer = MusicPlayer.active;
        }

        protected virtual void OnCurrentManagerChange(MusicManager before, MusicManager after) { }

        protected static async Task<bool> SetCurrentManager(MusicManager newManager, bool setLocked = false)
        {
            if (newManager == currentManager) return true;
            if (newManager.queued)
            {
                newManager.requeued = true;

                while (newManager.queued)
                {
                    await Task.Yield();
                }
            }

            newManager.queued = true;

            while (locked)
            {
                await Task.Yield();

                if (newManager.requeued)
                {
                    newManager.requeued = false;
                    newManager.queued = false;
                    return false;
                }
            }

            newManager.queued = false;
            locked = setLocked;
            currentManager = newManager;
            return true;
        }

        public async Task PlayPlaylist(List<AudioClip> songs)
        {
            if (currentManager != this) return;
            var length = songs.Count;
            if (length == 0) return;

            var currentIndex = 0;

            while (true)
            {
                var song = songs[currentIndex];

                var success = await musicPlayer.PlaySong(song, this, .5f, targetVolume);
                if (!success) return;

                currentIndex = (currentIndex + 1) % length;
            }
        }

        public bool CanPlay()
        {
            if (currentManager == this) return true;
            return false;
        }

        public static void Unlock()
        {
            locked = false;
        }
    }
}
