using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Architome.Enums;

namespace Architome
{
    public class AbilityClickHandler : MonoBehaviour
    {
        [SerializeField] ActionBarBehavior currentBehavior;
        ContainerTargetables targetManager;
        [SerializeField] bool activeAbility;
        ActionBarsInfo actionBarsInfo;
        ArchInput archInput;




        void Start()
        {
            GetDependencies();

        }
        void GetDependencies()
        {
            targetManager = ContainerTargetables.active;

            actionBarsInfo = GetComponentInParent<ActionBarsInfo>();
            var pauseMenu = PauseMenu.active;
            archInput = ArchInput.active;

            if (archInput)
            {
                archInput.OnEscape += Cancel;
            }
            if (pauseMenu)
            {
                pauseMenu.OnTryOpenPause += HandleTryOpenPause;
            }

            if (actionBarsInfo)
            {
                actionBarsInfo.OnUseActionBar += HandleUseActionBar;
                actionBarsInfo.OnAlternateAction += HandleAlternativeAction;
            }
        }

        void HandleUseActionBar(ActionBarBehavior behavior)
        {
            Cancel();
        }

        void HandleAlternativeAction()
        {
            Cancel();
        }

        public void Cancel()
        {
            if (activeAbility)
            {
                activeAbility = false;
            }
        }

        void HandleTryOpenPause(PauseMenu menu)
        {
            if (activeAbility)
            {
                menu.pauseBlocked = true;
            }
        }
        
        public async void StartAbility()
        {
            if (!currentBehavior) return;
            if (activeAbility) return;
            if (actionBarsInfo.Busy()) return;
            var ability = currentBehavior.abilityInfo;
            if (ability == null) return;
            if (!ability.IsReady()) return;
            var abilityType = ability.abilityType;

            if(abilityType == AbilityType.Use)
            {
                _= ability.Cast();
                return;
            }

            activeAbility = true;


            actionBarsInfo.currentAbilityClickHandler = this;
            
            if(abilityType == AbilityType.LockOn)
            {
                targetManager.OnSelectTarget += HandleSelectTarget;
                targetManager.OnSelectNothing += Cancel;

            }

            if(abilityType == AbilityType.Spawn || abilityType == AbilityType.SkillShot)
            {
                

                archInput.OnSelect += HandleSelect;
                
            }

            while (activeAbility)
            {
                await Task.Yield();
            }

            if(abilityType == AbilityType.LockOn)
            {
                targetManager.OnSelectTarget -= HandleSelectTarget;
                targetManager.OnSelectNothing -= Cancel;
            }

            if (abilityType == AbilityType.Spawn || abilityType == AbilityType.SkillShot)
            {
                archInput.OnSelect -= HandleSelect;
            }

            if(actionBarsInfo.currentAbilityClickHandler == this)
            {
                actionBarsInfo.currentAbilityClickHandler = null;
            }

            activeAbility = false;

            void HandleSelectTarget(EntityInfo entityInfo)
            {

                ability.target = entityInfo;
                _= ability.Cast();
                activeAbility = false;
            }

            void HandleSelect()
            {
                var playerController = ability.entityInfo.PlayerController();

                var mouseLocation = playerController.RelativeMouseLocation();

                ability.location = mouseLocation;
                _= ability.Cast();
                activeAbility = false;
            }
        }
    }
}
