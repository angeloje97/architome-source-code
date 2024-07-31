using System;
using System.Linq;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using static Architome.Augment.AugmentEventData;

namespace Architome
{
    public class AugmentType : MonoActor
    {
        #region Common Data
        [Header("AugmentType Properties")]
        public Augment augment;
        protected AbilityInfo ability => augment.ability;

        public float value;
        public float valueContribution = 1f;

        public CatalystInfo activeCatalyst;
        protected CatalystHit activeHit;

        [SerializeField] protected bool ignoreDescription;


        public CombatEvents combatEvents => augment.entity.combatEvents;

        #endregion

        #region Initialization
        protected virtual void Start()
        {
            InitiateAugmentType();
        }
        async void InitiateAugmentType()
        {
            augment = GetComponent<Augment>();


            var successful = await augment.UntilDependenciesAcquired();
            if (!successful) return;

            if (augment)
            {
                value = valueContribution * augment.info.value;
            }

            if (augment != null && augment.ability != null)
            {
                GetDependencies();
            }
        }
        protected virtual void GetDependencies()
        {
            
        }

        #region Startup Actions
        protected void EnableCatalyst()
        {
            augment.OnNewCatalyst += HandleNewCatlyst;

        }
        protected void EnableAugmentAbility()
        {

            ability.OnActivateAugmentAbilites += HandleActivateAugmentAbilities;


            augment.OnRemove += (Augment augment) =>
            {
                ability.OnActivateAugmentAbilites -= HandleActivateAugmentAbilities;
            };

            void HandleActivateAugmentAbilities(AbilityInfo ability, List<Func<Task<bool>>> abilities)
            {
                abilities.Add(Ability);
            }
        }

        protected void AllowInterruptable()
        {
            var interruptableStates = new List<EntityState>() {
                EntityState.Silenced,
                EntityState.Stunned,
            };

            Action<StateChangeEvent> action = (eventData) => {

                if (eventData.afterEventStates.Intersect(interruptableStates).ToList().Count > 0)
                {
                    HandleCancelAbility(this);
                }
            };

            combatEvents.AddListenerStateEvent(eStateEvent.OnStatesChange, action, this);
        }
        protected void EnableCasting()
        {
            augment.ability.WhileCasting += WhileCasting;
            augment.OnRemove += (Augment augment) => {
                ability.WhileCasting -= WhileCasting;
            };
        }

        protected void EnableCombatChange()
        {
            var entity = augment.ability.entityInfo;

            entity.OnCombatChange += HandleCombatChange;

            augment.OnRemove += (Augment augment) => {
                entity.OnCombatChange -= HandleCombatChange;
            };
        }

        protected void EnableStartCast()
        {
            ability.OnCastStart += HandleCastStart;
            augment.OnRemove += (augment) => {
                ability.OnCastStart -= HandleCastStart;
            };

        }

        protected void EnablePlayableParty()
        {
            var gameManager = GameManager.active;

            if (gameManager)
            {
                gameManager.OnNewPlayableParty += HandleNewPlayableParty;
            }

            augment.OnRemove += delegate (Augment augment) {
                gameManager.OnNewPlayableParty -= HandleNewPlayableParty;
            };

        }

        protected void EnableAugmentTrigger()
        {
            var abilityManager = augment.abilityManager;
            if (abilityManager == null) return;

            abilityManager.events.OnTriggerAugment += HandleAugmentTrigger;

            augment.OnRemove += (Augment augment) => {
                abilityManager.events.OnTriggerAugment -= HandleAugmentTrigger;
            };
        }

        protected void EnableSuccesfulCast()
        {
            ability.OnSuccessfulCast += HandleSuccessfulCast;

            augment.OnRemove += (Augment augment) =>
            {
                ability.OnSuccessfulCast -= HandleSuccessfulCast;
            };
        }
        protected void EnableAbilityStartEnd()
        {
            ability.OnAbilityStartEnd += HandleAbility;

            augment.OnRemove += (Augment augment) => { ability.OnAbilityStartEnd -= HandleAbility; };
        }

        protected void EnableAbilityChanneling()
        {

            var abilityManager = augment.abilityManager;
            abilityManager.OnChannelStart += MiddleWare;

            augment.OnRemove += (augment) => {
                abilityManager.OnChannelStart -= MiddleWare;
            };

            void MiddleWare(AbilityInfo ability, AugmentChannel channelAugment)
            {
                if (ability != this.ability) return;
                HandleChannelStart(ability, channelAugment);
            }
        }

        public void EnableAbilityBeforeCast(Action<AbilityInfo, List<Func<Task>>> action)
        {
            bool removedListener = false;
            ability.BeforeCastStart += MiddleWare;
            augment.OnRemove += (augment) => {
                RemoveListener();
            };

            void RemoveListener()
            {
                if (removedListener) return;
                removedListener = true;

                ability.BeforeCastStart -= MiddleWare;
            }

            void MiddleWare(AbilityInfo ability, List<Func<Task>> tasks)
            {
                if (this == null) RemoveListener();
                action(ability, tasks);
            }
            
        }


        protected void EnableTasks()
        {
            augment.entity.taskEvents.OnTaskComplete += HandleTaskComplete;
            augment.entity.taskEvents.OnStartTask += HandleStartTask;

            augment.OnRemove += (Augment augment) => {
                augment.entity.taskEvents.OnTaskComplete -= HandleTaskComplete;
                augment.entity.taskEvents.OnStartTask -= HandleStartTask;
            };

        }



        #endregion

        #endregion

        #region Live Functions
        public virtual async Task<bool> Ability()
        {
            return true;
        }

        public virtual void SetCatalyst(CatalystInfo catalyst, bool active)
        {
            if (active)
            {
                activeCatalyst = catalyst;
                activeHit = catalyst.GetComponent<CatalystHit>();

            }
            else
            {
                if (activeCatalyst == catalyst)
                {
                    activeCatalyst = null;
                    activeHit = null;
                }
            }
        }
        public virtual void HandleNewCatlyst(CatalystInfo catalyst)
        {

        }

        public virtual void HandleAugmentTrigger(Augment augment) { }

        public virtual void HandleCancelAbility(AugmentType augment)
        {

        }

        protected virtual void HandleChannelStart(AbilityInfo ability, AugmentChannel channel) { }

        public virtual void HandleNewPlayableParty(PartyInfo party, int index)
        {

        }

        protected virtual void HandleCastStart(AbilityInfo ability) { }

        protected virtual void HandleCombatChange(bool isInCombat) { }

        public virtual void WhileCasting(AbilityInfo ability)
        {

        }
        public virtual void HandleSuccessfulCast(AbilityInfo ability)
        {

        }
        public virtual void HandleAbility(AbilityInfo ability, bool start)
        {

        }

        public virtual void HandleTaskComplete(TaskEventData eventData)
        {

        }

        public virtual void HandleStartTask(TaskEventData eventData)
        {

        }
        public void TriggerAugment()
        {

        }

        #endregion

        #region Dynamic Properties
        protected virtual string Description()
        {
            var result = "";
            return result;
        }

        public virtual bool CanAttachToAbility(Augment.AugmentEventData eventData)
        {
            return true;
        }
        public override string ToString()
        {
            if (ignoreDescription) return "";
            return Description();

        }
        #endregion


    }
}
