using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;

namespace Architome
{
    public class AbilityIndicator : MonoBehaviour
    {
        AbilityIndicator parentIndicator;
        public DecalProjector projector;
        public AbilityInfo ability;


        public bool projectorActive;
        bool projectorActiveCheck;

        public Vector3 pivotMultiplier;
        public Vector3 currentScale = Vector3.one;
        protected Vector3 previousScale;



        protected virtual void Start()
        {
            projectorActiveCheck = !projectorActive;

            GetDependencies();
            SetToFloor();
            UpdateProjector();
            SetProjector(false);


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

            parentIndicator = transform.parent.GetComponent<AbilityIndicator>();
        }

        #region Validation
        protected void OnValidate()
        {

            Validate();
            
        }

        protected virtual void Validate()
        {
            ValidateCurrentScale();
            ValidateProjectorActive();
        }

        protected virtual void ValidateCurrentScale()
        {
            if (previousScale != currentScale)
            {
                previousScale = currentScale;
                SetScale(currentScale);
            }
        }

        protected virtual void ValidateProjectorActive()
        {
            if (projectorActive == projectorActiveCheck) return;
            projectorActiveCheck = projectorActive;

            if (projectorActive)
            {
                foreach (var indicator in GetComponentsInParent<AbilityIndicator>())
                {
                    if (!indicator.projectorActive)
                    {
                        return;
                    }
                }
                SetProjector(true);
            }

            foreach (var indicator in GetComponentsInChildren<AbilityIndicator>())
            {
                indicator.projector.enabled = projectorActive;

                if (projectorActive)
                {
                    indicator.UpdateProjector(true);
                }

            }
        }

        #endregion


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


        public virtual void SetScale(Vector3 scale)
        {
            scale.z = 1;
            currentScale = scale;
            if(previousScale != currentScale)
            {
                previousScale = currentScale;
            }

            if(projector.scaleMode == DecalScaleMode.InheritFromHierarchy)
            {
                transform.localScale = scale;
            }
            else
            {
                projector.size = scale;
                projector.pivot = V3Helper.Multiply(scale, pivotMultiplier);
            }

        }

        public async void SetToFloor()
        {
            if (parentIndicator != null) return;
            await Task.Delay(1000);
            var groundLayer = LayerMasksData.active.walkableLayer;
            var groundPosition = V3Helper.GroundPosition(transform.position, groundLayer, 0, .65f);
            Debugger.Environment(5978, $"Setting to floor {groundPosition}");
            transform.position = new Vector3(transform.position.x, groundPosition.y, transform.position.z);
        }

        #endregion

        #region AbilityListener

        public virtual void OnAbilityStartEnd(AbilityInfo ability, bool isActivated)
        {
            SetProjector(isActivated);
        }

        #endregion

    }
}
