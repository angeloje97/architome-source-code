using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Architome.Enums;
using UnityEngine.UI;
using TMPro;
using System.Linq;

namespace Architome
{
    public class ItemShaderHandler : MonoBehaviour
    {

        [Serializable]
        public struct Info
        {
            public ItemInfo itemInfo;
            public Image borderImage;
            public Image itemIcon;
            public TextMeshProUGUI itemQuantity;
        }

        [Serializable]
        public struct Presets
        {
            [Serializable]
            public struct RarityColor
            {
                public Rarity rarity;
                public Color color;
            }

            public List<RarityColor> rarityColors;
        }

        

        public Presets presets;
        public Info info;
        // Start is called before the first frame update

        void GetDependencies()
        {
            info.itemInfo = GetComponent<ItemInfo>();
            
            if (info.itemInfo)
            {
                info.itemInfo.OnUpdate += OnUpdate;
            }
        }

        void Awake()
        {
            GetDependencies();
            //ArchAction.Delay(() => OnUpdate(info.itemInfo), .125f);
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void OnUpdate(ItemInfo item)
        {

            UpdateGraphics();
        }

        public void UpdateGraphics()
        {

            SetBorderRarityColor();
        }

        public Color RarityColor(Rarity itemRarity)
        {
            var rarityPreset = presets.rarityColors.Find(preset => preset.rarity == itemRarity);

            Debugger.InConsole(2359, $"{rarityPreset.rarity} is {rarityPreset.color}");
            return rarityPreset.color;

        }

        public void SetBorderRarityColor()
        {
            var color = RarityColor(info.itemInfo.item.rarity);

            info.borderImage.color = color;
        }
    }

}
