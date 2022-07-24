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


        public void SetBorderRarityColor()
        {
            //var color = World.active.rarities.Find(rarity => rarity.name == info.itemInfo.item.rarity).color;
            //info.borderImage.color = color;
        }
    }

}
