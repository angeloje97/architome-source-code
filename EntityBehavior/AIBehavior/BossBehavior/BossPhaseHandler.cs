using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class BossPhaseHandler : MonoBehaviour
    {

        [SerializeField] BossBehavior behavior;
        [SerializeField] EntitySpeech speech;

        void GetDependenices()
        {
            behavior = GetComponentInParent<BossBehavior>();
            behavior.OnPhase += OnPhase;

            var entityInfo = GetComponentInParent<EntityInfo>();

            if (entityInfo)
            {
                speech = entityInfo.Speech;
            }
        }
        void Start()
        {
            GetDependenices();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnPhase(BossBehavior.Phase phase)
        {
            HandleAbility();
            HandleSpeech();

            if (phase.refillsMana)
            {
                behavior.entityInfo.GainResource(behavior.entityInfo.maxMana);
            }
            void HandleSpeech()
            {
                if (!speech) return;

                speech.Yell(phase.activationPhrase);
            }

            void HandleAbility()
            {
                if (phase.phaseAbility.abilityIndex == -1) return;

                behavior.combatBehavior.specialAbilities.Insert(0, phase.phaseAbility);
            }
        }
    }

}