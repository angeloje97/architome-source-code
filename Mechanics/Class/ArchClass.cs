using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Class", menuName = "Architome/Entity/Class")]
    public class ArchClass : ScriptableObject
    {
        [SerializeField]int id;
        public int _id
        {
            get
            {
                return id == 0 ? 999999 : id;
            }
        }
        bool idSet;

        public void SetID(int id, bool force = false)
        {
            if (idSet && !force) return;
            this.id = id;

            idSet = true;

        }

        // Start is called before the first frame update
        public string className;
        public Color classColor;
        public Sprite classIcon;
        public int classIconId = -1;

        public ClassType classType;
        public ArmorType highestArmorLevel;
        public List<WeaponType> equipableWeapons;
        public List<Role> possibleRoles;

        public List<AbilityInfo> generalAbilities;
        public List<AbilityInfo> tankAbilities;
        public List<AbilityInfo> healerAbilities;
        public List<AbilityInfo> damageAbilities;


        public bool CanEquip(Item item, out string reason)
        {
            if (!Item.Equipable(item))
            {
                reason = "Item is not equippable";
                return false;

            }

            if (Item.IsWeapon(item))
            {
                var weapon = (Weapon)item;

                if (!equipableWeapons.Contains(weapon.weaponType))
                {
                    reason = $"{className} can't weild {weapon.weaponType}.";
                    return false;
                }
            }

            if (Item.IsEquipment(item))
            {
                var equipment = (Equipment)item;


                if(highestArmorLevel < equipment.armorType)
                {
                    reason = $"{className} can't equip {equipment.armorType}";
                    return false;
                }

            }


            reason = "Can equip";
            return true;
        }

    }

    

}