using System.Collections;
using UnityEngine;

namespace Architome
{
    public class SkillCheckDifficultyHandler : MonoActor
    {
        [Header("Components")]
        [SerializeField] SkillCheckHandler handler;

        [Header("Properties")]
        [SerializeField][Range(0f, 100f)]float difficultyValue;

        #region Initiation

        private void Start()
        {
            ListenToHandler();
        }

        void ListenToHandler()
        {
            if (handler == null) return;

            handler.AddListener(eSkillCheckEvent.BeforeCreateSkillCheck, HandleBeforeStartSkillCheck, this);
        }

        #endregion

        #region SkillCheck Start Handler

        void HandleBeforeStartSkillCheck(SkillCheckData data)
        {

        }
        #endregion
    }
}