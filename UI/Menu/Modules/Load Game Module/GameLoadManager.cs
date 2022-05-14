using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GameLoadManager : MonoBehaviour
    {
        public List<SaveGame> savedGames;

        public void GetAllSaves()
        {
            var objects = SerializationManager.LoadSaves();
            savedGames = new();

            foreach (var obj in objects)
            {
                if (obj.GetType() != typeof(SaveGame)) continue;
                var save = (SaveGame)obj;

                savedGames.Add(save);
            }
        }

        private void Start()
        {
            GetAllSaves();
        }

    }
}
