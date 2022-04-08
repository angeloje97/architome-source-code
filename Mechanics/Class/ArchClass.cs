using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Class", menuName = "Create New Class")]
    public class ArchClass : ScriptableObject
    {
        // Start is called before the first frame update
        public string className;
        public Color classColor;
        public int classId;

        public List<ArmorType> equipableArmor;
        public List<WeaponType> equipableWeapons;
        public List<Role> possibleRoles;

    }

}