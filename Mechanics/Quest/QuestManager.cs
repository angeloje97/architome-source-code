using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Architome
{
    public class QuestManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public static QuestManager active;
        public List<Quest> quests;
        public Action<Quest> OnNewQuest;

        public Action<Quest> OnQuestCompleted;
        public Action<Quest> OnQuestActive;

        public Quest AddQuest(GameObject questObject)
        {
            if (!questObject.GetComponent<Quest>()) return null;

            var newQuest = Instantiate(questObject, transform).GetComponent<Quest>();

            UpdateQuest();


            OnNewQuest?.Invoke(newQuest);
            return newQuest;

        }

        void UpdateQuest()
        {
            quests = new List<Quest>(GetComponentsInChildren<Quest>());
        }

        public void Awake()
        {
            active = this;
        }


    }

}
