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
            [SerializeField]
            object data;
            [SerializeField]
            bool positive;



            public object Data
            {
                get { return data; }
                set { data = value; }
            }

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

            public bool Positive
            {
                get { return positive; }
                set { positive = value; }
            }
        }

        //SecondaryStats.
        [Header("Core Stats")]
        public int Level = 1;
        public int Vitality = 10;
        public int Strength = 10;
        public int Dexterity = 10;
        public int Wisdom = 10;

        //Secondary Stats
        [Header("Secondary Stats")]
        public float attackSpeed = 1;               //%
        public float attackDamage;                   
        public float haste;                         //%
        public float criticalChance;                //%
        public float criticalDamage = 1.50f;        //%
        public float damageReduction;               //%
        public float magicResist;     
        public float armor;
        public float manaRegen;
        public float healthRegen;
        public float outOfCombatRegenMultiplier;    //%
        public float healingReceivedMultiplier;     //%
        public float damageMultiplier = 1;          //%
        public float damageTakenMultiplier = 1;     //%
        public float movementSpeed = 1;             //%

        //Experience Stats
        public float experience;
        public float experienceReq;

        public static HashSet<string> HiddenFields
        {
            get
            {
                return new HashSet<string>()
                {
                    "experience",
                    "experienceReq",
                    "outOfCombatRegenMultiplier",
                    "damageMultiplier",
                    "damageTakenMultiplier",
                    "healingReceivedMultiplier",
                };
            }
        }

        public static HashSet<string> PercentageFields
        {
            get
            {
                return new HashSet<string>()
                {
                    "attackSpeed",
                    "haste",
                    "criticalChance",
                    "criticalDamage",
                    "damageReduction",
                    "outOfCombatRegenMultiplier",
                    "healingReceivedMultiplier",
                    "damageMultiplier",
                    "damageTakenMultiplier",
                    "movementSpeed"
                };
            }
        }

        public static HashSet<string> PerSecondFields 
        {
            get
            {
                return new HashSet<string>()
                {

                };
            }
        }
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

        public static Stats operator +(Stats s1, Stats s2)
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
        }

        public static Stats operator *(Stats s1, Stats s2)
        {
            Stats s3 = new Stats();

            foreach (var field in s3.GetType().GetFields())
            {
                if (field.GetValue(s3).GetType() == typeof(int))
                {
                    var s1Value = (int)field.GetValue(s1);
                    var s2Value = (int)field.GetValue(s2);


                    field.SetValue(s3, s1Value * s2Value);
                }

                if (field.GetValue(s3).GetType() == typeof(float))
                {
                    var s1Value = (float)field.GetValue(s1);
                    var s2Value = (float)field.GetValue(s2);

                    field.SetValue(s3, s1Value * s2Value);
                }
            }

            return s3;
        }

        public static Stats operator *(Stats s1, float value)
        {
            Stats s3 = new Stats();



            foreach (var field in s3.GetType().GetFields())
            {
                if (field.GetValue(s3).GetType() == typeof(int))
                {
                    var s1Value = (int)field.GetValue(s1);


                    field.SetValue(s3, s1Value * (int) value);
                }

                if (field.GetValue(s3).GetType() == typeof(float))
                {
                    var s1Value = (float)field.GetValue(s1);

                    field.SetValue(s3, s1Value * value);
                }
            }

            return s3;
        }

        public static Stats operator -(Stats s1, Stats s2)
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

        public void Copy(Stats stats)
        {
            var s3 = new Stats();

            foreach (var field in s3.GetType().GetFields())
            {
                if (field.GetValue(s3).GetType() == typeof(int))
                {
                    var s1Value = (int)field.GetValue(stats);

                    field.SetValue(this, s1Value);

                }

                if (field.GetValue(s3).GetType() == typeof(float))
                {
                    var s1Value = (float)field.GetValue(stats);

                    field.SetValue(this, s1Value);
                }
            }

        }

        public List<Attribute> Attributes()
        {
            var attributes = new List<Attribute>();

            foreach (var field in this.GetType().GetFields())
            {
                bool positive = false;
                if (field.GetValue(this).GetType() == typeof(int))
                {
                    positive = (int)field.GetValue(this) >= 0;
                    if ((int)field.GetValue(this) == 0) continue;

                }

                if (field.GetValue(this).GetType() == typeof(float))
                {

                    positive = (float)field.GetValue(this) >= 0;
                    
                    if ((float)field.GetValue(this) == 0f) continue;
                }

                attributes.Add(new Attribute()
                {
                    Name = field.Name,
                    Value = field.GetValue(this).ToString(),
                    Type = field.GetValue(this).GetType().ToString(),
                    Data = field.GetValue(this),
                    Positive = positive
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