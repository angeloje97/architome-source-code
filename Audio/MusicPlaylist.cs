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
        public class ScenePlaylist
        {
            public ArchScene scene;
            public List<AudioClip> songs;
            public bool shuffle;
        }

        public List<ScenePlaylist> scenePlaylists;

        public List<AudioClip> CurrentScenePlaylist()
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
                    if (scenePlaylist.shuffle)
                    {
                        return ArchGeneric.Shuffle(scenePlaylist.songs);
                    }
                    return scenePlaylist.songs;
                }
            }
           

            return playlist;
        }

    }
}
