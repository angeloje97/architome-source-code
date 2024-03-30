using Architome.Events;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public enum eSkillCheckDifficulty
    {
        Easy,
        Medium,
        Hard,
        Impossible,
    }

    #region SkillCheckData
    public enum eSkillCheckEvent
    {
        OnHit,
        OnEnd,
        OnStart,
        OnStartBeforeDelay,
        OnFail,
        OnSuccess,
    }

    [Serializable]
    public class SkillCheckData
    {
        #region Common Data

        public Transform target { get; private set; }

        public MonoActor source { get; private set; }

        public float angle { get; private set; }
        public float value { get; private set; }
        public bool success { get; private set; }
        public float range { get; private set; }
        public float offSet { get; private set; }

        public float delay { get; private set; }

        public bool active;



        ArchEventHandler<eSkillCheckEvent, SkillCheckData> skillCheckEventHandler;



        public void Invoke(eSkillCheckEvent trigger, SkillCheckData data) => skillCheckEventHandler.Invoke(trigger, data);
        public Action AddListener(eSkillCheckEvent trigger,Action<SkillCheckData> action, MonoActor listener) => skillCheckEventHandler.AddListener(trigger, action, listener);

        public Action AddAllListeners(Action<eSkillCheckEvent, SkillCheckData> action, MonoActor listener) => skillCheckEventHandler.AddAllListeners(action, listener);

        #endregion


        public SkillCheckData(MonoActor source)
        {
            this.source = source;
            skillCheckEventHandler = new(source);
        }

        bool started;
        public async void StartSkillCheck(Action<SkillCheckData> onEndSkillCheck, float space, float delay, float skillCheckTime, MonoActor listener)
        {
            if (started) return;
            started = true;
            active = true;
            this.delay = delay;

            var stopListening = AddListener(eSkillCheckEvent.OnEnd, onEndSkillCheck, listener);

            range = Mathf.Clamp(space, 0f, 1f);
            offSet = range * .5f;

            CreateSkillCheck();

            var currentTime = 0f;
            await ArchAction.WaitUntil((float deltaTime) => {
                currentTime += deltaTime;
                return currentTime >= delay;
            }, true);

            bool stopSkillCheck = false;

            Action<SkillCheckData> onHit = (SkillCheckData data) => {

                if(value < (angle + offSet) && value > (angle - offSet))
                {
                    success = true;
                }

                stopSkillCheck = true;
            };

            stopListening += AddListener(eSkillCheckEvent.OnHit, onHit, listener);

            currentTime = 0f;

            await ArchAction.WaitUntil((float deltaTime) =>
            {
                currentTime += deltaTime;
                var lerpValue = Mathf.Lerp(0f, 1f, currentTime / skillCheckTime);
                value = lerpValue;

                if (!stopSkillCheck)
                {
                    stopSkillCheck = value >= 1f;
                }

                return stopSkillCheck;
            }, true);




            active = false;
            if (success)
            {
                Invoke(eSkillCheckEvent.OnSuccess, this);
            }
            else
            {
                Invoke(eSkillCheckEvent.OnFail, this);
            }
            Invoke(eSkillCheckEvent.OnEnd, this);
            stopListening();
            

            void CreateSkillCheck()
            {
                if(range >= 1f)
                {
                    angle = .5f;
                    return;
                }

                angle = UnityEngine.Random.Range(offSet, 1f - offSet);
            }
        }

        public void HitSkillCheck()
        {
            Invoke(eSkillCheckEvent.OnHit, this);
        }

        public async Task WhileProgress(Action<SkillCheckData> action)
        {
            while (active)
            {
                action(this);
                await Task.Yield();
            }
        }

        public async Task UntilDone()
        {
            await ArchAction.WaitUntil(() => active, false);
        }
    }
    #endregion

    #region SkillCheckHandler

    public class SkillCheckHandler : MonoActor
    {
        #region Common Data

        [Header("Skill Check Properties")]
        [SerializeField] float intervals = 1f;
        [SerializeField] float timeWindow;
        [SerializeField] float startDelay;
        [SerializeField][Range(0f, 1f)] float defaultRange;
        [SerializeField][Range(0f, 1f)] float chance;


        [Header("Components")]
        [SerializeField] SkillCheckUI skillCheckUI;
        static PopupContainer popupContainer;
        #endregion

        #region Initialization
        void Start()
        {
            popupContainer = PopupContainer.active;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        #endregion
        public async void HandleSkillChecks(TaskEventData eventData)
        {
            var timer = intervals;

            while (eventData.task.BeingWorkedOn)
            {
                if(timer >= intervals)
                {
                    timer = 0f;

                    if (!eventData.task.BeingWorkedOn)
                    {
                        break;
                    }
                    if (!ArchGeneric.RollSuccess(chance * 100f))
                    {
                        continue;
                    }

                    var skillCheck = CreateSkillCheck(this, eventData.workInfo.transform, (SkillCheckData data) => {});

                    await skillCheck.UntilDone();
                    if (skillCheck.success) continue;

                    eventData.task.RemoveAllWorkers();

                    break;
                }
                else
                {
                    timer += World.deltaTime;
                }
                await Task.Yield();
            }
        }

        public static SkillCheckData CreateSkillCheck(SkillCheckHandler handler, Transform target, Action<SkillCheckData> onEndSkillCheck)
        {
            var skillCheckData = new SkillCheckData(handler);


            skillCheckData.StartSkillCheck(onEndSkillCheck, handler.defaultRange, handler.startDelay, handler.timeWindow, handler);
            CreateUI();

            return skillCheckData;

            void CreateUI()
            {
                var skillCheckUI = Instantiate(handler.skillCheckUI, popupContainer.transform);
                skillCheckUI.SetData(skillCheckData);

                if(target != null)
                {
                    skillCheckUI.SetTarget(target);
                }
            }
        }


    }
    #endregion
}