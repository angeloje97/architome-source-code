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

        async void HandleCombatChange(bool isInCombat)
        {


            if (!isInCombat)
            {
                if (combatStart)
                {
                    combatStart = false;

                    var audioManager = musicPlayer.audioManager;

                    var endSong = musicPlaylist.bossPlaylist.combatEnd;

                    if (endSong)
                    {
                        audioManager.PlayAudioClip(endSong);
                    }
                }
                return;
            }
            if (musicPlaylist == null) return;
            combatStart = true;
            var songs = musicPlaylist.bossPlaylist.songList;
            

            foreach(var song in songs)
            {
                await musicPlayer.PlayTemp(song, (object obj) => {
                    return entity.isInCombat;
                });

                if (!entity.isInCombat)
                {
                    return;
                }
            }
        }
    }
}
