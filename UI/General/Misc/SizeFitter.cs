using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome
{
    public class SizeFitter : MonoBehaviour
    {
        public Transform target;
        public List<Transform> targetWidths, targetHeights;

        public bool manifestMaxX;
        public bool manifestMaxY;
        public bool ignoreInactives;

        public Vector2 offSet, min;
        public Vector2 max = new(float.PositiveInfinity, float.PositiveInfinity);

        public Action<GameObject> OnAdjustSize;

        [SerializeField] bool test;

        private void OnValidate()
        {
            if (!test) return;
            test = false;
            AdjustToSize();
        }


        public void AdjustToSize()
        {
            if (target == null) target = transform;
            var rectTransform = target.GetComponent<RectTransform>();

            var height = V3Helper.Height(targetHeights) + offSet.y;
            var width = V3Helper.Width(targetWidths) + offSet.x;

            if (manifestMaxX)
            {
                width = MaxX() + offSet.x;
            }

            if (manifestMaxY)
            {
                height = MaxY() + offSet.y;
            }

            if (height < min.y) height = min.y;
            if (height > max.y) height = max.y;

            if (width < min.x) width = min.x;
            if (width > max.x) width = max.x;


            rectTransform.sizeDelta = new Vector2(width, height);

            OnAdjustSize?.Invoke(gameObject);
        }

        float MaxX()
        {
            var max = 0f;

            foreach (var trans in targetWidths)
            {
                if (ignoreInactives && !trans.gameObject.activeSelf) continue;
                var rectTrans = trans.GetComponent<RectTransform>();

                if (rectTrans.rect.width > max)
                {
                    max = rectTrans.rect.width;
                }
            }

            return max;
        }

        float MaxY()
        {
            var max = 0f;

            foreach (var trans in targetHeights)
            {
                if (ignoreInactives && !trans.gameObject.activeSelf) continue;
                var rectTrans = trans.GetComponent<RectTransform>();

                if (rectTrans.rect.height > max)
                {
                    max = rectTrans.rect.height;
                }
            }

            return max;
        }

    }
}
