using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;

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

        #region Activation
        public override void Activate()
        {
            base.Activate();

            questInfo.OnObjectiveComplete += OnObjectiveComplete;
            requirement = (object o) => OtherObjectivesComplete();

            HandleEntityDeath();
            StartTimer();
            HandleOtherObjectives();

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

        #endregion
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


        public void AddTime(float time)
        {
            timer += time;
            UpdatePrompt();
        }
        //Cheat Commit because of LA fires.
        void HandleOtherObjectives()
        {
            var enemyForces = GetComponent<ObjectiveKillEnemyForces>();
            var killEntity = GetComponent<ObjectiveKillEntity>();

            if (enemyForces)
            {
                enemyForces.OnEntityDeathEvent += (CombatEvent eventData) => {
                    AddTimeFromEntity(eventData.target);
                };

            }
        }

        Dictionary<EntityRarity, float> entityRarityMultiplier = new()
        {
            {  EntityRarity.Boss, 4 },
            { EntityRarity.Elite, 3 },
            { EntityRarity.Rare, 3.5f },
            { EntityRarity.Common, 1 }
        };

        public void AddTimeFromEntity(EntityInfo entity, int baseTime = 1)
        {
            if (!entityRarityMultiplier.ContainsKey(entity.rarity)) return;
            AddTime(baseTime * entityRarityMultiplier[entity.rarity]);
        }

        public void HandleEntityDeath()
        {
            if (!enableMemberDeaths) return;
            var deathHandler = EntityDeathHandler.active;

            deathHandler.OnPlayableEntityDeath += (CombatEvent eventData) => { 
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
