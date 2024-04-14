using System;
using System.Collections;
using UnityEngine;

namespace Architome.SkillCheck
{
    #region SkillCheckOffsets

    [Serializable]
    public class SkillCheckOffsets
    {
        public float rangeMultiplier = 1f;
        public float timeMultiplier = 1f;
        public float delayMultiplier = 1f;

        public void AdjustSettings(SkillCheckData data, float difficultyValue)
        {
            var newRange = data.range - data.range * (rangeMultiplier * difficultyValue);
            newRange = Mathf.Clamp(newRange, .025f, 1f);

            var newDelay = data.delay - data.delay * (delayMultiplier * difficultyValue);
            newDelay = Mathf.Clamp(newDelay, 0f, Mathf.Infinity);

            var newDuration = data.skillCheckTime - data.skillCheckTime * (timeMultiplier * difficultyValue);
            newDuration = Mathf.Clamp(newDuration, .25f, Mathf.Infinity);

            data.UpdateValues(newRange, newDelay, newDuration);
        }
    }
    #endregion

    public class SkillCheckDifficultyHandler : MonoActor
    {

        #region Common Data

        [SerializeField] SkillCheckOffsets currentOffsets;

        [Header("Components")]
        [SerializeField] SkillCheckHandler handler;

        [Header("Properties")]
        [SerializeField][Range(0f, 100f)]float difficultyValue;

        #endregion

        #region Initiation
        private void Start()
        {
            UpdateCurrentOffsets();
            ListenToHandler();
        }
        void ListenToHandler()
        {
            if (handler == null) return;
            handler.AddListener(eSkillCheckEvent.BeforeCreateSkillCheck, HandleBeforeStartSkillCheck, this);

        }

        void UpdateCurrentOffsets()
        {
            var difficultySettings = DifficultyModifications.active;
            currentOffsets = difficultySettings.settings.skillCheckOffsets;
        }

        #endregion

        #region SkillCheck Start Handler

        void HandleBeforeStartSkillCheck(SkillCheckData data)
        {
            var difficultyOffset = DifficultyOffsetFromEntity(data.skillCheckUser);
            currentOffsets.AdjustSettings(data, difficultyOffset);
        }

        public float DifficultyOffsetFromEntity(EntityInfo entity)
        {
            return difficultyValue;
        }
        #endregion
    }
}