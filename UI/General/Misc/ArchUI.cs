using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public static class ArchUI
    {

        public static void SetCanvas(CanvasGroup canvas, bool val, float lerpValue = 1f)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1 : 0;

            UpdateCanvas();

            canvas.interactable = val;
            canvas.blocksRaycasts = val;


            async void UpdateCanvas()
            {
                while (canvas != null && canvas.alpha != targetAlpha)
                {
                    canvas.alpha = Mathf.Lerp(canvas.alpha, targetAlpha, lerpValue);
                    await Task.Yield();
                }
            }
        }

        public static async Task FixLayoutGroups(GameObject target, bool controlCanvas = false, float delay = 0f) // Needs multiple iterations for some reason
        {

            var canvasGroup = target.GetComponent<CanvasGroup>();

            if (canvasGroup && controlCanvas)
            {
                SetCanvas(canvasGroup, false);
            }

            await Task.Delay((int)(delay * 1000));

            var layoutGroups = target.GetComponentsInChildren<HorizontalOrVerticalLayoutGroup>();
            var contentFitters = target.GetComponentsInChildren<ContentSizeFitter>();

            var layoutGroup = target.GetComponent<LayoutGroup>();
            var contentSizeFitter = target.GetComponent<ContentSizeFitter>();




            
            if(layoutGroup) layoutGroup.enabled = false;
            if(contentSizeFitter) contentSizeFitter.enabled = false;

            for (int i = 0; i < 4; i++) //Toggling components asynchronously
            {
                foreach(var group in layoutGroups) { group.enabled = false; }
                foreach(var fitter in contentFitters) { fitter.enabled = false; }
                

                await Task.Delay(25);

                foreach (var group in layoutGroups) { group.enabled = true; }
                foreach (var fitter in contentFitters) { fitter.enabled = true; }
                
            }

            if(layoutGroup) layoutGroup.enabled = true;
            if(contentSizeFitter) contentSizeFitter.enabled = true;


            if (canvasGroup && controlCanvas)
            {
                SetCanvas(canvasGroup, true);
            }
        }
    }
}
