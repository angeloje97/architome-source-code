using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ObjectiveTimer : Objective
    {
        public float timer;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public override void Activate()
        {
            base.Activate();

            questInfo.OnObjectiveComplete += OnObjectiveComplete;
            StartTimer();
        }

        public void UpdatePrompt()
        {
            prompt = $"Complete objectives before timer runs out.\n{ArchString.FloatToTimer(timer)} left";

            if (isComplete)
            {
                prompt = $"Complete";
            }

            HandleObjectiveChange();
        }

        async void StartTimer()
        {
            while (timer > 0)
            {
                await Task.Delay(1000);
                if (!isActive) return;
                timer -= 1f;
                UpdatePrompt();
            }

            questInfo.ForceFail();
        }

        public void OnObjectiveComplete(Objective obj)
        {
            if (obj == this) return;


            foreach (var objective in questInfo.ActiveObjectives())
            {
                if (objective == this) continue;
                if (objective.isActive) return;
            }

            CompleteObjective();
        }
    }
}
