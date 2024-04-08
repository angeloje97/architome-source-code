using Architome.Settings.Keybindings;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Cheat Commit

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

        public SkillCheckData skillCheckData => currentData;

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
            UpdateValue(0f);
        }

        #endregion

        #region Updating UI

        bool movedToTarget { get; set; }

        #region UI Updaters

        public async void SetTarget(Transform target)
        {
            var popupContainer = PopupContainer.active;
            var stopListening = false;



            AddListener(eMonoEvent.OnDestroy, () =>
            {
                stopListening = true;
            }, this);


            await popupContainer.StickToTarget(transform, target, () => {

                if (!movedToTarget)
                {
                    movedToTarget = true;
                }

                return stopListening; 
            }, true, new Vector3(0f, 150f, 0f));

        }
        void SetFrame(float angle, float range)
        {
            successArea.fillAmount = range;

            var offset = range * .5f;

            angle = Mathf.Clamp(angle, offset, 1f - offset);
            this.angle = angle;


            var zAngle = Mathf.Lerp(0f, 360f, (angle + offset));
            successArea.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle);
        }
        public void SetKeyBindIcon(KeybindListener.ListenerEvent listener)
        {
            keybind.spriteAsset = SpriteAssetData.active.SpriteAsset(SpriteAssetType.KeyBindings);
            //var finalString = new StringBuilder();

            foreach (var type in listener.setTypes)
            {
                var spriteIndex = KeyBindings.active.SpriteIndex(type, listener.bindingName);
                keybind.text = $"<sprite={spriteIndex}>";
            }

        }
        void UpdateValue(float value)
        {
            var zAngle = Mathf.Lerp(0f, 360f, value);
            skillCheckMarker.fillAmount = skillCheckMarkerSize;
            skillCheckMarker.rectTransform.eulerAngles = new Vector3(0f, 0f, zAngle + (skillCheckMarkerSize * 50f));
        }

        #endregion

        #region SkillCheckData handling

        public async void SetData(SkillCheckData data)
        {
            currentData = data;

            SetFrame(data.angle, data.range);

            await ArchAction.WaitUntil(() => movedToTarget, true);

            var revealTime = data.delay * .5f;

            await canvasGroup.SetCanvasAsync(true, revealTime);

            data.AddListener(eSkillCheckEvent.OnEnd, OnEndSkillCheck, this);

            await data.WhileProgress((SkillCheckData data) => {
                UpdateValue(data.value);
            });

            await Task.Delay(500);

            await canvasGroup.SetCanvasAsync(false, revealTime);

            await HandleBackgroundProcesses();

            Destroy(gameObject);
        }

        public Action<List<Func<Task>>> OnHandleBackgroundProcesses { get; set; }
        List<Func<Task>> backgroundProcesses;

        public async Task HandleBackgroundProcesses()
        {
            backgroundProcesses = new();
            OnHandleBackgroundProcesses?.Invoke(backgroundProcesses);
            await backgroundProcesses.HandleTasks(TaskType.Parallel);
        }

        #endregion

        #endregion

        public void OnEndSkillCheck(SkillCheckData data)
        {

        }

        public void HitSkillCheck()
        {
            Debugger.System(1014, "Hitting Skillcheck");
            currentData.HitSkillCheck();
        }


        public void CancelSkillCheck()
        {
            currentData.CancelSkillCheck();
        }
    }
}