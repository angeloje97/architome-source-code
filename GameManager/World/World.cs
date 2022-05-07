using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Linq;


namespace Architome
{
    public class World : MonoBehaviour
    {

        public static World active;
        // Start is called before the first frame update
        public GameObject defaultCatalyst;

        public float baseMovementSpeed;
        public float baseWalkSpeed;
        public float lengthOfDay;

        public float currentTime, deltaTime;
        public int timeScale = 1;
        public int day;

        public int week { get { return day % 7; } }
        public float hour { get { return (currentTime / lengthOfDay) * 24; } }
        public float minute { get { return (hour - (int)hour) * 60; } }

        public float clockHour;
        public float clockMinute;

        [Serializable]
        public class RarityProperties
        {
            public Rarity name;
            public Color color;
            public float valueMultiplier;
        }
        public List<RarityProperties> rarities;


        [Serializable]
        public class NPCProperty
        {
            public NPCType npcType;
            public Color color;
        }

        public List<NPCProperty> npcProperties; 

        void Start()
        {

        }

        private void Awake()
        {
            if (active)
            {
                foreach (var field in typeof(World).GetFields())
                {
                    field.SetValue(this, field.GetValue(active));
                }

                foreach (var property in typeof(World).GetProperties())
                {
                    property.SetValue(this, property.GetValue(active));
                }
                
            }

            active = this;
        }

        // Update is called once per frame
        void Update()
        {
            HandleTimers();
        }

        void HandleTimers()
        {
            var next = currentTime + Time.deltaTime * timeScale;
            deltaTime = next - currentTime;
            currentTime = next;
            clockHour = hour;
            clockMinute = minute;

            if (currentTime > lengthOfDay)
            {
                day++;
                currentTime = 0;

            }
        }

        private void OnValidate()
        {
            CreateProperties();
        }

        void CreateProperties()
        {
            foreach (Rarity rarity in Enum.GetValues(typeof(Rarity)))
            {
                if (ContainsRarity(rarity))
                {
                    continue;
                }

                rarities.Add(new() { name = rarity});
            }

            bool ContainsRarity(Rarity rarity)
            {
                foreach (var property in rarities)
                {
                    if (property.name == rarity)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public RarityProperties RarityColor(Rarity rarity)
        {
            foreach (var property in rarities)
            {
                if (property.name == rarity)
                {
                    return property;
                }
            }

            return new RarityProperties();
        }

        public NPCProperty NPCPRoperty(NPCType type)
        {
            foreach (var property in npcProperties)
            {
                if (property.npcType == type)
                {
                    return property;
                }
            }

            return null;
        }
    }

}