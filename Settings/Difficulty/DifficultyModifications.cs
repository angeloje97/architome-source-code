using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using Architome.SkillCheck;
using System.Linq;

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
        public Dictionary<Difficulty, DifficultySet> difficultyDict { get; set; }
        public Dictionary<string, DifficultySet> difficultyDictString { get; set; }
        public DifficultySet settings { get; private set; }
        


        public void DetermineSettings()
        {
            UpdateDictionary();
            settings = DifficultySet(gameDifficulty);
        }


        void UpdateDictionary()
        {
            difficultyDict = new();
            difficultyDictString = new();

            foreach (var set in difficultySets)
            {
                if (!difficultyDictString.ContainsKey(set.name))
                {
                    difficultyDictString.Add(set.name, set);
                }

                if (!difficultyDict.ContainsKey(set.difficulty))
                {
                    difficultyDict.Add(set.difficulty, set);
                }
            }
        }

        private void Awake()
        {
            active = this;

            if (Core.currentSave != null)
            {
                gameDifficulty = Core.currentSave.gameSettings.difficulty;
            }

            difficultySets = SerializedSets.GetSavedSets(difficultySets).difficultySets;

            DetermineSettings();
        }

        public DifficultySet DifficultySet(Difficulty type)
        {
            difficultyDict ??= new();
            if (!difficultyDict.ContainsKey(type)) return new();

            return difficultyDict[type];
        }

        public DifficultySet DifficultySet(string alias, bool createNewIfNull)
        {
            foreach(var set in difficultySets)
            {
                if (set.name.Equals(alias)) return set;
            }

            if (!createNewIfNull) return new();

            var newSet = new DifficultySet() { name = alias };

            difficultySets.Add(newSet);

            return newSet;
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

        public void SaveCurrentSet()
        {
            SerializedSets.SaveSets(new(difficultySets));
        }
    }

    [Serializable]
    public class SerializedSets 
    {
        public List<DifficultySet> difficultySets;
        static string fileName => "DifficultySets";

        public SerializedSets(List<DifficultySet> sets)
        {
            this.difficultySets = sets.ToList();
        }

        public static SerializedSets GetSavedSets(List<DifficultySet> defaultSets)
        {
            var setData = (SerializedSets) SerializationManager.LoadConfig(fileName);
            if(setData == null)
            {
                setData = new(defaultSets);
                SerializationManager.SaveConfig(fileName, setData);

                Debugger.System(6195, $"Failed to locate saved difficulty set. Creating new one");
            }

            return setData;
        }

        public static void SaveSets(SerializedSets setToSave)
        {
            SerializationManager.SaveConfig(fileName, setToSave);
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

        [Header("Misc Difficulty Settings")]
        public SkillCheckOffsets skillCheckOffsets;



        static Dictionary<string, string> aliases => new Dictionary<string, string>() 
        {
            { "dungeonCoreMultiplier", "Extra Enemy Power" },
            { "experienceRequiredMultiplier", "Experience Required Per Level" }
        };

        static Dictionary<string, string> unit => new Dictionary<string, string>()
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

                var title = aliases.ContainsKey(field.Name) ? aliases[field.Name] : ArchString.CamelToTitle(field.Name);
                var unitOfMeasurement = unit.ContainsKey(field.Name) ? unit[field.Name] : "";
                result.Add($"{title}: {field.GetValue(this)}{unitOfMeasurement}");
            }

            return ArchString.NextLineList(result);
        }
    }

}