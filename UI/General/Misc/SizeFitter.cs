using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Architome
{
    public class SizeFitter : MonoBehaviour
    {
        #region Common Data
        public Transform target;
        [SerializeField] LayoutGroup layoutGroup;
        public List<Transform> targetWidths, targetHeights, targetHeightParents, targetWidthParents, ignoreTransforms;

        HashSet<Transform> ignoreTransformsHash;

        public bool manifestMaxX;
        public bool manifestMaxY;
        public bool ignoreInactives;

        public Vector2 offSet, min;
        public Vector2 max = new(float.PositiveInfinity, float.PositiveInfinity);
        public float offsetXPerItem;
        public float offsetYPerItem;

        public Action<GameObject> OnAdjustSize;

        [Header("Action Buttons")]
        [SerializeField] bool test;
        [SerializeField] bool updateOffsetsFromLayoutGroup;

        [Header("Results From Size Fitter")]
        [SerializeField] float currentX;
        [SerializeField] float currentY;
        #endregion

        #region Initialization

        private void OnValidate()
        {
            Initialize();

            if (test)
            {
                test = false;
                AdjustToSize();
            }

            HandleUpdateFromLayoutGroup();
        }

        private void Start()
        {
            Initialize();
            AdjustToSize();
        }

        void Initialize()
        {
            ignoreTransformsHash ??= ignoreTransforms.ToHashSet();

        }
        #endregion

        #region HandleUpdateFromLayoutGroup

        void HandleUpdateFromLayoutGroup()
        {
            if (!updateOffsetsFromLayoutGroup) return;
            updateOffsetsFromLayoutGroup = false;

            var padding = layoutGroup.padding;

            if (layoutGroup is VerticalLayoutGroup vertical)
            {
                offsetYPerItem = vertical.spacing;
            }

            if (layoutGroup is HorizontalLayoutGroup horizontal)
            {
                offsetXPerItem = horizontal.spacing;
            }


            offSet.x = padding.left + padding.right;
            offSet.y = padding.top + padding.bottom;

        }
        #endregion

        public async Task AdjustToSize(int iterations = 1, int timeBetween = 50)
        {
            for (int i = 0; i < iterations; i++)
            {
                AdjustToSize();
                await Task.Delay(timeBetween);
            }
        }

        public void AdjustToSize()
        {
            if (target == null) target = transform;
            var rectTransform = target.GetComponent<RectTransform>();


            var height = V3Helper.Height(targetHeights) + offSet.y;
            var width = V3Helper.Width(targetWidths) + offSet.x;

            Predicate<Transform> qualify = (trans) => true;

            if (ignoreTransformsHash != null)
            {
                qualify = (child) => !ignoreTransformsHash.Contains(child);
            }

            height += V3Helper.ChildrenHeight(targetHeightParents, offsetYPerItem, qualify);
            width += V3Helper.ChildrenWidth(targetWidthParents, offsetXPerItem, qualify);



            if (manifestMaxX)
            {
                width = MaxX + offSet.x;
            }

            if (manifestMaxY)
            {
                height = MaxY + offSet.y;
            }

            if (height < min.y) height = min.y;
            if (height > max.y) height = max.y;

            if (width < min.x) width = min.x;
            if (width > max.x) width = max.x;


            rectTransform.sizeDelta = new Vector2(width, height);

            currentX = width;
            currentY = height;

            OnAdjustSize?.Invoke(gameObject);
        }

        #region Properties
        float MaxX
        {
            get
            {
                var max = 0f;

                foreach (var trans in targetWidths)
                {
                    if (ignoreInactives && !trans.gameObject.activeInHierarchy) continue;
                    var rectTrans = trans.GetComponent<RectTransform>();

                    if (rectTrans.rect.width > max)
                    {
                        max = rectTrans.rect.width;
                    }
                }

                return max;
            }
        }

        float MaxY
        {
            get
            {
                var max = 0f;

                foreach (var trans in targetHeights)
                {
                    if (ignoreInactives && !trans.gameObject.activeInHierarchy) continue;
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
    #endregion
}
