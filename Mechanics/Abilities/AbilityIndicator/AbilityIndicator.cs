using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;

namespace Architome
{
    public class AbilityIndicator : MonoBehaviour
    {
        public DecalProjector projector;
        public AbilityInfo ability;

        public TransformType transformType;
        TransformType previousTransformType;

        public bool projectorActive;
        bool projectorActiveCheck;

        public Vector3 pivotMultiplier;
        public Vector3 currentScale = Vector3.one;
        Vector3 previousScale;



        public enum TransformType
        {
            Transform,
            Projector,
        }

        protected virtual void Start()
        {
            projectorActiveCheck = !projectorActive;

            GetDependencies();
            UpdateProjector();


        }

        // Update is called once per frame
        protected virtual void Update()
        {
        
        }

        protected virtual void GetDependencies()
        {
            projector ??= GetComponent<DecalProjector>();
            ability ??= GetComponentInParent<AbilityInfo>();

            if (ability)
            {
                ability.OnAbilityStartEnd += OnAbilityStartEnd;
            }
        }

        protected virtual void OnValidate()
        {

            UpdateTransformType();

            if(previousScale != currentScale)
            {
                previousScale = currentScale;
                SetScale(currentScale);
            }

            if (projectorActive)
            {
                foreach(var indicator in GetComponentsInParent<AbilityIndicator>())
                {
                    if (!indicator.projectorActive)
                    {
                        return;
                    }
                }
                SetProjector(true);
            }

            foreach(var indicator in GetComponentsInChildren<AbilityIndicator>())
            {
                indicator.projector.enabled = projectorActive;

                if (projectorActive)
                {
                    indicator.UpdateProjector(true);
                }

            }
        }


        #region Projector

        public void SetProjector(bool val)
        {
            projectorActive = val;
            UpdateProjector();
        }

        public void ToggleProjector()
        {
            projectorActive = !projectorActive;
            UpdateProjector();
        }

        public void UpdateProjector(bool forceUpdate = false)
        {
            if (projectorActive == projectorActiveCheck && !forceUpdate) return;
            projectorActiveCheck = projectorActive;
            projector.enabled = projectorActive;
        }

        public void UpdateTransformType()
        {
            if (projector == null) return;
            if (previousTransformType == transformType) return;
            previousTransformType = transformType;

            if(transformType == TransformType.Transform)
            {
                transform.localScale = projector.size;
                projector.size = Vector3.one;
                projector.scaleMode = DecalScaleMode.InheritFromHierarchy;
            }
            else
            {
                projector.size = transform.localScale;
                transform.localScale = Vector3.one;
                projector.scaleMode = DecalScaleMode.ScaleInvariant;
            }
        }

        public void SetScale(Vector3 scale)
        {
            scale.z = 1;
            currentScale = scale;
            if(previousScale != currentScale)
            {
                previousScale = currentScale;
            }

            if(transformType == TransformType.Transform)
            {
                transform.localScale = scale;
            }
            else
            {
                projector.size = scale;
                projector.pivot = V3Helper.Multiply(pivotMultiplier, scale);
            }

        }

        #endregion

        #region AbilityListener

        public virtual void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            SetProjector(isActivated);
            SetScale(Vector3.one * ability.range);
        }

        #endregion

    }
}
