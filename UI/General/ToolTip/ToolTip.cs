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
        public ToolTipManager manager;
        [Serializable]
        public class Components
        {
            public Icon icon;
            public TextMeshProUGUI name, subHeadline, type, description, attributes, requirements, value;
            public CanvasGroup canvasGroup;
            public RectTransform rectTransform;
            public SizeFitter sizeFitter;
        }

        public HashSet<string> rarityEffects
        {
            get
            {
                return new()
                {
                    "name",
                    "type",
                };
            }
        }

        [Serializable]
        public struct RarityInfo
        {
            public Sprite iconBackground;
            public List<Image> rarityImageTarget;
            public List<TextMeshProUGUI> textTargets;
        }

        public bool adjustToMouse;
        public bool adjustToSide;
        public bool followMouse;

        public bool formatting;
        public bool visible;
        
        public Components components;
        public RarityInfo rarityInfo;

        

        [SerializeField] float minXAnchor, maxXAnchor, minYAnchor, maxYAnchor;
        [SerializeField] int adjustSizeIterations = 3;
        bool canDestroySelf { get; set; }
        bool destroyedSelf;

        void GetDependencies()
        {
            manager = ToolTipManager.active;
        }

        void Start()
        {
            GetDependencies();
            //PrepareFormatting();
        }

        async void PrepareFormatting()
        {
            if (components.canvasGroup == null) return;

            var group = components.canvasGroup;
            formatting = true;

            group.interactable = false;
            group.blocksRaycasts = false;
            group.alpha = 0;



            //await Task.Delay(50);
            var sizeFitters = GetComponentsInChildren<ContentSizeFitter>();
            AdjustSize(sizeFitters);

            bool halfWay = false;
            for (int i = 0; i < adjustSizeIterations; i++)
            {

                await Task.Yield();

                if (halfWay) continue;

                if (i == adjustSizeIterations / 2)
                {
                    halfWay = true;
                    AdjustSize(sizeFitters);
                }


            }
            AdjustSize(sizeFitters);

            AdjustToMouse();
            AdjustToSide();
            FollowMouse();


            canDestroySelf = true;

            if (destroyedSelf) return;

            if (visible)
            {
                group.interactable = true;
                group.blocksRaycasts = false;
                group.alpha = 1;

            }

            formatting = false;
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
            visible = true;

            components.icon.SetIcon(new() { sprite = data.icon });
            //components.icon.SetIconImage(data.icon);

            HandleNullTexts();
            HandleRarity();
            HandleEntityRarity();
            PrepareFormatting();

            void HandleRarity()
            {
                if (!data.enableRarity) return;
                var world = World.active;
                if (world == null) return;

                var rarityProperties = world.RarityProperty(data.rarity);
                var color = rarityProperties.color;

                color.a = .65f;
                components.type.color = rarityProperties.color;


                foreach (var image in rarityInfo.rarityImageTarget)
                {
                    image.color = color;

                }

                foreach (var tmpro in rarityInfo.textTargets)
                {
                    tmpro.color = color;
                }
            }

            void HandleEntityRarity()
            {
                if (!data.enableEntityRarity) return;
                var world = World.active;
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

        public async void SetVisibility(bool visible)
        {
            this.visible = visible;

            while (formatting)
            {
                await Task.Yield();
            }

            ArchUI.SetCanvas(components.canvasGroup, visible);
        }

        public async void FollowMouse()
        {
            if (!followMouse) return;

            while (this)
            {
                AdjustToPosition(Input.mousePosition);
                await Task.Yield();
            }
        }

        public void AdjustToPosition(Vector3 position)
        {

            float xAnchor = position.x > Screen.width / 2 ? maxXAnchor : minXAnchor;
            float yAnchor = position.y > Screen.height / 2 ? maxYAnchor : minYAnchor;

            components.rectTransform.pivot = new Vector2(xAnchor, yAnchor);

            transform.position = position;
        }

        public void AdjustSize(ContentSizeFitter[] sizeFitters)
        {
            foreach (var sizeFitter in sizeFitters)
            {
                sizeFitter.enabled = false;
                sizeFitter.enabled = true;
            }
            components.sizeFitter.AdjustToSize();
            return;
        }
    }

    public struct ToolTipData
    {
        public string name, subeHeadline, type, description, attributes, requirements, value;
        public Sprite icon;
        public bool enableRarity;
        public bool enableEntityRarity;
        public Rarity rarity;
        public EntityRarity entityRarity;
        
    }
}
