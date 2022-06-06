using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using System.Linq;

namespace Architome
{
    public class DungeonTable : MonoBehaviour
    {
        [Serializable]
        public class DungeonInfo
        {
            public DungeonSet set;
            public int amount;
            public Transform dungeonParent;
            public List<Size> allowedSize;
            public List<Dungeon> dungeons;


            [Serializable]
            public struct PresetDungeons
            {
                public List<Dungeon.Rooms> levels;
            }

            public List<PresetDungeons> presets;
        }

        [Serializable]
        public struct Prefabs
        {
            public GameObject dungeon;
        }

        [Serializable]
        public struct Info
        {
            public Transform dungeonParent;
        }

        public Prefabs prefabs;
        
        public List<DungeonInfo> dungeonInfos;
        public Dungeon selectedDungeon;
        public DungeoneerManager manager;

        void GetDependencies()
        {
            manager = GetComponentInParent<DungeoneerManager>();

            if (manager)
            {
                manager.BeforeCheckCondition += BeforeCheckCondition;
            }

            var saveSystem = SaveSystem.active;
            var sceneManager = ArchSceneManager.active;

            if (saveSystem)
            {
                saveSystem.BeforeSave += BeforeSave;
            }

            if (sceneManager)
            {
                sceneManager.BeforeLoadScene += BeforeLoadScene;
            }
        }

        private void Start()
        {
            GetDependencies();
            CreateDungeonSets();
            UpdateDungeons();
        }

        void BeforeSave(SaveGame save)
        {
            SaveDungeons();
        }

        void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            SaveDungeons();
        }

        void SaveDungeons()
        {
            var currentSave = Core.currentSave;
            if (currentSave == null) return;

            foreach (var info in dungeonInfos)
            {
                foreach (var dungeon in info.dungeons)
                {
                    if (dungeon.preset) continue;
                    currentSave.SaveDungeon(dungeon);
                }
            }

            if (selectedDungeon)
            {
                Core.currentDungeon = selectedDungeon.levels;
            }
        }

        public void OnSelectDungeon(Dungeon selectedDungeon)
        {
            this.selectedDungeon = selectedDungeon;
            manager.SetDungeon(selectedDungeon);
            UpdateDungeons();
        }

        

        public void UpdateDungeons()
        {
            foreach (var dungeonInfo in dungeonInfos)
            {
                foreach (var dungeon in dungeonInfo.dungeons)
                {
                    var selected = dungeon == selectedDungeon;

                    dungeon.SetHighlight(selected);
                }
            }
        }

        public void BeforeCheckCondition(List<bool> conditions)
        {
            conditions.Add(selectedDungeon != null);
        }


        public void CreateDungeonSets()
        {
            if (prefabs.dungeon == null) return;

            var mappedInfos = new Dictionary<int, DungeonInfo>();

            foreach (var dungeonInfo in dungeonInfos)
            {

                if (dungeonInfo.dungeonParent == null) continue;
                if (dungeonInfo.dungeons == null)
                {
                    dungeonInfo.dungeons = new();
                }

                mappedInfos.Add(dungeonInfo.set._id, dungeonInfo);

                FillPresetDungeons(dungeonInfo);
                ArchAction.Yield(() => FillRandomDungeons(dungeonInfo));


            }

            LoadDungeons(mappedInfos);

            void FillPresetDungeons(DungeonInfo info)
            {
                foreach (var preset in info.presets)
                {
                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();

                    newDungeon.SetDungeon(info, preset);
                    newDungeon.OnSelectDungeon += OnSelectDungeon;
                    info.dungeons.Add(newDungeon);

                }
            }

            void LoadDungeons(Dictionary<int, DungeonInfo> mappedInfos) // Poor run time
            {
                var currentSave = Core.currentSave;

                if (currentSave == null) return;

                var savedDungeons = currentSave.savedDungeons;
                if (savedDungeons == null || savedDungeons.Count == 0) return;

                foreach (var savedDungeon in savedDungeons)
                {
                    if (!mappedInfos.ContainsKey(savedDungeon.dungeonSetID)) continue;
                    var info = mappedInfos[savedDungeon.dungeonSetID];
                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();

                    newDungeon.SetDungeon(info, savedDungeon);
                    info.dungeons.Add(newDungeon);
                }
            }

            void FillRandomDungeons(DungeonInfo info)
            {
                while (info.dungeons.Count < info.amount)
                {
                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();
                    newDungeon.SetDungeon(info);
                    newDungeon.OnSelectDungeon += OnSelectDungeon;
                    info.dungeons.Add(newDungeon);
                }
            }
        }

        


    }
}
