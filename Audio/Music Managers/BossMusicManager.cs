using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BossMusicManager : MusicManager
    {
        public MusicPlaylist musicPlaylist;
        EntityInfo entity;

        public bool combatStart;

        void Start()
        {
            GetDependencies();
        }

        void Update()
        {
        
        }

        new void GetDependencies()
        {
            base.GetDependencies();
            entity = GetComponentInParent<EntityInfo>();

            if (entity)
            {
                entity.OnCombatChange += HandleCombatChange;
            }
        }

        void HandleCombatChange(bool isInCombat)
        {
            if (musicPlaylist == null) return;

            HandleEnterCombat();
            HandleExitCombat();

            async void HandleEnterCombat()
            {
                if (!isInCombat) return;
                var songs = musicPlaylist.bossPlaylist.songList;

                for(int i = 0; i <  songs.Count; i = (i + 1) % songs.Count)
                {
                    if (!entity.isInCombat) return;
                    var song = songs[i];

                    await musicPlayer.PlayTemp(song, (object obj) => entity.isInCombat);
                }
            }

            void HandleExitCombat()
            {
                if (isInCombat) return;
                if (!combatStart) return;
                combatStart = false;

                var audioManager = musicPlayer.audioManager;

                var endSong = musicPlaylist.bossPlaylist.combatEnd;

                if (endSong)
                {
                    audioManager.PlayAudioClip(endSong);
                }
            }

            combatStart = true;
            
        }
    }
}
