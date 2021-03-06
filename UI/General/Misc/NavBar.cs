using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Threading.Tasks;

namespace Architome
{
    public class NavBar : MonoBehaviour
    {
        public List<GameObject> items = new();
        public List<Toggle> toggles = new();

        public float smoothening = 1f;

        public int index;
        public bool update;

        private void Start()
        {
            toggles = GetComponentsInChildren<Toggle>().ToList();

            OnValueChange();
        }

        private void OnValidate()
        {
            if (!update) return;
            update = false;
            toggles = GetComponentsInChildren<Toggle>().ToList();
            UpdateFromIndex();
            OnValueChange();
        }

        public void UpdateFromIndex()
        {
            if (toggles == null) return;
            if (toggles.Count == 0) return;

            index = Mathf.Clamp(index, 0, toggles.Count - 1);

            toggles[index].isOn = true;
        }

        public void OnValueChange()
        {
            if (items == null) return;
            if (toggles == null) return;

            var lerpValue = smoothening != 0 ? 1 / smoothening : 1;
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == null) continue;
                if (toggles.Count <= i) continue;
                var toggle = toggles[i];
                var canvasGroup = items[i].GetComponent<CanvasGroup>();

                if (toggle.isOn) index = i;

                if (canvasGroup == null)
                {
                    canvasGroup = items[i].AddComponent<CanvasGroup>();
                }

                float targetAlpha = toggle.isOn ? 1 : 0;

                if (targetAlpha == canvasGroup.alpha) continue;


                canvasGroup.alpha = targetAlpha;

                canvasGroup.blocksRaycasts = toggle.isOn;
                canvasGroup.interactable = toggle.isOn;

            }
        }
    }
}
