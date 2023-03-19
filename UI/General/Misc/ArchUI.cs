using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public static class ArchUI
    {

        public static void SetCanvas(CanvasGroup canvas, bool val, float lerpValue = 1f)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;

            UpdateCanvas();

            canvas.interactable = val;
            canvas.blocksRaycasts = val;



            async void UpdateCanvas()
            {
                if (lerpValue < 1)
                {
                    while (canvas != null && canvas.alpha != targetAlpha)
                    {
                        canvas.alpha = Mathf.Lerp(canvas.alpha, targetAlpha, lerpValue);
                        if (Mathf.Abs(canvas.alpha - targetAlpha) < .03125) canvas.alpha = targetAlpha;
                        await Task.Yield();
                    }
                }
                else
                {
                    canvas.alpha = targetAlpha;
                }
            }
        }

        public static void SetCanvases(List<CanvasGroup> canvasGroups, bool val, float lerpValue = 1f)
        {
            foreach (var canvas in canvasGroups)
            {
                SetCanvas(canvas, val, lerpValue);
            }
        }

        public static async Task SetCanvasAsync(CanvasGroup canvas, bool val, float lerpValue = 1f)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;

            await UpdateCanvas();

            canvas.interactable = val;
            canvas.blocksRaycasts = val;



            async Task UpdateCanvas()
            {
                if (lerpValue < 1)
                {
                    while (canvas != null && canvas.alpha != targetAlpha)
                    {
                        canvas.alpha = Mathf.Lerp(canvas.alpha, targetAlpha, lerpValue);
                        var difference = Mathf.Abs(canvas.alpha - targetAlpha);

                        if(difference <= .01)
                        {
                            canvas.alpha = targetAlpha;
                        }
                        await Task.Yield();
                    }
                }
                else
                {
                    canvas.alpha = targetAlpha;
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

        public static void SetText(TextMeshProUGUI tmp, string str, SpriteAssetType type)
        {
            var spriteAsset = SpriteAssetData.active.SpriteAsset(type);
            tmp.spriteAsset = spriteAsset;
            tmp.text = str;
        }

    }
}
