using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class DungeonMusicManager : MusicManager
    {
        
        void Start()
        {
            GetDependencies();
            OnLoadSceneLate(null);


        }

        // Update is called once per frame
        void Update()
        {
            HandleEvents();
        }

        new void GetDependencies()
        {
            base.GetDependencies();

            var sceneManager = ArchSceneManager.active;

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
                sceneManager.AddListener(SceneEvent.OnLoadSceneLate, OnLoadSceneLate, this);
            }
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            if (sceneManager.sceneToLoad.scene == ArchScene.PostDungeon)
            {
                Unlock();
            }
        }
        
        void OnLoadSceneLate(ArchSceneManager sceneManager)
        {
            var entityGenerator = MapEntityGenerator.active;

            if (entityGenerator == null) return;
            entityGenerator.OnEntitiesGenerated += (MapEntityGenerator generator) => {
                PlayDungeonMusic();
            };
        }

        async void PlayDungeonMusic()
        {
            var success = await SetCurrentManager(this, true);
            if (!success) return;
            _= PlayPlaylist(CurrentDungeonPlaylist());   
        }

        public List<AudioClip> CurrentDungeonPlaylist()
        {
            var playlist = new List<AudioClip>();
            var dungeonIndex = Core.dungeonIndex;
            var dungeonSets = Core.currentDungeonSets;

            if (dungeonSets == null) return playlist;

            if (dungeonIndex < 0 || dungeonIndex >= dungeonSets.Count) return playlist;

            var dungeonPlaylist = dungeonSets[dungeonIndex].dungeonPlaylist;
            if (dungeonPlaylist == null) return playlist;

            playlist = dungeonPlaylist.DungeonSongs();

            return playlist;
        }
    }
}
