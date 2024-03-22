using System;
using System.Collections;
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

    [Serializable]
    public class SkillCheckData
    {
        #region Common Data
        public eSkillCheckDifficulty difficulty;

        [SerializeField][Range(0f, 100f)] float angle;
        [SerializeField][Range(0f, 100f)] float range;
        [SerializeField] float skillCheckTime;
        [SerializeField] float delay;

        public float currentAngle { get; private set; }
        public float currentValue { get; private set; }
        public bool success { get; private set; }
        public float space => range;

        public float offSet => range * .5f;

        Action OnHitSkillCheck;

        #endregion

        public async void StartSkillCheck(Action<SkillCheckData> onEndSkillCheck)
        {
            CreateSkillCheck();

            var currentTime = 0f;
            await ArchAction.WaitUntil((float deltaTime) => {
                currentTime += deltaTime;
                return currentTime >= delay;
            }, true);

            bool stopSkillCheck = false;

            Action onHit = () => {

                if(currentValue < (currentAngle + offSet) && currentValue > (currentAngle - offSet))
                {
                    success = true;
                }

                stopSkillCheck = true;
            };

            OnHitSkillCheck += onHit;

            await ArchAction.WaitUntil((float deltaTime) =>
            {
                currentTime += deltaTime;
                var lerpValue = Mathf.Lerp(0f, 1f, currentTime / skillCheckTime);
                currentValue = Mathf.Lerp(0f, 100f, lerpValue);

                if (!stopSkillCheck)
                {
                    stopSkillCheck = currentValue > 100f;
                }

                return stopSkillCheck;
            }, true);

            OnHitSkillCheck -= onHit;

            onEndSkillCheck?.Invoke(this);
        }

        void CreateSkillCheck()
        {
            do
            {
                currentAngle = UnityEngine.Random.Range(0f, 100f);
            } while (currentAngle - offSet < 0f || currentAngle + offSet > 100f);
        }

        public void HitSkillCheck()
        {
            OnHitSkillCheck?.Invoke();
        }
    }

    public class SkillCheckHandler : MonoActor
    {
        [SerializeField] SkillCheckData skillCheckData;

        

        public void HandleSkillChecks(TaskEventData eventData)
        {

        }
    }
}