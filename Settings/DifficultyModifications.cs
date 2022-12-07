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
        public DifficultySet settings;


        public void DetermineSettings()
        {
            settings = difficultySets[((int)gameDifficulty) - 1];
            UpdateDictionary();
        }

        void Start()
        {
            //DetermineSettings();
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
            if (!difficultyDict.ContainsKey(type)) return null;

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
            return settings.experienceMultiplier * currentLevel;
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
        public float experienceMultiplier = 300f;
        public float dungeonCoreMultiplier = .15f;
        [Range(0, 1)]
        public float minimumEnemyForces = .90f;

        public static readonly HashSet<string> ignoreField = new HashSet<string>()
        {
            "name",
            "ignoreField"
        };

        public string Description()
        {
            var result = new List<string>();
            foreach(var field in GetType().GetFields())
            {
                if (ignoreField.Contains(field.Name)) continue;
                result.Add($"{ArchString.CamelToTitle(field.Name)}: {field.GetValue(this)}");
            }

            return ArchString.NextLineList(result);
        }
    }

}