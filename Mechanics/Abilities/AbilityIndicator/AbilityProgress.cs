using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms;

namespace Architome.Indicator
{
    public class AbilityProgress : AbilityIndicator
    {
        [Header("Progress Settings")]
        [Min(1f)]
        public float progressSpeed = 1f;
        public float maxRangeOffset;
        public float currentRange;
        [Range(0f, 1f)]
        public float currentProgress;
        public bool ignoreX, ignoreY;

        float previousProgress;


        protected override void Validate()
        {
            base.Validate();

            if(currentProgress != previousProgress)
            {
                previousProgress = currentProgress;
                UpdateProgress(currentProgress);
            }

        }

        void UpdateProgress(float newProgress)
        {

            var xTarget = ignoreX ? 1 : newProgress;
            var yTarget = ignoreY ? 1 : newProgress;

            SetScale(new(xTarget, yTarget, 1f));

        }

        public async override void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            if (!isActivated) return;
            SetProjector(true);

            currentRange = ability.range;

            while (ability.activated)
            {
                UpdateProgress(ability.progress);
                await Task.Yield();


            }
            SetProjector(false);

        }
    }
}
