using Architome.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SkillCheckUIFX : MonoActor
    {
        [Header("Components")]
        [SerializeField] SkillCheckUI skillCheckUI;
        [SerializeField] CanvasGroup canvasGroup;



        [Header("Properties")]
        [SerializeField] EffectsHandler<eSkillCheckEvent, EventItemHandler<eSkillCheckEvent>> effectsHandler;


        #region Initiation

        private void Start()
        {
            effectsHandler.InitiateItemEffects((eventItem) => {
                
            });

            effectsHandler.Activate(eSkillCheckEvent.OnStartBeforeDelay);

            skillCheckUI.skillCheckData.AddAllListeners((eSkillCheckEvent trigger, SkillCheckData data) => {
                effectsHandler.Activate(trigger);
            }, this);
        }

        #endregion


    }
}
