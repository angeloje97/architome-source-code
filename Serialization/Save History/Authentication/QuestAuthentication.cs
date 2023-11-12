using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Architome.Enums;
using Architome.History;
using UnityEngine;
using UnityEngine.Rendering;

namespace Architome
{
    public class QuestAuthentication : Authentication
    {
        public List<string> validQuests;
        
        Dictionary<string, bool> values;
        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            HandleQuestManager();

            Validated(fromStart: true);
        }

        void HandleQuestManager()
        {
            var questManager = QuestManager.active;

            questManager.AddListener(QuestEvents.OnEnd, (Quest quest) => {
                if (quest.info.state != QuestState.Completed) return;
                if (!values.ContainsKey(quest.ToString())) return;
                values[quest.ToString()] = true;

                var validated = authenticated;
                Validated();

            }, this);
        }

        protected override void UpdateValues()
        {
            values ??= new();

            var questHistory = QuestHistory.active;
            if (questHistory == null) return;

            foreach(var quest in validQuests)
            {
                values.Add(quest ,questHistory.IsComplete(quest));
            }

            ValidDictionary(values);
        }

        public override AuthenticationDetails Details()
        {
            var details = new AuthenticationDetails();
            var questHistory = QuestHistory.active;

            if (questHistory == null) return details;
            foreach(var name in validQuests)
            {
                var list = questHistory.IsComplete(name) ? details.validValues : details.invalidValues;
                list.Add(name);

            }
            return details;

        }
    }
}
