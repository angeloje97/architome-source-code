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
        public string sourceAlias;
        public object sourceData;

        [Serializable]
        public struct Rewards
        {
            public float experience;
            public ItemPool possibleItems;
            public List<ItemData> items;
        }

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
        public Rewards rewards;
        
        public List<Objective> questObjectives;

        public Action<Quest> OnActive { get; set; }
        public Action<Quest> OnCompleted { get; set; }
        public Action<Quest> OnQuestEnd { get; set; }
        public Action<Quest> OnQuestFail { get; set; }

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
            if (info.state != QuestState.Active) return;

            HandleParallel();
            HandleLinear();
            HandleRadio();
            HandleCompleted();
            HandleFail();

            OnObjectiveComplete?.Invoke(objective);



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

            Debugger.Environment(2945, $"{questName} Completed");

            OnCompleted?.Invoke(this);
            EndQuest();
        }
        public void ForceFail()
        {
            if (info.state == QuestState.Failed) return;
            info.state = QuestState.Failed;

            HandleFail();
        }
        public void ForceComplete()
        {
            if (info.state == QuestState.Completed) return;
            info.state = QuestState.Completed;
            HandleCompleted();
        }
        void HandleFail()
        {
            if (info.state != QuestState.Failed) return;

            OnQuestFail?.Invoke(this);
            EndQuest();

        }

        void EndQuest()
        {
            OnQuestEnd?.Invoke(this);
            questManager.OnQuestEnd?.Invoke(this);
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

        public override string ToString()
        {
            return questName;
        }
        public void SetSource(object sourceData, string alias)
        {
            this.sourceData = sourceData;
            sourceAlias = alias;
        }
    }
}
