using Language.Lua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome
{
    public class QuestGiver : MonoBehaviour
    {
        [Header("Quest Info")]
        public Quest questPrefab;

        [Header("State")]
        bool started;

        public Action<QuestGiver, Quest> beforeActivateQuest;

        public void StartQuest()
        {
            if (started) return;
            var questManager = QuestManager.active;
            if (!questManager) return;


            var createdQuest = questManager.AddQuest(questPrefab);
            beforeActivateQuest?.Invoke(this, createdQuest);

            createdQuest.Activate();
            started = true;

        }
    }
}
