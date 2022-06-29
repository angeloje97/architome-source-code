using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SizeFitter : MonoBehaviour
    {
        public List<Transform> targetWidths, targetHeights;
        public Vector2 offSet, min;
        public Vector2 max = new(float.PositiveInfinity, float.PositiveInfinity);


        public Action<GameObject> OnAdjustSize;
        public void AdjustToSize()
        {
            var rectTransform = GetComponent<RectTransform>();

            var height = V3Helper.Height(targetHeights) + offSet.y;
            var width = V3Helper.Width(targetWidths) + offSet.x;

            if (height < min.y) height = min.y;
            if (height > max.y) height = max.y;

            if (width < min.x) width = min.x;
            if (width > max.x) width = max.x;


            rectTransform.sizeDelta = new Vector2(width, height);

            OnAdjustSize?.Invoke(gameObject);
        }
    }
}
