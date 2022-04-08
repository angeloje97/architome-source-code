using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Preset Stats", menuName = "Preset Stats")]
    public class PresetStats : ScriptableObject
    {
        public new string name;
        public Role Role;
        public NPCType npcType;
        public bool canLevel;
        [SerializeField]
        private Stats stats;

        public Stats Stats
        {
            get { return stats; }
        }
    }
}
