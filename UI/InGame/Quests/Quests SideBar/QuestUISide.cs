using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

namespace Architome
{
    public class QuestUISide : MonoBehaviour
    {
        
        public Quest quest;
        public GameObject objectivePrefab;
        public TextMeshProUGUI questTitle;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        

        public void SetQuest(Quest quest)
        {
            this.quest = quest;

            this.quest.OnObjectiveActivate += OnObjectiveActivate;

            questTitle.text = quest.questName;
            quest.OnCompleted += OnCompleted;

            var activeObjectives = quest.ActiveObjectives();

            foreach(var objective in activeObjectives)
            {
                CreateObjective(objective);
            }

        }

        public void OnCompleted(Quest quest)
        {
            questTitle.text +=  " (Completed)";
            foreach(var objective in GetComponentsInChildren<ObjectiveUISide>())
            {
                Destroy(objective.gameObject);
            }

            GetComponentInParent<ModuleListInfo>()?.UpdateModule();
        }


        public void OnObjectiveActivate(Objective objective)
        {
            CreateObjective(objective);
        }

        
        public void CreateObjective(Objective objective)
        {
            Instantiate(objectivePrefab, transform).GetComponent<ObjectiveUISide>().SetObjective(objective);
        }
    }

}