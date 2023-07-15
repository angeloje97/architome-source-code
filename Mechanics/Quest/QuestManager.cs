using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public enum QuestEvents 
    {
        OnNew,
        OnEnd,
        OnActive,
    }
    public class QuestManager : MonoBehaviour
    {
        public ArchEventHandler<QuestEvents, Quest> events;

        public static QuestManager active;
        public List<Quest> quests;


        private void Start()
        {
        }

        public void Awake()
        {
            active = this;
            events = new(this);
        }

        public Quest AddQuest(Quest questPrefab)
        {

            var newQuest = Instantiate(questPrefab, transform);

            UpdateQuest();

            events.InvokeEvent(QuestEvents.OnNew, newQuest);
            return newQuest;

        }

        public void DeleteQuest(Quest quest)
        {
            for (int i = 0; i < quests.Count; i++)
            {
                var current = quests[i];
                if (current != quest) continue;

                Destroy(current.gameObject);
                quests.RemoveAt(i);
                i--;
            }
        }

        void UpdateQuest()
        {
            quests = GetComponentsInChildren<Quest>().ToList();
        }

    }

}
