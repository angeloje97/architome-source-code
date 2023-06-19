using Language.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Architome.Enums;
using UnityEngine;

namespace Architome
{
    public class QuestGiver : MonoBehaviour
    {
        [Header("Quest Info")]
        public List<Quest> questPrefabs;
        public List<Quest> instantiatedQuests;


        public Action<QuestGiver, Quest> beforeActivateQuest;

        private void Start()
        {
            InstantiateQuests();
        }

        void InstantiateQuests()
        {
            instantiatedQuests = new();
            var manager = QuestManager.active;
            foreach(var quest in questPrefabs)
            {
                instantiatedQuests.Add(manager.AddQuest(quest));
            }
        }

        public void StartQuest(int index)
        {
            if (index < 0 || index >= instantiatedQuests.Count) return;
            var quest = instantiatedQuests[index];

            if (quest.info.state != QuestState.Available) return;

            quest.Activate();
        }

        public List<Quest> AvailableQuests()
        {
            return instantiatedQuests.FindAll(quest => quest.info.state == QuestState.Available);
        }
    }
}
