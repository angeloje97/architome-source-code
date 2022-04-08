using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;



namespace Architome
{
    

    [Serializable]
    public class Stats
    {
        [Serializable]
        public class Attribute
        {
            [SerializeField]
            string name;
            [SerializeField]
            string value;
            [SerializeField]
            string type;

            public string Name
            {
                get { return name; }
                set { name = value; }
            }

            public string Value
            {
                get { return value; }
                set { this.value = value; }
            }

            public string Type
            {
                get { return type; }
                set { this.type = value; }
            }
        }

        //SecondaryStats.
        [Header("Core Stats")]
        public int Level;
        public int Vitality;
        public int Strength;
        public int Dexterity;
        public int Wisdom;

        //Secondary Stats
        [Header("Secondary Stats")]
        public float attackSpeed;
        public float attackDamage;
        public float haste;
        public float criticalStrikeChance;
        public float criticalDamage;
        public float damageReduction;
        public float magicResist;
        public float armor;
        public float manaRegen;
        public float healthRegen;
        public float outOfCombatRegenMultiplier;
        public float healingReceivedMultiplier;
        public float damageMultiplier;
        public float damageTakenMultiplier;
        public float movementSpeed;

        //Experience Stats
        public float experience;
        public float experienceReq;

        public void ZeroOut()
        {
            foreach (var field in this.GetType().GetFields())
            {
                if (field.GetValue(this).GetType() == typeof(int))
                {
                    field.SetValue(this, 0);
                }

                if (field.GetValue(this).GetType() == typeof(float))
                {
                    field.SetValue(this, 0f);
                }
            }

            return;
            //Vitality = 0;
            //Strength = 0;
            //Dexterity = 0;
            //Wisdom = 0;

            //attackSpeed = 0;
            //attackDamage = 0;
            //criticalStrikeChance = 0;
            //criticalDamage = 0;
            //damageReduction = 0;
            //magicResist = 0;
            //armor = 0;
            //manaRegen = 0;
            //healthRegen = 0;
            //outOfCombatRegenMultiplier = 0;
            //healingReceivedMultiplier = 0;
            //damageMultiplier = 0;
            //damageTakenMultiplier = 0;
            //movementSpeed = 0;
            //haste = 0;

            //experience = 0;
            //experienceReq = 0;
        }
        public Stats Sum(Stats s1, Stats s2)
        {
            Stats s3 = new Stats();


            foreach (var field in s3.GetType().GetFields())
            {
                if (field.GetValue(s3).GetType() == typeof(int))
                {
                    var s1Value = (int)field.GetValue(s1);
                    var s2Value = (int)field.GetValue(s2);
                    

                    field.SetValue(s3, s1Value + s2Value);
                }

                if (field.GetValue(s3).GetType() == typeof(float))
                {
                    var s1Value = (float)field.GetValue(s1);
                    var s2Value = (float)field.GetValue(s2);

                    field.SetValue(s3, s1Value + s2Value);
                }
            }

            return s3;


            //s3.Level = s1.Level + s2.Level;
            //s3.Vitality = s1.Vitality + s2.Vitality;
            //s3.Strength = s1.Strength + s2.Strength;
            //s3.Dexterity = s1.Dexterity + s2.Dexterity;
            //s3.Wisdom = s1.Wisdom + s2.Wisdom;

            //s3.attackSpeed = s1.attackSpeed + s2.attackSpeed;
            //s3.attackDamage = s1.attackDamage + s2.attackDamage;
            //s3.criticalStrikeChance = s1.criticalStrikeChance + s2.criticalStrikeChance;
            //s3.criticalDamage = s1.criticalDamage + s2.criticalDamage;
            //s3.damageReduction = s1.damageReduction + s2.damageReduction;
            //s3.magicResist = s1.magicResist + s2.magicResist;
            //s3.armor = s1.armor + s2.armor;
            //s3.manaRegen = s1.manaRegen + s2.manaRegen;
            //s3.healthRegen = s1.healthRegen + s2.manaRegen;
            //s3.outOfCombatRegenMultiplier = s1.outOfCombatRegenMultiplier + s2.outOfCombatRegenMultiplier;
            //s3.healingReceivedMultiplier = s1.healingReceivedMultiplier + s2.healingReceivedMultiplier;
            //s3.damageMultiplier = s1.damageMultiplier + s2.damageMultiplier;
            //s3.damageTakenMultiplier = s1.damageTakenMultiplier + s2.damageTakenMultiplier;
            //s3.movementSpeed = s1.movementSpeed + s2.movementSpeed;
            //s3.haste = s1.haste + s2.haste;

            //s3.experience = s1.experience + s2.experience;
            //s3.experienceReq = s1.experienceReq + s2.experienceReq;

            //return s3;
        }

        public Stats Difference(Stats s1, Stats s2)
        {
            var s3 = new Stats();

            foreach (var field in s3.GetType().GetFields())
            {
                if (field.GetValue(s3).GetType() == typeof(int))
                {
                    var s1Value = (int)field.GetValue(s1);
                    var s2Value = (int)field.GetValue(s2);

                    field.SetValue(s3, s1Value - s2Value);
                }

                if (field.GetValue(s3).GetType() == typeof(float))
                {
                    var s1Value = (float)field.GetValue(s1);
                    var s2Value = (float)field.GetValue(s2);

                    field.SetValue(s3, s1Value - s2Value);
                }
            }

            return s3;

        }

        public List<Attribute> Attributes()
        {
            var attributes = new List<Attribute>();

            foreach (var field in this.GetType().GetFields())
            {
                if (field.GetValue(this).GetType() == typeof(int))
                {
                    if ((int)field.GetValue(this) == 0) continue;
                }

                if (field.GetValue(this).GetType() == typeof(float))
                {
                    if ((float)field.GetValue(this) == 0f) continue;
                }

                attributes.Add(new Attribute()
                {
                    Name = field.Name,
                    Value = field.GetValue(this).ToString(),
                    Type = field.GetValue(this).GetType().ToString()

                });
            }

            return attributes;
        }

        public List<Attribute> Cores
        {
            get { return Attributes().Where(attribute => attribute.Type.Equals(typeof(int).ToString())).ToList(); }
        }

        

        public List<Attribute> Secondaries
        {
            get { return Attributes().Where(attribute => attribute.Type.Equals(typeof(float).ToString())).ToList(); }
        }

        public Stats Copy()
        {
            var newStats = new Stats();

            foreach (var field in this.GetType().GetFields())
            {
                field.SetValue(newStats, field.GetValue(this));
            }

            return newStats;
        }

        public void UpdateCoreStats()
        {
            Vitality = (Level * 5) + 5;
            Strength = (Level * 5) + 5;
            Dexterity = (Level * 5) + 5;
            Wisdom = (Level * 5) + 5;
        }

        public void UpdateExperienceRequiredToLevel()
        {
            experienceReq = GMHelper.Difficulty().settings.experienceMultiplier * Level;
        }
    }
}