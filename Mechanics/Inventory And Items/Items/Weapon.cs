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
        // Start is called before the first frame update
        [Header("Weapon Info")]
        public WeaponType weaponType;
        public BodyPart sheathPart;
        public BodyPart drawPart;
        public bool usesSecondDraw;
        public BodyPart secondDraw;
        public AnimatorOverrideController weaponController;
        
        [Header("Animation")]

        public Vector3 weaponAttackStyle;
        public int weaponMovementStyle;
        public GameObject weaponCatalyst;
        public AbilityInfo weaponAbility;


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