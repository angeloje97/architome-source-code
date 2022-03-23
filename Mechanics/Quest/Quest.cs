using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Architome.Enums;

namespace Architome
{
    public class Quest : MonoBehaviour
    {
        // Start is called before the first frame update
        private QuestManager questManager;

        public string questName;
        [Multiline]
        public string questDescription;
        public int questId;

        [Serializable]
        public struct QuestInfo
        {
            public QuestState state;
            public CompletionType completionType;
            public QuestType questType;
            public bool failed;
            public bool isActive;
        }

        public QuestInfo info;
        
        public List<Objective> questObjectives;

        public Action<Quest> OnActive;
        public Action<Quest> OnCompleted;
        public Action<Objective> OnObjectiveComplete;
        public Action<Objective> OnObjectiveActivate;
        public Action<Objective> OnObjectiveChange;

        public void GetDependencies()
        {
            questObjectives = new List<Objective>(GetComponentsInChildren<Objective>());
            questManager = GetComponentInParent<QuestManager>();
        }

        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public List<Objective> ActiveObjectives()
        {
            return questObjectives.Where(objective => objective.isActive == true).ToList();
        }

        public bool Activate()
        {
            if(questObjectives.Count == 0)
            {
                GetDependencies();

                if(questObjectives.Count == 0)
                {
                    return false;
                }
            }

            if(info.state != QuestState.Available) { return false; }
            

            info.isActive = true;
            info.state = QuestState.Active;

            if (info.completionType != CompletionType.Linear)
            {
                foreach(var objective in questObjectives)
                {
                    objective.Activate();
                }
            }
            else
            {
                questObjectives[0].Activate();
            }

            OnActive?.Invoke(this);
            questManager.OnQuestActive?.Invoke(this);

            return true;
        }

        public void CompleteObjective(Objective objective)
        {
            if (!questObjectives.Contains(objective)) return;

            OnObjectiveComplete?.Invoke(objective);

            HandleParallel();
            HandleLinear();
            HandleRadio();
            HandleCompleted();

            void HandleParallel()
            {
                if (info.completionType != CompletionType.Parallel) return;
                if(AllObjectivesComplete())
                {
                    info.state = QuestState.Completed;
                }
            }
            void HandleLinear()
            {
                if (info.completionType != CompletionType.Linear) return;
                if (AllObjectivesComplete())
                {
                    info.state = QuestState.Completed;
                    return;
                }

                var nextIndex = questObjectives.IndexOf(objective) + 1;

                questObjectives[nextIndex].Activate();
                
            }
            void HandleRadio()
            {
                if (info.completionType != CompletionType.Radio) return;
                info.state = QuestState.Completed;
            }   
        }

        void HandleCompleted()
        {
            if (info.state != QuestState.Completed) return;

            OnCompleted?.Invoke(this);
            questManager.OnQuestCompleted?.Invoke(this);
        }

        public void ForceComplete()
        {
            info.state = QuestState.Completed;
            HandleCompleted();
        }

        public bool AllObjectivesComplete()
        {
            foreach(var currentObjective in questObjectives)
            {
                if(!currentObjective.isComplete)
                {
                    return false;
                }
            }
            return true;
        }

    }
}
