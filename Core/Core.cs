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


        //Dungeon Properties
        public static int dungeonIndex { get; set; }
        public static List<Dungeon.Rooms> currentDungeon { get; private set; }

        public static List<DungeonSet> currentDungeonSets { get; private set; }


        public static Action<GameState> OnSetState;

        public static Action<SaveGame> OnSetSave;
        
        

        public static void Reset()
        {
            OnSetState = null;
        }

        public static void ResetAll()
        {
            OnSetState = null;
            currentSave = null;
            currentDungeon = null;
        }

        public static void SetSave(SaveGame save)
        {
            currentSave = save;
            currentSave.OnSetSave();
            OnSetSave?.Invoke(currentSave);
        }

        public static void SaveCurrent()
        {
            currentSave.Save();
        }

        public static void SaveNew(SaveGame newSave)
        {
            currentSave = newSave;
            newSave.Save();
        }

        public static void SetState(GameState newState)
        {
            currentState = newState;
            
        }

        public static List<SaveGame> AllSaves()
        {
            var objects = SerializationManager.LoadSaves();
            var savedGames = new List<SaveGame>();

            if(objects == null)
            {
                return savedGames;
            }

            foreach (var obj in objects)
            {
                if (obj.GetType() != typeof(SaveGame)) continue;
                var save = (SaveGame)obj;

                savedGames.Add(save);
            }

            return savedGames;
        }

        public static void SetDungeon(List<Dungeon.Rooms> dungeonRooms, List<DungeonSet> dungeonSets)
        {
            currentDungeon = dungeonRooms;
            currentDungeonSets = dungeonSets;
        }

    }
}
