using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;
using Architome.Enums;
using UnityEngine.UI;
using System.Threading.Tasks;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToolTip : MonoBehaviour
    {

        [Serializable]
        public class Components
        {
            public Icon icon;
            public TextMeshProUGUI name, subHeadline, description, attributes, requirements, value;
            public List<Transform> manifestHeight, manifestWidth;
            public RectTransform rectTransform;
            public CanvasGroup canvasGroup;
        }

        [Serializable]
        public struct RarityInfo
        {
            public Rarity rarity;
            public Color backGroundColor, headerColor;
            public Sprite iconBackground;
        }

        public bool adjustToMouse;
        public Components components;
        public List<RarityInfo> rarityInfos;

        [Header("Adjustments")]
        public float widthOffset;
        public float heightOffSet;
        public float extraTime = 0f;
        bool canDestroySelf;
        bool destroyedSelf;

        void Start()
        {
            ToolTipRoutine();
        }

        async void ToolTipRoutine()
        {
            if (components.canvasGroup == null) return;

            var group = components.canvasGroup;

            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0;

            await Task.Delay(50);


            for (int i = 0; i < 3; i++)
            {

                await Task.Delay(50);

                AdjustSize();
            }

            AdjustToMouse();

            canDestroySelf = true;

            if (destroyedSelf) return;

            group.interactable = true;
            group.blocksRaycasts = false;
            group.alpha = 1;
        }

        async public void DestroySelf()
        {
            destroyedSelf = true;

            while (!canDestroySelf)
            {
                await Task.Yield();
            }

            Destroy(gameObject);
        }

        private void OnValidate()
        {
            components.rectTransform = GetComponent<RectTransform>();
            AdjustSize();
        }

        void HandleNullTexts()
        {
            foreach (var field in components.GetType().GetFields())
            {
                if (field.FieldType != typeof(TextMeshProUGUI)) continue;

                var tmpro = (TextMeshProUGUI)field.GetValue(components);

                if (tmpro.text != null && tmpro.text.Length > 0) continue;

                tmpro.gameObject.SetActive(false);
            }
        }

        public void SetToolTip(ToolTipData data)
        {
            components.name.text = data.name;
            components.subHeadline.text = data.subeHeadline;
            components.description.text = data.description;
            components.attributes.text = data.attributes;
            components.requirements.text = data.requirements;
            components.value.text = data.value;
            components.icon.SetIconImage(data.icon);

            HandleNullTexts();
        }


        public ToolTip Icon(Sprite sprite)
        {
            components.icon.SetIconImage(sprite);
            return this;
        }

        public ToolTip Name(string name)
        {
            components.name.text = name;
            return this;
        }

        public ToolTip Description(string description)
        {
            components.description.text = description;
            return this;
        }

        public void AdjustToMouse()
        {
            if (!adjustToMouse) return;

            AdjustToPosition(Input.mousePosition);
        }

        public void AdjustToPosition(Vector3 position)
        {

            float xAnchor = position.x > Screen.width / 2 ? 1.0625f : -.0625f;
            float yAnchor = position.y > Screen.height / 2 ? 1 : 0;

            GetComponent<RectTransform>().pivot = new Vector2(xAnchor, yAnchor);

            transform.position = position;
        }

        public void AdjustSize()
        {

            var height = V3Helper.Height(components.manifestHeight);
            var width = V3Helper.Width(components.manifestWidth);

            //var width = V3Helper.Width(new List<Transform>() {
            //    components.icon.transform,
            //    components.name.transform
            //});

            components.rectTransform.sizeDelta = new(width + widthOffset, height + heightOffSet);

            foreach (var sizeFitter in GetComponentsInChildren<ContentSizeFitter>())
            {
                sizeFitter.enabled = false;
                sizeFitter.enabled = true;
            }

            //AdjustToMouse();
        }
    }

    public struct ToolTipData
    {
        public string name, subeHeadline, description, attributes, requirements, value;
        public Sprite icon;
    }
}
