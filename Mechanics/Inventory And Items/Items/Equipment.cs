using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using JetBrains.Annotations;

namespace Architome
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Architome/Item/Equipment/New Equipment")]
    public class Equipment : Item
    {
        [Header("Equipment Properties")]
        public EquipmentSlotType equipmentSlotType;
        public bool usesSecondarySlot;
        public EquipmentSlotType secondarySlotType;
        public ArmorType armorType;
        public List<Vector3> equipmentOverRide;

        public int LevelRequired;
        public int itemLevel = 1;

        public Stats stats;

        public List<BuffInfo> equipmentEffects = new();


        [Header("Stat Weights")]
        public float vitalityWeight;
        public float strengthWeight;
        public float dexterityWeight;
        public float wisdomWeight;
        public float armorWeight;
        public float magicResistWeight;


        [SerializeField] bool updateBuffs;
        [SerializeField] bool updateStateWeights;
        [SerializeField] bool clearStats;

        [SerializeField] bool updateItemLevel;

        public void OnValidate()
        {
            HandleStatWeights();
            HandleClearStats();
            HandleUpdateBuffs();
            HandleItemLevel();

            itemType = ItemType.Equipment;

            void HandleStatWeights()
            {
                if (!updateStateWeights) return;


                updateStateWeights = false;
                ProcessStatWeights();
            }

            void HandleClearStats()
            {
                if (!clearStats) return;
                clearStats = false;

                stats.ZeroOut();
            }

            void HandleUpdateBuffs()
            {
                if (!updateBuffs) return;
                updateBuffs = false;

                equipmentEffects ??= new();
            }

            void HandleItemLevel()
            {
                if (!updateItemLevel) return;
                updateItemLevel = false;
                SetPower(LevelRequired, itemLevel, rarity);
            }
        }

        public override bool Equals(Item other)
        {
            if (!base.Equals(other)) return false;
            if (!IsEquipment(other)) return false;
            if (other.rarity != rarity) return false;

            var equipment = (Equipment)other;

            if (!stats.Equals(equipment.stats)) return false;
            Debugger.UI(4912, $"Equipment equals other made it here");
            if (itemLevel != equipment.itemLevel) return false;

            return true;
        }

        public override void AdjustValue()
        {
            base.AdjustValue();

            int minLevel = LevelRequired > 0 ? LevelRequired : 1;
            int minIlevel = itemLevel > 0 ? itemLevel : 1;
            value = minIlevel * minLevel * (int) rarity;
        }

        public override bool Useable(UseData data)
        {
            return true;
        }

        public override string UseString()
        {
            return "Equip";
        }

        public override void Use(UseData data)
        {
            var entity = data.entityUsed;
            if (!entity) return;
            entity.infoEvents.OnTryEquip?.Invoke(data.itemInfo, entity);
        }

        public List<int> EquipmentEffectsData()
        {
            var data = new List<int>();
            equipmentEffects ??= new();
            foreach (var effect in equipmentEffects)
            {
                var info = effect.GetComponent<BuffInfo>();
                data.Add(info._id);
            }

            return data;
        }
        public override string Description()
        {
            return base.Description();
        }

        public override string SubHeadline()
        {

            var result = $"{armorType}, {equipmentSlotType}\n";
                
            
            return result;
        }

        public override string Attributes()
        {
            var result = base.Attributes();
            var attributes = stats.Attributes();

            var attributeStrings = new List<string>();
            var levelStrings = new List<string>();
            if (LevelRequired > 0)
            {
                levelStrings.Add($"Level Required: {LevelRequired}");
            }

            if (itemLevel > 0)
            {
                levelStrings.Add($"Item Level {itemLevel}");
            }

            result += ArchString.NextLineList(levelStrings);

            foreach (var attribute in attributes)
            {
                string value = attribute.Value;
                if (Stats.PercentageFields.Contains(attribute.Name))
                {
                    var floatValue = (float)attribute.Data;

                    value = $"{ArchString.FloatToSimple(floatValue * 100)}%";
                }

                attributeStrings.Add($"{value} {ArchString.CamelToTitle(attribute.Name)}");
            }

            if (attributes.Count > 0)
            {
                result += "\n\n";
            }

            return result + ArchString.NextLineList(attributeStrings);
        }
        // Start is called before the first frame update
        public void UpdateEquipmentStats()
        {

        }

        public void ProcessStatWeights()
        {
            var total = vitalityWeight + strengthWeight + dexterityWeight + wisdomWeight + armorWeight + magicResistWeight;
            if (total == 0) { return; }
            var vContribution = vitalityWeight / total;
            var sContribution = strengthWeight / total;
            var dContribution = dexterityWeight / total;
            var wContribution = wisdomWeight / total;
            var mrCondtribution = magicResistWeight / total;
            var armContribution = armorWeight / total;
            //Total Stats Per Level
            var totalStats = itemLevel;

            stats.Vitality = (int)(vContribution * totalStats);
            stats.Strength = (int)(sContribution * totalStats);
            stats.Dexterity = (int)(dContribution * totalStats);
            stats.Wisdom = (int)(wContribution * totalStats);
            stats.magicResist = (int)(mrCondtribution * totalStats);
            stats.armor = (int)(armContribution * totalStats);
        }
        
        public void SetPower(int level, int itemLevel, Rarity rarity)
        {
            this.rarity = rarity;
            LevelRequired = level;
            this.itemLevel = itemLevel;

            ProcessStatWeights();
        }

        // Update is called once per frame
    }
}