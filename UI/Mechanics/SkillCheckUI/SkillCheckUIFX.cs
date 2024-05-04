using Architome.Effects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome.SkillCheck
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

            skillCheckUI.OnHandleBackgroundProcesses += HandleBackgroundProcesses; 
        }

        void HandleBackgroundProcesses(List<Func<Task>> processes)
        {
            processes.Add(effectsHandler.UntilAudioManagerDone);
        }

        #endregion


    }
}
