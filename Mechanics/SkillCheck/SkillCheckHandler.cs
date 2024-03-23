using System;
using System.Collections;
using UnityEditor.ShaderGraph.Internal;
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

    public enum eSkillCheckDataEvent
    {
        OnHitSkillCheck,
        OnEndSkillCheck,
        OnStartSkillCheck,
        OnStartBeforeDelay,
    }

    [Serializable]
    public class SkillCheckData
    {
        #region Common Data
        public float angle { get; private set; }
        public float value { get; private set; }
        public bool success { get; private set; }
        public float range { get; set; }
        public float offSet => range * .5f;

        Action OnHitSkillCheck;

        public Action<SkillCheckData> OnEndSkillCheck { get; set; }



        #endregion

        bool started;

        public async void StartSkillCheck(Action<SkillCheckData> onEndSkillCheck, float space, float delay, float skillCheckTime)
        {
            if (started) return;
            started = true;

            OnEndSkillCheck += (SkillCheckData data) => {
                onEndSkillCheck?.Invoke(data);
            };

            range = Mathf.Clamp(space, 0f, 100f);

            CreateSkillCheck();

            var currentTime = 0f;
            await ArchAction.WaitUntil((float deltaTime) => {
                currentTime += deltaTime;
                return currentTime >= delay;
            }, true);

            bool stopSkillCheck = false;

            Action onHit = () => {

                if(value < (angle + offSet) && value > (angle - offSet))
                {
                    success = true;
                }

                stopSkillCheck = true;
            };

            OnHitSkillCheck += onHit;

            currentTime = 0f;

            await ArchAction.WaitUntil((float deltaTime) =>
            {
                currentTime += deltaTime;
                var lerpValue = Mathf.Lerp(0f, 1f, currentTime / skillCheckTime);
                value = Mathf.Lerp(0f, 100f, lerpValue);

                if (!stopSkillCheck)
                {
                    stopSkillCheck = value > 100f;
                }

                return stopSkillCheck;
            }, true);

            OnHitSkillCheck -= onHit;

            OnEndSkillCheck?.Invoke(this);

            void CreateSkillCheck()
            {
                if(range >= 80f)
                {
                    angle = 50f;
                    return;
                }

                do
                {
                    angle = UnityEngine.Random.Range(0f, 100f);
                } while (angle - offSet < 0f || angle + offSet > 100f);
            }
        }

        public void HitSkillCheck()
        {
            OnHitSkillCheck?.Invoke();
        }
    }

    public class SkillCheckHandler : MonoActor
    {
        [Header("Skill Check Properties")]
        [SerializeField] float intervals = 1f;
        [SerializeField] float timeWindow;
        [SerializeField] float startDelay;

        [SerializeField][Range(0f, 100f)] float defaultRange;
        [SerializeField][Range(0f, 1f)] float chance;

        public async void HandleSkillChecks(TaskEventData eventData)
        {
            await ArchAction.WaitUntil(() => {
                if(!ArchGeneric.RollSuccess(chance * 100f))
                {
                    return eventData.task.BeingWorkedOn;
                }

                CreateSkillCheck(this, (SkillCheckData data) => {
                    if (!data.success)
                    {
                        eventData.task.RemoveAllWorkers();
                    }
                });

                return eventData.task.BeingWorkedOn;
            }, false, intervals);
        }

        public static SkillCheckData CreateSkillCheck(SkillCheckHandler handler, Action<SkillCheckData> onEndSkillCheck)
        {
            var skillCheckData = new SkillCheckData();

            skillCheckData.StartSkillCheck(onEndSkillCheck, handler.defaultRange, handler.startDelay, handler.timeWindow);

            return skillCheckData;
        }
    }
}