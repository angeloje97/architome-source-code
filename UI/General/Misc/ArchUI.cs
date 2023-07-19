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

        public static async void SetCanvas(this CanvasGroup canvas, bool val, float totalTime = 0f)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;
            var startingAlpha = canvas.alpha;

            await ArchCurve.Smooth((float lerpValue) => {
                canvas.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerpValue);
            }, CurveType.EaseInOut, totalTime);


            canvas.alpha = targetAlpha;
            canvas.interactable = val;
            canvas.blocksRaycasts = val;
        }

        public static void SetCanvases(List<CanvasGroup> canvasGroups, bool val, float totalTime = 0f)
        {
            foreach (var canvas in canvasGroups)
            {
                SetCanvas(canvas, val, totalTime);
            }
        }

        public static async Task SetCanvasAsync(this CanvasGroup canvas, bool val, float totalTime)
        {
            if (canvas == null) return;

            var targetAlpha = val ? 1f : 0f;
            var startingAlpha = canvas.alpha;


            await ArchCurve.Smooth((float lerpValue) => {
                canvas.alpha = Mathf.Lerp(startingAlpha, targetAlpha, lerpValue);
            }, CurveType.EaseInOut, totalTime);

            canvas.interactable = val;
            canvas.blocksRaycasts = val;



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
