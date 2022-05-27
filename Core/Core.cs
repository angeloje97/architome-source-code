using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public static class Core
    {
        public static SaveGame currentSave { get; private set; }
        public static GameState currentState { get; private set; }

        public struct SaveEvents
        {
            public Action<SaveGame> OnLoadSave;
            public Action<SaveGame> OnSaveGame;
            public Action<SaveGame> OnSaveNew;
            public Action<SaveGame> OnSetSave;
        }

        public static SaveEvents saveEvents { get; set; }

        public static Action<GameState> OnSetState;

        public static void Reset()
        {
            saveEvents = new();
            OnSetState = null;
        }

        public static void SetSave(SaveGame save)
        {
            currentSave = save;
            saveEvents.OnSetSave?.Invoke(save);
        }

        public static void SaveCurrent()
        {
            currentSave.Save();
            saveEvents.OnSaveGame?.Invoke(currentSave);
        }

        public static void SaveNew(SaveGame newSave)
        {
            currentSave = newSave;
            newSave.Save();
            saveEvents.OnSaveNew?.Invoke(newSave);
        }

        public static void SetState(GameState newState)
        {
            currentState = newState;
            
        }

        public static List<SaveGame> AllSaves()
        {
            var objects = SerializationManager.LoadSaves();
            var savedGames = new List<SaveGame>();

            foreach (var obj in objects)
            {
                if (obj.GetType() != typeof(SaveGame)) continue;
                var save = (SaveGame)obj;

                savedGames.Add(save);
            }

            return savedGames;
        }
        
    }
}
