using System;
using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Architome.Enums;
using TMPro;

namespace Architome
{
    [RequireComponent(typeof(CanvasGroup))]
    public class ToolTip : MonoBehaviour
    {
        public bool update;
        public ToolTipManager manager;
        [Serializable]
        public class Components
        {
            public Icon icon;
            public TextMeshProUGUI name, subHeadline, type, description, attributes, requirements, value;
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
        public bool adjustToSide;
        
        public Components components;
        public List<RarityInfo> rarityInfos;

        [Header("Adjustments")]
        public float widthOffset;
        public float minWidth;
        public float heightOffSet;
        public float minHeight;
        public float extraTime = 0f;

        [SerializeField] float minXAnchor, maxXAnchor, minYAnchor, maxYAnchor;
        bool canDestroySelf;
        bool destroyedSelf;

        void GetDependencies()
        {
            manager = ToolTipManager.active;
        }

        void Start()
        {
            GetDependencies();
            PrepareFormatting();
        }

        async void PrepareFormatting()
        {
            if (components.canvasGroup == null) return;

            var group = components.canvasGroup;

            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0;

            await Task.Delay((int)(extraTime * 1000));

            await Task.Delay(50);


            for (int i = 0; i < 3; i++)
            {

                await Task.Delay(50);

                AdjustSize();
            }

            AdjustToMouse();
            AdjustToSide();


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
            if (this == null) return;
            if (gameObject == null) return;
            Destroy(gameObject);
        }

        private void OnValidate()
        {
            if (!update) return;
            update = false;
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
            components.type.text = data.type;
            components.description.text = data.description;
            components.attributes.text = data.attributes;
            components.requirements.text = data.requirements;
            components.value.text = data.value;

            components.icon.SetIcon(new() { sprite = data.icon });
            //components.icon.SetIconImage(data.icon);

            HandleNullTexts();
            HandleRarity();

            void HandleRarity()
            {
                if (!data.enableRarity) return;
                var world = World.active;
                if (world == null) return;

                var rarityProperties = world.RarityProperty(data.rarity);

                components.type.color = rarityProperties.color;
            }
        }


        public ToolTip Icon(Sprite sprite)
        {
            //components.icon.SetIconImage(sprite);
            components.icon.SetIcon(new() { sprite = sprite });
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

        public void AdjustToMouse(bool forceAdjust = false)
        {
            if (forceAdjust) adjustToMouse = true;
            if (!adjustToMouse) return;

            AdjustToPosition(Input.mousePosition);
        }

        public void AdjustToSide(bool forceAdjust = false)
        {
            if (forceAdjust) adjustToSide = true;
            if (!adjustToSide) return;
            if (manager == null) return;
            if (manager.sideToolBarPosition == null) return;

            AdjustToPosition(manager.sideToolBarPosition.position);
        }

        public void AdjustToPosition(Vector3 position)
        {

            float xAnchor = position.x > Screen.width / 2 ? maxXAnchor : minXAnchor;
            float yAnchor = position.y > Screen.height / 2 ? maxYAnchor : minYAnchor;

            GetComponent<RectTransform>().pivot = new Vector2(xAnchor, yAnchor);

            transform.position = position;
        }

        public void AdjustSize()
        {

            var height = V3Helper.Height(components.manifestHeight) + heightOffSet;
            var width = V3Helper.Width(components.manifestWidth) + widthOffset;

            if (height < minHeight)
            {
                height = minHeight;
            }

            if (width < minWidth)
            {
                width = minWidth;
            }

            components.rectTransform.sizeDelta = new(width, height);

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
        public string name, subeHeadline, type, description, attributes, requirements, value;
        public Sprite icon;
        public bool enableRarity;
        public Rarity rarity;
        
    }
}
