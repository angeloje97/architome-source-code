using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;

namespace Architome
{
    public class DifficultyModifications : MonoBehaviour
    {
        // Start is called before the first frame update
        public static DifficultyModifications active;
        public Difficulty gameDifficulty;
        [SerializeField]
        private bool startUpdate;
        public List<DifficultySet> difficultySets;
        public Dictionary<Difficulty, DifficultySet> difficultyDict;
        public DifficultySet settings { get; private set; }
        


        public void DetermineSettings()
        {
            UpdateDictionary();
            settings = DifficultySet(gameDifficulty);
        }


        void UpdateDictionary()
        {
            difficultyDict = new();
            foreach(var difficultySet in difficultySets)
            {
                if (difficultyDict.ContainsKey(difficultySet.difficulty)) continue;

                difficultyDict.Add(difficultySet.difficulty, difficultySet);
            }
        }

        private void Awake()
        {
            active = this;

            if (Core.currentSave != null)
            {
                gameDifficulty = Core.currentSave.gameSettings.difficulty;
            }

            DetermineSettings();
        }

        public DifficultySet DifficultySet(Difficulty type)
        {
            difficultyDict ??= new();
            if (!difficultyDict.ContainsKey(type)) return new();

            return difficultyDict[type];
        }

        public void OnValidate()
        {
            if (startUpdate)
            {
                startUpdate = false;
                DetermineSettings();
            }


            foreach(var set in difficultySets)
            {
                set.name = set.difficulty.ToString();
            }
        }

        // Update is called once per frame
        void Update()
        {

        }

        public float ExperienceRequiredToLevel(int currentLevel)
        {
            return settings.experienceRequiredMultiplier * currentLevel;
        }


    }

    [Serializable]
    public class DifficultySet
    {
        [HideInInspector] public string name;
        public Difficulty difficulty;
        public float tankThreatMultiplier = 9f;
        public float healThreatMultiplier = .0625f;
        public float tankHealthMultiplier = 2.5f;
        public float npcDetectionRange = 10f;
        public float playerDetectionRange = 45f;
        public float experienceRequiredMultiplier = 300f;
        public float dungeonCoreMultiplier = .15f;
        [Range(0, 1)]
        public float minimumEnemyForces = .90f;

        static readonly HashSet<string> ignoreField = new HashSet<string>()
        {
            "name",
            "ignoreField",
            "aliases"
        };


        static readonly Dictionary<string, string> aliases = new Dictionary<string, string>() 
        {
            { "dungeonCoreMultiplier", "Extra Enemy Power" },
            { "experienceRequiredMultiplier", "Experience Required Per Level" }
        };

        static readonly Dictionary<string, string> unit = new Dictionary<string, string>()
        {
            { "npcDetectionRange", "m" },
            { "playerDetectionRange", "m" },
            { "minimumEnemyForces", "%" },
            { "experienceRequiredMultiplier", "exp/level" }
        };

        public void Copy(DifficultySet otherSet)
        {
            var fields = typeof(DifficultySet).GetFields();

            foreach(var field in fields)
            {
                var value = field.GetValue(otherSet);
                field.SetValue(this, value);
            }
        }

        public string Description()
        {
            var result = new List<string>();
            foreach(var field in GetType().GetFields())
            {
                if (ignoreField.Contains(field.Name)) continue;

                var title = aliases.ContainsKey(field.Name) ? aliases[field.Name] : ArchString.CamelToTitle(field.Name);
                var unitOfMeasurement = unit.ContainsKey(field.Name) ? unit[field.Name] : "";
                result.Add($"{title}: {field.GetValue(this)}{unitOfMeasurement}");
            }

            return ArchString.NextLineList(result);
        }
    }

}