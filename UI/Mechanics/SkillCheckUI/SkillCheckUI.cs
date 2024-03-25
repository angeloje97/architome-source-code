using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class SkillCheckUI : MonoActor
    {
        SkillCheckData currentData;

        [Header("Components")]
        [SerializeField] Image ring;
        [SerializeField] Image successArea;
        [SerializeField] Image skillCheckMarker;
        [SerializeField] CanvasGroup group;




        [Header("Testing Properties")]
        [SerializeField][Range(0f, 1f)] float range;
        [SerializeField][Range(0f, 1f)] float angle;
        [SerializeField][Range(0f, 1f)] float value;
        [SerializeField][Range(0f, 1f)] float skillCheckMarkerSize;

        [SerializeField] bool enableTesting;
        [SerializeField] bool isSuccessful;
        public void OnValidate()
        {
            if (!enableTesting) return;
            SetFrame(angle, range);
            UpdateValue(value);

            isSuccessful = value > angle - (range / 2f) && value < angle + (range / 2f);
        }
        
        public void SetTarget(Transform target)
        {
            var popupContainer = PopupContainer.active;
            var stopListening = false;

            AddListener(eMonoEvent.OnDestroy, () => {
                stopListening = true;
            }, this);

            popupContainer.StickToTarget(transform, target, () => stopListening, true);
        }

        public async void SetData(SkillCheckData data)
        {
            currentData = data;

            SetFrame(data.angle, data.range);

            await data.WhileProgress((SkillCheckData data) => {
                UpdateValue(data.value);
            });

            Destroy(gameObject);
        }
        
        public void SetFrame(float angle, float range)
        {
            successArea.fillAmount = range;

            var offset = range *.5f;

            angle = Mathf.Clamp(angle, offset, 1f - offset);
            this.angle = angle;


            var zAngle = Mathf.Lerp(0f, 360f, (angle + offset));
            successArea.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle);
        }

        public void UpdateValue(float value)
        {
            var zAngle = Mathf.Lerp(0f, 360f, value);
            skillCheckMarker.fillAmount = skillCheckMarkerSize;
            skillCheckMarker.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle + (skillCheckMarkerSize * 50f));
        }
    }
}