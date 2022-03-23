using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Architome
{
    public class Objective : MonoBehaviour
    {
        // Start is called before the first frame update
        public string prompt;

        public Quest questInfo;
        public bool isActive = false;
        public bool isComplete = false;

        public Action<Objective> OnActivate;
        public Action<Objective> OnChange;
        public Action<Objective> OnComplete;

        public void GetDependencies()
        {
            if(GetComponentInParent<Quest>())
            {
                questInfo = GetComponentInParent<Quest>();
            }
        }

        public void CompleteObjective()
        {
            isActive = false;
            isComplete = true;
            questInfo.CompleteObjective(this);
            OnComplete?.Invoke(this);
            
        }

        public void HandleObjectiveChange()
        {
            OnChange?.Invoke(this);
            questInfo.OnObjectiveChange?.Invoke(this);
        }

        public void Activate()
        {
            if (questInfo == null)
            {
                GetDependencies();
                if(questInfo == null) { return; }
            }

            isActive = true;
            OnActivate?.Invoke(this);
            questInfo.OnObjectiveActivate?.Invoke(this);
        }
    }
}

