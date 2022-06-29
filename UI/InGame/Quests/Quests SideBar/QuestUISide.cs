using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class QuestUISide : MonoBehaviour
    {
        
        [Header("Components")]
        public TextMeshProUGUI questTitle;
        public CanvasGroup canvasGroup;
        public Animator animator;

        [Header("Properties")]
        public Quest quest;
        
        [Header("Prefabs")]
        public GameObject objectivePrefab;

        bool transitioning;


        // Start is called before the first frame update
        async void Start()
        {
            ArchUI.SetCanvas(canvasGroup, false);

            for (int i = 0; i < 2; i++)
            {
                await ArchUI.FixLayoutGroups(gameObject, false, .35f);
            }

            animator.SetBool("Active", true);
        }


        // Update is called once per frame
        void Update()
        {

        }

        private void OnValidate()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }


        public void SetQuest(Quest quest)
        {
            this.quest = quest;

            this.quest.OnObjectiveActivate += OnObjectiveActivate;

            questTitle.text = quest.questName;
            quest.OnCompleted += OnCompleted;
            quest.OnQuestFail += OnFail;

            var activeObjectives = quest.ActiveObjectives();

            foreach(var objective in activeObjectives)
            {
                CreateObjective(objective);
            }

            
        }



        public void OnCompleted(Quest quest)
        {
            questTitle.text +=  " (Completed)";
            ClearObjectives();
            FadeAway();
        }

        public void OnFail(Quest quest)
        {
            questTitle.text += " (Failed)";
            ClearObjectives();
            FadeAway();
        }


        async void FadeAway()
        {
            
            await Task.Delay(1000);

            transitioning = true;

            animator.SetBool("Active", false);

            while (transitioning)
            {
                await Task.Yield();
            }

            Destroy(gameObject);
        }

        void ClearObjectives()
        {
            foreach (var objective in GetComponentsInChildren<ObjectiveUISide>())
            {
                Destroy(objective.gameObject);
            }
            GetComponentInParent<ModuleListInfo>()?.UpdateModule();
        }


        public void OnObjectiveActivate(Objective objective)
        {
            CreateObjective(objective);
        }

        
        public void EndTransition()
        {
            transitioning = false;
        }

        public void CreateObjective(Objective objective)
        {
            Instantiate(objectivePrefab, transform).GetComponent<ObjectiveUISide>().SetObjective(objective);
        }
    }

}