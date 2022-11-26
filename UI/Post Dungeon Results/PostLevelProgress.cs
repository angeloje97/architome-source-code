using System;
using System.Collections;
using System.Threading.Tasks;
using Architome.Enums;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PostLevelProgress : MonoBehaviour
    {
        public EntityInfo entity;
        DifficultyModifications difficulty;

        [Header("Components")]
        public CanvasGroup canvasGroup;
        public Image portrait, progressBar;
        public TextMeshProUGUI entityName, level;

        [Header("Settings")]
        [Range(0, 1)]
        [SerializeField] float maxSpeedStart;
        [Range(0, 1)]
        [SerializeField] float maxSpeedStop;
        [Range(0, 1)]
        [SerializeField] float maxSpeedPercent = .125f;
        [Range(0, 1)]
        [SerializeField] float maxSpeedPercentOnGain;
        [SerializeField] float delay;


        int startLevel, endLevel;
        float targetValue, currentValue, totalValue;
        float startExperience, currentExperience, endExperience;
        float currentSpeed, accel, deccel;
        bool active;

        ArchSceneManager archSceneManager;


        public Action<PostLevelProgress> OnLevelUp;
        public Action<PostLevelProgress> OnFillStart;
        public Action<PostLevelProgress> OnFillEnd;


        public float speedPercent { get { return  currentSpeed / (maxSpeedPercent * totalValue); }}

        PostDungeonManager manager;
        private void OnValidate()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            manager = GetComponentInParent<PostDungeonManager>();
        }

        private void Start()
        {
            GetDependencies();
        }

        void GetDependencies()
        {
            difficulty = DifficultyModifications.active;
        }

        async public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;

            archSceneManager = ArchSceneManager.active;

            if (archSceneManager)
            {
                archSceneManager.AddListener(SceneEvent.BeforeLoadScene, BeforeLoadScene, this);
            }

            entity.OnExperienceGain += OnEntityGainExperience;

            var combatInfo = entity.GetComponentInChildren<CombatInfo>();
            if (combatInfo != null)
            {
                var levels = combatInfo.combatLogs.levels;

                startLevel = levels.startingLevel;
                endLevel = levels.currentLevel;
                startExperience = levels.startingExperience;
                level.text = $"Level {levels.startingLevel}";
            }

            endExperience = entity.entityStats.experience;

            //lastProgress = entity.entityStats.experience / entity.entityStats.experienceReq;

            difficulty = DifficultyModifications.active;

            if (difficulty)
            {
                targetValue = difficulty.ExperienceRequiredToLevel(startLevel);
            }

            progressBar.fillAmount = startExperience / targetValue;
            entityName.text = entity.entityName;
            portrait.sprite = entity.PortraitIcon();

            active = true;

            await Task.Delay((int)(1000 * delay));

            HandleSpeed();
            HandleFill();

        }
        async void HandleSpeed(bool onGainExperience = false)
        {


            var experienceBetweenLevels = ExperienceBetweenLevels(startLevel, endLevel);


            totalValue = (endExperience - startExperience) + experienceBetweenLevels;
            currentValue = 0f;
            Debugger.InConsole(5324, $"Total Value : {totalValue}, Starting : {startExperience}, end experience: {endExperience}, Experience Between Levels: {experienceBetweenLevels}");


            var distance1 = totalValue * maxSpeedStart;
            var distance2 = totalValue * maxSpeedStop;

            float maxSpeed;

            if (onGainExperience)
            {
                maxSpeed = totalValue * maxSpeedPercentOnGain;
            }
            else
            {
                maxSpeed = totalValue * maxSpeedPercent;
            }

            accel = (maxSpeed * maxSpeed) / (2 * (distance1));
            deccel = -(maxSpeed * maxSpeed) / (2 * (totalValue - distance2));


            active = true;
            while (currentSpeed < maxSpeed)
            {
                currentSpeed += accel * Time.deltaTime;
                if (active == false) return;
                await Task.Yield();
            }

            currentSpeed = maxSpeed;

            while (currentValue < distance2)
            {
                if (active == false) return;
                await Task.Yield();
            }

            while (currentSpeed > 0)
            {
                if (active == false) return;
                currentSpeed += deccel * Time.deltaTime;
                await Task.Yield();
            }

            currentSpeed = 0f;
            active = false;
        }
        async void HandleFill()
        {


            int currentLevel = startLevel;
            currentValue = 0f;
            currentExperience = startExperience;
            targetValue = difficulty.ExperienceRequiredToLevel(currentLevel);


            OnFillStart?.Invoke(this);
            while (currentValue < totalValue)
            {
                currentValue += Time.deltaTime * currentSpeed;
                currentExperience += Time.deltaTime * currentSpeed;

                var progress = currentExperience / targetValue;
                progressBar.fillAmount = progress;
                
                if (currentExperience > targetValue)
                {

                    currentExperience -= targetValue;
                    currentLevel++;
                    targetValue = difficulty.ExperienceRequiredToLevel(currentLevel);
                    level.text = $"Level {currentLevel}";
                    OnLevelUp?.Invoke(this);
                }
                
                await Task.Yield();
            }

            level.text = $"Level {endLevel}";
            progressBar.fillAmount = endExperience / targetValue;

            OnFillEnd?.Invoke(this);


        }
        public void SetActive(bool active)
        {
            if (canvasGroup == null) return;
            ArchUI.SetCanvas(canvasGroup, active);
            gameObject.SetActive(active);
        }
        async Task CatchUp()
        {
            active = false;

            await Task.Yield();

            var level = entity.entityStats.Level;
            var experience = entity.entityStats.experience;
            var reqExperience = entity.entityStats.experienceReq;

            this.level.text = $"Level {level}";
            progressBar.fillAmount = experience / reqExperience;
        }
        public async Task Progress()
        {
            while (active)
            {
                await Task.Yield();
            }
        }
        async void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            if (!active) return;
            active = false;

            await CatchUp();
        }
        public float ExperienceBetweenLevels(int startingLevel, int targetLevel)
        {
            var value = 0f;

            for (int i = startingLevel; i < targetLevel; i++)
            {
                value += difficulty.ExperienceRequiredToLevel(i);
            }

            return value;
        }
        async void OnEntityGainExperience(float amount)
        {
            if (active)
            {
                active = false;
                await Task.Delay(125);
            }

            //progressBar.fillAmount = lastProgress;
            level.text = $"Level {endLevel}";
            targetValue = difficulty.ExperienceRequiredToLevel(endLevel);
            startLevel = endLevel;

            endLevel = entity.entityStats.Level;

            //lastProgress = entity.entityStats.experience / entity.entityStats.experienceReq;

            startExperience = endExperience;
            endExperience = entity.entityStats.experience;


            HandleSpeed(true);
            HandleFill();
        }
    }
}
