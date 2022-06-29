using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class QuestManager : MonoBehaviour
    {
        // Start is called before the first frame update

        public static QuestManager active;
        public List<Quest> quests;
        public Action<Quest> OnNewQuest;

        public Action<Quest> OnQuestEnd { get; set; }
        public Action<Quest> OnQuestActive { get; set; }

        private void Start()
        {
        }

        public void Awake()
        {
            active = this;
        }

        public Quest AddQuest(GameObject questObject)
        {
            if (!questObject.GetComponent<Quest>()) return null;

            var newQuest = Instantiate(questObject, transform).GetComponent<Quest>();

            UpdateQuest();


            OnNewQuest?.Invoke(newQuest);
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
