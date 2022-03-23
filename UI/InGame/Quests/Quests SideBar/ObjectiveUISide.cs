using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Architome
{
    public class ObjectiveUISide : MonoBehaviour
    {
        // Start is called before the first frame update
        public TextMeshProUGUI objectiveDescription;

        public Image completionRing;
        public Image completionFill;
        public Image failureFill;

        public void SetObjective(Objective objective)
        {
            objectiveDescription.text = objective.prompt;
            objective.OnChange += OnChange;
            objective.OnComplete += OnComplete;
        }

        public void OnComplete(Objective objective)
        {
            completionFill.enabled = true;
        }

        public void OnChange(Objective objective)
        {
            objectiveDescription.text = objective.prompt;
        }
}

}