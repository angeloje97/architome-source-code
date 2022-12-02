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
        public static DungeonTable active;
        [Serializable]
        public class DungeonInfo
        {
            public List<DungeonSet> sets;
            //public DungeonSet set;
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

            public DungeonSet set
            {
                get
                {
                    if(sets != null && sets.Count > 0)
                    {
                        return sets[0];
                    }
                    else
                    {
                        return null;
                    }
                }
            }
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

        public Action<Dungeon, Dungeon> OnSelectedDungeonChange;

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
                saveSystem.AddListener(SaveEvent.BeforeSave, BeforeSave, this);
            }

            if (sceneManager)
            {
                sceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }
        }

        private void Awake()
        {
            active = this;
        }

        private void Start()
        {
            GetDependencies();
            CreateDungeonSets();
            ArchAction.Yield(() => UpdateDungeons());
            
        }

        void BeforeSave(SaveSystem system, SaveGame save)
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

                currentSave.currentDungeon = currentSave.savedDungeons[selectedDungeon.SaveIndex];
            }
            else
            {
                currentSave.currentDungeon = null;
            }
        }

        public void OnSelectDungeon(Dungeon selectedDungeon)
        {
            if (this.selectedDungeon == selectedDungeon) return;

            OnSelectedDungeonChange?.Invoke(this.selectedDungeon, selectedDungeon);

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


            foreach (var dungeonInfo in dungeonInfos)
            {

                if (dungeonInfo.dungeonParent == null) continue;
                if (dungeonInfo.dungeons == null)
                {
                    dungeonInfo.dungeons = new();
                }


                FillPresetDungeons(dungeonInfo);
                ArchAction.Yield(() => FillRandomDungeons(dungeonInfo));


            }

            LoadDungeons();

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

            void LoadDungeons() // Poor run time
            {
                var currentSave = Core.currentSave;

                if (currentSave == null) return;

                var savedDungeons = currentSave.savedDungeons;
                if (savedDungeons == null || savedDungeons.Count == 0) return;

                foreach (var savedDungeon in savedDungeons)
                {
                    if (savedDungeon.completed) continue;
                    if (savedDungeon.dungeonInfosIndex == -1) continue;
                    if (savedDungeon.dungeonInfosIndex >= dungeonInfos.Count) continue;

                    var info = dungeonInfos[savedDungeon.dungeonInfosIndex];
                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();

                    newDungeon.OnSelectDungeon += OnSelectDungeon;

                    newDungeon.SetDungeon(info, savedDungeon);
                    info.dungeons.Add(newDungeon);
                }
            }

            void FillRandomDungeons(DungeonInfo info)
            {
                while (info.dungeons.Count < info.amount)
                {
                    var size = 0;

                    if (info.dungeons.Count < 3)
                    {
                        size = info.dungeons.Count + 1;
                    }

                    var newDungeon = Instantiate(prefabs.dungeon, info.dungeonParent).GetComponent<Dungeon>();
                    newDungeon.SetDungeon(info, size);
                    newDungeon.OnSelectDungeon += OnSelectDungeon;
                    info.dungeons.Add(newDungeon);
                }
            }
        }


    }
}
