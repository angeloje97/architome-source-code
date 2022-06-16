using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

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
                while (canvas.alpha != targetAlpha)
                {
                    canvas.alpha = Mathf.Lerp(canvas.alpha, targetAlpha, lerpValue);
                    await Task.Yield();
                }
            }
        }
    }
}
