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

        void Start()
        {
            GetDependencies();

        }
        void GetDependencies()
        {
            targetManager = ContainerTargetables.active;
        }

        
        public async void StartAbility()
        {
            if (!currentBehavior) return;
            if (activeAbility) return;
            var ability = currentBehavior.abilityInfo;
            if (ability == null) return;
            if (!ability.IsReady()) return;


            if(ability.abilityType == AbilityType.Use)
            {
                ability.Cast();
                return;
            }

            activeAbility = true;

            bool targetting = true;

            targetManager.OnSelectTarget += HandleSelectTarget;

            while (targetting)
            {
                await Task.Yield();
            }

            targetManager.OnSelectTarget -= HandleSelectTarget;

            activeAbility = false;

            void HandleSelectTarget(GameObject target)
            {
                targetting = false;
            }
        }
    }
}
