using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Equipment")]
    public class Equipment : Item
    {
        [Header("Equipment Info")]
        public EquipmentSlotType equipmentSlotType;
        public EquipmentSlotType secondarySlotType;
        public ArmorType armorType;
        public List<Vector2> equipmentOverRide;

        public int LevelRequired;
        public int itemLevel = 1;

        public Stats stats;

        [Header("Stat Weights")]
        public float vitalityWeight;
        public float strengthWeight;
        public float dexterityWeight;
        public float wisdomWeight;

        public void OnValidate()
        {
            ProcessStatWeights();
            itemType = ItemType.Equipment;
        }

        public override string Description()
        {
            return base.Description();
        }

        public override string SubHeadline()
        {
            return $"{equipmentSlotType}";
        }

        public override string Attributes()
        {
            var result = base.Attributes();
            var attributes = stats.Attributes();

            foreach (var attribute in attributes)
            {
                result += $"{ArchString.CamelToTitle(attribute.Name)} ({attribute.Value})\n";
            }

            return result;
        }




        // Start is called before the first frame update
        public void UpdateEquipmentStats()
        {

        }

        public void ProcessStatWeights()
        {
            var total = vitalityWeight + strengthWeight + dexterityWeight + wisdomWeight;
            if (total == 0) { return; }
            var vContribution = vitalityWeight / total;
            var sContribution = strengthWeight / total;
            var dContribution = dexterityWeight / total;
            var wContribution = wisdomWeight / total;

            //Total Stats Per Level
            var totalStats = itemLevel * 4;

            stats.Vitality = (int)(vContribution * totalStats);
            stats.Strength = (int)(sContribution * totalStats);
            stats.Dexterity = (int)(dContribution * totalStats);
            stats.Wisdom = (int)(wContribution * totalStats);


        }

        // Update is called once per frame
    }
}