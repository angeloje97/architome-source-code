using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.Animations;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Weapon")]
    public class Weapon : Equipment
    {

        public static List<WeaponType> TwoHanders
        {
            get
            {
                return new()
                {
                    WeaponType.Rifle,
                    WeaponType.Staff,
                    WeaponType.TwoHanded,
                    WeaponType.Crossbow,
                    WeaponType.Bow
                };
            }
        }
        public WeaponType weaponType;
        public BodyPart sheathPart;
        public BodyPart drawPart;
        public bool usesSecondDraw;
        public float attackValue;
        public BodyPart secondDraw;
        public AnimatorOverrideController weaponController;

        
        [Header("Animation")]

        public Vector3 weaponAttackStyle;
        public int weaponMovementStyle;
        public GameObject ability;


        [Header("Saved Positions")]
        [SerializeField] Vector3 sheathPos;
        [SerializeField] Quaternion sheathRot;

        [SerializeField] Vector3 unsheathPos;
        [SerializeField] Quaternion unsheathRot;

        public Vector3 sheathPosition { get { return sheathPos; } private set { sheathPos = value; } }

        public Quaternion sheathRotation { get { return sheathRot; } private set { sheathRot = value; } }


        public Vector3 unsheathPosition { get { return unsheathPos; } private set { unsheathPos = value; } }

        public Quaternion unsheathRotation
        {
            get { return unsheathRot; }
            private set { unsheathRot = value; }

        }

        new private void OnValidate()
        {
            itemType = ItemType.Weapon;
            UpdateAbility();

        }

        void UpdateAbility()
        {
            if (ability == null) return;

            var info = ability.GetComponent<AbilityInfo>();

            if (info == null)
            {
                ability = null;

            }

            if (info.abilityType2 == AbilityType2.AutoAttack) return;

            ability = null;
        }

        public override string SubHeadline()
        {
            return $"{ArchString.CamelToTitle(weaponType.ToString())}, {ArchString.CamelToTitle(equipmentSlotType.ToString())}";
        }



        public override string Attributes()
        {

            var result = "";

            if (attackValue > 0)
            {
                result += $"Attack Value: {attackValue}\n";
            }

            result += $"{base.Attributes()}";

            return result;
        }

        public void SetSheath(Transform trans)
        {
            sheathPosition = trans.localPosition;
            sheathRotation = trans.localRotation;
        }

        public void SetUnsheath(Transform trans)
        {
            unsheathPos = trans.localPosition;
            unsheathRot = trans.localRotation;
        }
    }
}