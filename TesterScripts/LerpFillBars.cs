using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Architome
{
    public class LerpFillBars : MonoBehaviour
    {
        public float totalExperienceGained;
        public float startingExperience;
        public float smoothness = 1;

        public int startingLevel;
        public int currentLevel;

        public float currentSpeed;
        public float maxSpeed;
        public float delay = 2f;
        
        [Range(0, 1)]
        public float maxSpeedStart;
        [Range(0, 1)]
        public float maxSpeedStop;

        float currentTotal, total, acceleration, decceleration, d1, d2, targetExperience, currentExperience, progress;

        

        [Serializable]
        public struct AccelerationBenchMarks
        {
            [Range(0, 1)]
            public float accelerationTarget;
        }


        [Serializable]
        public struct Components
        {
            public TextMeshProUGUI levelText;
            public Image progressBar;
        }

        public Components comps;

        void Start()
        {
            
            HandleSpeed();
            FillBar();
        }

        private void OnValidate()
        {
            if (maxSpeedStart > maxSpeedStop)
            {
                maxSpeedStop = maxSpeedStart;
            }
        }

        async void HandleSpeed()
        {
            total = totalExperienceGained - startingExperience;
            currentTotal = 0;
            d1 = total * maxSpeedStart;
            d2 = total * maxSpeedStop;

            acceleration = (maxSpeed * maxSpeed) / (2 * (d1));
            decceleration = -(maxSpeed * maxSpeed) / (2 * (total - d2));

            await Task.Delay((int)(delay * 1000));

            while (currentSpeed < maxSpeed)
            {
                currentSpeed += acceleration*Time.deltaTime;
                await Task.Yield();
            }

            currentSpeed = maxSpeed;

            while (currentTotal < d2)
            {
                await Task.Yield();
            }

            while (currentSpeed > 0)
            {
                currentSpeed += decceleration * Time.deltaTime;
                await Task.Yield();
            }

            currentSpeed = 0;

            currentTotal = total;

        }

        // Update is called once per frame
        void Update()
        {
        
        }

        async void FillBar()
        {
            var difficulty = DifficultyModifications.active;
            currentExperience = startingExperience;
            currentLevel = startingLevel;
            targetExperience = difficulty.ExperienceRequiredToLevel(currentLevel);
            comps.levelText.text = $"Level {currentLevel}";
            progress = currentExperience / targetExperience;

            comps.progressBar.fillAmount = progress;


            await Task.Delay((int)(delay * 1000));

            while (currentTotal != total)
            {
                currentTotal += currentSpeed*Time.deltaTime;
                currentExperience += currentSpeed * Time.deltaTime;


                progress = currentExperience / targetExperience;

                if (currentExperience > targetExperience)
                {
                    currentExperience = 0;
                    //totalExperienceGained -= targetExperience;
                    currentLevel++;
                    targetExperience = difficulty.ExperienceRequiredToLevel(currentLevel);
                    comps.levelText.text = $"Level {currentLevel}";
                }

                comps.progressBar.fillAmount = progress;

                await Task.Yield();
            }
        }
    }
}
