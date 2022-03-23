using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class QuestSideUIManager : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject questUIPrefab;
        public QuestManager questManager;
        public ModuleListInfo moduleInfo;
        public Transform questPrefabBin;

        void GetDependencies()
        {
            moduleInfo = GetComponentInParent<ModuleListInfo>();
            questManager = QuestManager.active;
            questManager.OnNewQuest += OnNewQuest;
            questManager.OnQuestActive += OnQuestActive;
        }
        void Start()
        {
            GetDependencies();

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnQuestActive(Quest quest)
        {
            CreateQuestUI(quest);
        }

        public void CreateQuestUI(Quest quest)
        {
            if (questUIPrefab == null || questPrefabBin == null) { return; }

            var newQuest = Instantiate(questUIPrefab, questPrefabBin);

            newQuest.GetComponent<QuestUISide>()?.SetQuest(quest);

            var cGroup = newQuest.GetComponent<CanvasGroup>();

            cGroup.alpha = 0;

            ArchAction.Delay(() => {
                moduleInfo.UpdateModule();
            }, .0625f);

            ArchAction.Delay(() =>
            {
                cGroup.alpha = 1;
            }, .50f);
        }

        public void OnNewQuest(Quest quest)
        {
            
        }
    }
}