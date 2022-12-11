using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class DungeonSet : ScriptableObject
    {
        [SerializeField] int id;

        public int _id
        {
            get
            {
                return idSet ? id : 9999999;
            }
        }

        [SerializeField] bool idSet;

        public void SetID(int id, bool forceSet = false)
        {
            if (idSet && !forceSet) return;

            this.id = id;

            idSet = true;

        }

        public string dungeonSetName;
        public int dungeonLevel = 1;
        public Sprite background;
        public List<RoomInfo> rooms, entrances, bosses, main, sides;

    }
}
