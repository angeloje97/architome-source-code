using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Threading.Tasks;

namespace Architome.Indicator
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
        public float decalThickness = 1f;

        public LayerMask groundLayer;


        protected virtual void Start()
        {
            projectorActiveCheck = !projectorActive;

            GetDependencies();
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

            var layerMasks = LayerMasksData.active;
            if (layerMasks)
            {
                groundLayer = layerMasks.walkableLayer;
            }
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

            if (projectorActive)
            {
                SetToFloor(0);
            }
            projector.enabled = projectorActive;
        }


        public virtual void SetScale(Vector3 scale)
        {
            scale.z = decalThickness;
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

        public virtual void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public async void SetToFloor(int delay = 2000)
        {
            if (parentIndicator != null) return;
            await Task.Delay(delay);
            var groundPosition = V3Helper.GroundPosition(transform.position, groundLayer, 0, 0f);
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
