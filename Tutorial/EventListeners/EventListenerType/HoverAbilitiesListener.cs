using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Tutorial
{
    public class HoverAbilitiesListener : EventListener
    {
        [Header("Hover Abilities Properties")]
        public List<AbilityInfo> abilities;
        void Start()
        {
            HandleStart();
        }

        public override void StartEventListener()
        {
            base.StartEventListener();

            if (abilities == null || abilities.Count == 0)
            {
                CompleteEventListener();
                return;
            }

            int abilitiesToRead = 0;
            int abilitiesRead = 0;

            foreach (var ability in abilities)
            {
                abilitiesToRead++;
                ability.OnAcquireToolTip += OnAcquireToolTip;
            }

            void OnAcquireToolTip(AbilityInfo ability, ToolTipData data)
            {
                abilitiesRead++;
                ability.OnAcquireToolTip -= OnAcquireToolTip;

                if (abilitiesRead == abilitiesToRead)
                {
                    CompleteEventListener();
                }
            }
        }

        public override string Directions()
        {
            var result = new List<string>
            {
                base.Directions(),
                $"Get information from your abilities by hovering your mouse over the abilities on the action bar."
            };

            return ArchString.NextLineList(result);
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
