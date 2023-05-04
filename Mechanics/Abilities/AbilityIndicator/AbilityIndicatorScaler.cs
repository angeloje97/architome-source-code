using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AbilityIndicatorScaler : AbilityIndicator
    {
        [Header("Scaler Properties")]
        public Vector3 targetVector3 = Vector3.one;
        public Vector3 targetTranslation = Vector3.one;
        public Vector3 scalePoportion = Vector3.one;

        public bool useCatalystWidth;
        public bool squaredWidth;
        public float catalystWidth;
        public AnimationCurve curve;
        protected override void GetDependencies()
        {
            base.GetDependencies();

            if (ability)
            {
                ability.CatalystAction((CatalystInfo catalyst) => {
                    catalystWidth = catalyst.transform.localScale.x;
                });
            }
        }

        public Vector3 Scale(float scale)
        {
            var size = new Vector3
            {
                x = targetVector3.x == 0 ? targetTranslation.x : scale * scalePoportion.x,
                y = targetVector3.y == 0 ? targetTranslation.y : scale * scalePoportion.y,
                z = targetVector3.z == 0 ? targetTranslation.z : scale * scalePoportion.z
            };

            if (useCatalystWidth)
            {
                size.x = catalystWidth * scalePoportion.x;
            }

            if (squaredWidth)
            {
                size.y = size.x;
            }

            return size;
        }


        protected override void ValidateCurrentScale()
        {
            if (currentScale == previousScale) return;
            previousScale = currentScale;

            currentScale.x = targetVector3.x == 0 ? targetTranslation.x : currentScale.x;
            currentScale.y = targetVector3.y == 0 ? targetTranslation.y : currentScale.y;
            currentScale.z = targetVector3.z == 0 ? targetTranslation.z : currentScale.z;

            SetScale(currentScale);
        }

        public override void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            SetProjector(isActivated);
            var multiplier = ability.abilityType == AbilityType.Use ? 2f : 1f;
            SetScale(Scale(ability.range * multiplier));
        }
    }
}
