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

        ArchEventHandler<SaveEvent, (SaveSystem, SaveGame)> events;

        private void Awake()
        {
            active = this;
            events = new(this);
        }

        private void Start()
        {
            currentSave = Core.currentSave;
            
        }

        public void AddListener<T>(SaveEvent trigger, Action<SaveSystem, SaveGame> action, T caller) where T: Component
        {

            events.AddListener(trigger, (data) => {
                action(data.Item1, data.Item2);
            }, caller);
        }
        public void Save()
        {
            if (current == null) return;


            events.Invoke(SaveEvent.BeforeSave, (this, current));

            Core.SaveCurrent();

            events.Invoke(SaveEvent.OnSave, (this, current));
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
