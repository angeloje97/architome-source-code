using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class RoomPool : ScriptableObject
    {
        int id;

        public int _id
        {
            get
            {
                return idSet ? 999999 : id;
            }

            private set
            {
                id = value;
            }
        }

        bool idSet;
        [Serializable]
        public class PatrolGroup
        {
            public List<GameObject> entityMembers;
        }

        public List<GameObject> tier1Entities;
        public List<GameObject> tier2Entities;
        public List<GameObject> tier3Entities;
        public List<GameObject> neutralEntities;
        public List<GameObject> bossEntities;
        public List<PatrolGroup> patrolGroups;
    }
}
