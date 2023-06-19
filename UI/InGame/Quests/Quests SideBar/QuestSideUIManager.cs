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
            questManager.OnQuestActive += OnQuestActive;
        }
        void Start()
        {
            GetDependencies();
            //UpdateCanvas();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void OnQuestActive(Quest quest)
        {
            CreateQuestUI(quest);
        }

        public void UpdateCanvas()
        {
            if (moduleInfo == null) return;

            moduleInfo.SetActive(GetComponentsInChildren<QuestUISide>().Length > 0);
        }

        public void CreateQuestUI(Quest quest)
        {
            if (questUIPrefab == null || questPrefabBin == null) { return; }

            var newQuest = Instantiate(questUIPrefab, questPrefabBin).GetComponent<QuestUISide>();

            newQuest.SetQuest(quest);

            //UpdateCanvas();
        }
    }
}