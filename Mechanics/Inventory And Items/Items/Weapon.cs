using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
    public class Weapon : Equipment
    {
        // Start is called before the first frame update
        [Header("Weapon Info")]
        public WeaponType weaponType;
        public SheathType sheathType;
        public WeaponHolder weaponHolder;

        public Vector2 weaponAttackStyle;
        public int weaponMovementStyle;
        public GameObject weaponCatalyst;
        public AbilityInfo weaponAbility;

        public Vector3 sheathPosition;
        public Quaternion sheathRotation;

        public Vector3 unsheathPosition;
        public Quaternion unsheathRotation;

    }
}