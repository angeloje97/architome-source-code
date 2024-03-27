using Architome.Settings.Keybindings;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class SkillCheckUI : MonoActor
    {

        #region Common Data
        SkillCheckData currentData;

        [Header("Components")]
        [SerializeField] Image ring;
        [SerializeField] Image successArea;
        [SerializeField] Image skillCheckMarker;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] TextMeshProUGUI keybind;
        [SerializeField] Animator animator;

        AnimationHandler<eSkillCheckEvent> animationHandler;

        #endregion

        #region Testing

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

        #endregion

        #region Initialization
        protected override void Awake()
        {
            base.Awake();
            canvasGroup.SetCanvas(false);
            animationHandler = new(animator);
        }

        #endregion

        #region Animations

        public void HandleAnimations()
        {
            animationHandler = new(animator);
        }

        public void HandleSkillCheckDatas(SkillCheckData data)
        {
            data.AddAllListeners((eSkillCheckEvent trigger, SkillCheckData data) => {
                animationHandler.SetTrigger(trigger);
            }, this);
        }

        #endregion


        public void SetTarget(Transform target)
        {
            var popupContainer = PopupContainer.active;
            var stopListening = false;

            AddListener(eMonoEvent.OnDestroy, () => {
                stopListening = true;
            }, this);

            //ArchAction.Yield(() => {
            //    canvasGroup.SetCanvas(true);
            //});

            var movedToTarget = false;

            popupContainer.StickToTarget(transform, target, () => {

                if (!movedToTarget)
                {
                    movedToTarget = true;
                    canvasGroup.SetCanvas(true);
                }

                return stopListening; 
            }, true);
        }

        public async void SetData(SkillCheckData data)
        {
            currentData = data;

            SetFrame(data.angle, data.range);
                

            data.AddListener(eSkillCheckEvent.OnEndSkillCheck, OnEndSkillCheck, this);

            await data.WhileProgress((SkillCheckData data) => {
                UpdateValue(data.value);
            });

            await Task.Delay(500);

            Destroy(gameObject);
        }
        
        void SetFrame(float angle, float range)
        {
            successArea.fillAmount = range;

            var offset = range *.5f;

            angle = Mathf.Clamp(angle, offset, 1f - offset);
            this.angle = angle;


            var zAngle = Mathf.Lerp(0f, 360f, (angle + offset));
            successArea.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle);
        }

        void UpdateValue(float value)
        {
            var zAngle = Mathf.Lerp(0f, 360f, value);
            skillCheckMarker.fillAmount = skillCheckMarkerSize;
            skillCheckMarker.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle + (skillCheckMarkerSize * 50f));
        }

        public void OnEndSkillCheck(SkillCheckData data)
        {

        }

        public void HitSkillCheck()
        {
            Debugger.System(1014, "Hitting Skillcheck");
            currentData.HitSkillCheck();
        }

        public void SetKeyBindIcon(KeybindListener.ListenerEvent listener)
        {
            keybind.spriteAsset = SpriteAssetData.active.SpriteAsset(SpriteAssetType.KeyBindings);
            //var finalString = new StringBuilder();

            foreach(var type in listener.setTypes)
            {
                var spriteIndex = KeyBindings.active.SpriteIndex(type, listener.bindingName);
                keybind.text = $"<sprite={spriteIndex}>";
            }

        }
    }
}