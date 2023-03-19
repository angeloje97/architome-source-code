using System.Collections;
using System.Collections.Generic;
using Architome.History;
using UnityEngine;
using UnityEngine.Rendering;

namespace Architome
{
    public class QuestAuthentication : Authentication
    {
        public LogicType authenticationLogic;
        public List<string> validQuests;
        
        List<bool> values;

        public override void OnAuthenticationStart()
        {
            base.OnAuthenticationStart();
            UpdateValues();

            OnStartAuthentication?.Invoke(Validated());
        }

        void UpdateValues()
        {
            values ??= new();

            var questHistory = QuestHistory.active;
            if (questHistory == null) return;

            foreach(var quest in validQuests)
            {
                values.Add(questHistory.IsComplete(quest));
            }
        }

        bool Validated()
        {
            return new ArchLogic(values).Valid(authenticationLogic);
        }
    }
}
