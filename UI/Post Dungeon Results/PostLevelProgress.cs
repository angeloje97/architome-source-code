using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class PostLevelProgress : MonoBehaviour
    {
        public EntityInfo entity;

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
        [SerializeField] float delay;


        int startLevel, endLevel;
        float targetValue, currentValue, totalValue;
        float startExperience, currentExperience, endExperience;
        float currentSpeed, accel, deccel;
        bool active;


        public Action<PostLevelProgress> OnLevelUp;

        public float speedPercent { get { return  currentSpeed / (maxSpeedPercent * totalValue); }}

        PostDungeonManager manager;
        private void OnValidate()
        {
            canvasGroup = GetComponent<CanvasGroup>();
            manager = GetComponentInParent<PostDungeonManager>();

            if (manager)
            {
                manager.AddPostLevelProgress(this);
            }
        }

        
        async public void SetEntity(EntityInfo entity)
        {
            this.entity = entity;

            var archSceneManager = ArchSceneManager.active;

            if (archSceneManager)
            {
                archSceneManager.BeforeLoadScene += BeforeLoadScene;
            }

            var combatInfo = entity.GetComponentInChildren<CombatInfo>();
            if (combatInfo != null)
            {
                var levels = combatInfo.combatLogs.levels;

                startLevel = levels.startingLevel;
                endLevel = levels.currentLevel;
                startExperience = levels.startingExperience;
                endExperience = levels.currentExperienceGained;
                level.text = $"Level {levels.startingLevel}";
            }

            var difficulty = DifficultyModifications.active;

            if (difficulty)
            {
                targetValue = difficulty.ExperienceRequiredToLevel(startLevel);
            }

            progressBar.fillAmount = startExperience / targetValue;
            entityName.text = entity.entityName;
            portrait.sprite = entity.entityPortrait;

            await Task.Delay((int)(1000 * delay));

            HandleSpeed();
            HandleFill();

        }
        async void HandleSpeed()
        {
            totalValue = endExperience - startExperience;
            currentValue = 0f;

            var distance1 = totalValue * maxSpeedStart;
            var distance2 = totalValue * maxSpeedStop;
            
            var maxSpeed = totalValue * maxSpeedPercent;

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
            var difficulty = DifficultyModifications.active;
            int currentLevel = startLevel;
            currentValue = 0f;
            currentExperience = startExperience;
            targetValue = difficulty.ExperienceRequiredToLevel(currentLevel);


            while (active)
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
                }



                await Task.Yield();
            }

        }
        public void SetActive(bool active)
        {
            if (canvasGroup == null) return;
            ArchUI.SetCanvas(canvasGroup, active);
            gameObject.SetActive(active);
        }
        public void BeforeLoadScene(ArchSceneManager sceneManager)
        {
            if (!active) return;
            active = false;

            var level = entity.entityStats.Level;
            var experience = entity.entityStats.experience;
            var reqExperience = entity.entityStats.experienceReq;

            this.level.text = $"Level {level}";
            progressBar.fillAmount = experience / reqExperience;
        }
    }
}
