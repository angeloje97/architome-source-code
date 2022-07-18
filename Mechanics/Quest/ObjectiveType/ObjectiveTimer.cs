using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class ObjectiveTimer : Objective
    {
        public float timer, deathTimerPenalty;
        public int entityDeaths;
        public bool enableMemberDeaths;
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
            requirement = (object o) => OtherObjectivesComplete();

            HandleEntityDeath();
            StartTimer();

        }

        public void UpdatePrompt()
        {
            prompt = $"Complete objectives before timer runs out.\n{ArchString.FloatToTimer(timer)} left";

            var deadCount = DeadCount();
            if (deadCount.Length > 0)
            {
                prompt += $"\n{deadCount}";
            }

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

        public void HandleEntityDeath()
        {
            if (!enableMemberDeaths) return;
            var deathHandler = EntityDeathHandler.active;

            deathHandler.OnPlayableEntityDeath += (CombatEventData eventData) => { 
                entityDeaths++;
                timer -= deathTimerPenalty;
                Debugger.Environment(1095, $"Timer: {timer} after taking off {deathTimerPenalty}");
                UpdatePrompt();
            };
        }

        public string DeadCount()
        {
            var result = "";

            if (entityDeaths <= 0)
            {
                return result;
            }

            result += $"Deaths: {entityDeaths} (-{ArchString.FloatToTimer(entityDeaths*deathTimerPenalty)})";

            return result;
        }

        public void OnObjectiveComplete(Objective obj)
        {
            if (obj == this) return;
            HandleObjectiveChange();
        }

        public bool OtherObjectivesComplete()
        {
            foreach (var objective in questInfo.ActiveObjectives())
            {
                if (objective == this) continue;
                if (!objective.isComplete) return false;
            }
            return true;
        }
        
    }
}
