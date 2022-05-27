using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class GameLoadManager : MonoBehaviour
    {
        public List<SaveGame> savedGames;
        public SaveGame selectedSave;
        public SaveGame hoverSave;


        private void Start()
        {
            savedGames = Core.AllSaves();
        }

    }
}
