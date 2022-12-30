using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public enum SaveEvent
    {
        OnSave,
        BeforeSave
    }
    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem active { get; private set; }

        public static SaveGame current { get { return Core.currentSave; } }


        [SerializeField] SaveGame currentSave;

        Dictionary<SaveEvent, Action<SaveSystem, SaveGame>> eventDict;

        private void Awake()
        {
            active = this;
            CreateEvents();
        }

        private void Start()
        {
            currentSave = Core.currentSave;
            
        }

        void CreateEvents()
        {
            eventDict = new();
            foreach(SaveEvent trigger in Enum.GetValues(typeof(SaveEvent)))
            {
                eventDict.Add(trigger, delegate (SaveSystem system, SaveGame save) { });
            }
        }

        public void AddListener<T>(SaveEvent trigger, Action<SaveSystem, SaveGame> action, T caller) where T: Component
        {
            eventDict[trigger] += HandleAction;
            void HandleAction(SaveSystem system, SaveGame save)
            {
                if(caller == null)
                {
                    eventDict[trigger] -= HandleAction;
                    return;
                }

                action(system, save);
            }
        }
        public void Save()
        {
            if (current == null) return;


            eventDict[SaveEvent.BeforeSave]?.Invoke(this, current);

            Core.SaveCurrent();

            eventDict[SaveEvent.OnSave]?.Invoke(this, current);

        }
        public void CompleteCurrentDungeon()
        {
            if (current == null) return;
            if (!DungeonData.Exists(current.currentDungeon)) return;
            current.currentDungeon.completed = true;
            current.currentDungeon = null;
        }

        public static void Operate(Action<SaveGame> action)
        {
            if (current == null) return;
            action(current);
        }
    }
}
