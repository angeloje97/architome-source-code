using Architome.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Music Playlist", menuName = "Architome/Audio/Music Playlist")]
    public class MusicPlaylist : ScriptableObject
    {
        [Serializable]
        public class Playlist
        {
            public List<AudioClip> songs;
            public bool shuffle;

            public List<AudioClip> songList
            {
                get
                {
                    return shuffle ? ArchGeneric.Shuffle(songs) : songs;
                }
            }
        }
        [Serializable]
        public class ScenePlaylist: Playlist
        {
            public ArchScene scene;
        }

        [Serializable]
        public class BossPlaylist: Playlist
        {
            public AudioClip combatEnd;
            
        }


        public List<ScenePlaylist> scenePlaylists;
        public Playlist dungeonPlaylist;
        public BossPlaylist bossPlaylist;



        public List<AudioClip> CurrentScenePlaylist
        {
            get
            {

                var playlist = new List<AudioClip>();

                var sceneManager = ArchSceneManager.active;
                if (sceneManager == null) return playlist;
                var currentScene = sceneManager.sceneToLoad;

                if (currentScene == null) return playlist;



                foreach(var scenePlaylist in scenePlaylists)
                {
                    if(scenePlaylist.scene == currentScene.scene)
                    {
                        return  scenePlaylist.songList;
                    }
                }
           

                return playlist;
            }
        }

        public List<AudioClip> DungeonSongs
        {
            get
            {

                if (dungeonPlaylist != null && dungeonPlaylist.songs != null)
                {
                    return dungeonPlaylist.songList;
                }

                return new();
            }
        }

        public List<AudioClip> BossSongs
        {
            get
            {
                if(bossPlaylist != null && bossPlaylist.songs != null)
                {
                    return bossPlaylist.songList;
                }
                return new();
            }
        }

    }
}
