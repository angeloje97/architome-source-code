using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Threading.Tasks;

namespace Architome
{
    public class BossBehavior : EntityProp
    {
        // Start is called before the first frame update
        public BossRoom bossRoom;

        public CombatBehavior combatBehavior;
        public Transform originalPosition { get; private set; }

        public List<Phase> phases;

        public Action<Phase> OnPhase;

        Queue<Func<Task>> tasks;
        bool tasksActive;


        public override async Task GetDependencies(Func<Task> extension)
        {
            await base.GetDependencies(async () => {

                BossRoom();

                if (bossRoom)
                {
                    originalPosition = bossRoom.bossPosition;
                }


                if (entityInfo)
                {
                    entityInfo.OnHealthChange += OnHealthChange;
                    entityInfo.OnCombatChange += OnCombatChange;

                    combatBehavior = transform.parent.GetComponentInChildren<CombatBehavior>();
                }

                await extension();
            });

            
        }

        private void Awake()
        {
            tasks = new();
        }

        void Update()
        {

        }

        public async void AddTask(Func<Task> task, bool startImmediately = false)
        {
            if (startImmediately) {
                tasks = new();
                tasksActive = false;
            }

            tasks.Enqueue(task);

            if (tasksActive) return;
            tasksActive = true;

            while(tasks.Count > 0)
            {
                var current = tasks.Dequeue();
                await current();
            }

            tasksActive = false;
        }

        public BossRoom BossRoom()
        {
            if (bossRoom == null)
            {
                var room = Entity.Room(transform.position);
                if (room == null) return null;
                if (room.GetType() == typeof(BossRoom))
                {
                    bossRoom = (BossRoom)room;
                }
            }

            return bossRoom;

        }

        void OnCombatChange(bool isInCombat)
        {
            if (isInCombat)
            {

            }
            else
            {
                RevertPhases();
            }
        }

        void RevertPhases()
        {
            foreach (var phase in phases)
            {
                phase.activated = false;

                if (combatBehavior.specialAbilities.Contains(phase.phaseAbility))
                {
                    combatBehavior.specialAbilities.Remove(phase.phaseAbility);
                }
            }
        }

        void OnHealthChange(float health, float shield, float maxHealth)
        {
            if (!entityInfo.isInCombat) return;
            foreach (var phase in phases)
            {
                if (phase.Activated(health / maxHealth))
                {
                    OnPhase?.Invoke(phase);
                }
            }
        }

        [Serializable]
        public class Phase
        {
            public bool activated, refillsMana;

            [Range(0, 1)]
            public float threshHold;
            [Multiline]
            public string activationPhrase;
            public SpecialAbility phaseAbility;
            [Header("Boss Task Properties")]
            public bool usesBossStation;
            public int stationIndex;
            public int taskIndex;

            public bool Activated(float percentHealth)
            {
                if (activated) return false;

                if (threshHold > percentHealth)
                {
                    activated = true;
                    return true;
                }

                return false;
            }
        }
    }

    
}