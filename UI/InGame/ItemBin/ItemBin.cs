using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Architome
{
    public class ItemBin : MonoBehaviour
    {
        // Start is called before the first frame update
        ModuleInfo module;
        SizeFitter sizeFitter;
        List<InventorySlot> slots;


        [SerializeField]Transform slotParent;
        [SerializeField] bool update;

        [SerializeField] Image chestIcon;
        [SerializeField] TextMeshProUGUI title;

        void Start()
        {
            
        }

        private void OnValidate()
        {
            if (!update) return; update = false;
            GetDependencies();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        void GetDependencies()
        {
            module = GetComponent<ModuleInfo>();
            sizeFitter = GetComponent<SizeFitter>();
        }


        public void SetItemBin(ItemBinData binData)
        {

            var itemDatas = binData.items;
            var maxItemSlots = binData.maxSlots;

            if (module == null)
            {
                GetDependencies();
            }

            if (binData.moduleIcon != null)
            {
                chestIcon.sprite = binData.moduleIcon;
            }

            if (binData.title != null && binData.title.Length > 0)
            {
                title.text = binData.title;
            }

            if (itemDatas.Count > maxItemSlots)
            {
                throw new System.Exception("Cannot create more items than max slots");
            }

            CreateItemSlots();
            CreateItems();

            ArchAction.Yield(() => sizeFitter.AdjustToSize());
            //sizeFitter.AdjustToSize();

            void CreateItemSlots()
            {
                var inventorySlotPrefab = module.prefabs.inventorySlot;
                slots = new();
                for (int i = 0; i < maxItemSlots; i++)
                {
                    var newObject = Instantiate(inventorySlotPrefab, slotParent);
                    var inventorySlot = newObject.GetComponent<InventorySlot>();
                    slots.Add(inventorySlot);
                }
            }

            void CreateItems()
            {
                for (int i = 0; i < itemDatas.Count; i++)
                {
                    if (itemDatas[i].item == null) continue;

                    var itemInfoGameObject = module.CreateItem(itemDatas[i], true);
                    var newItemInfo = itemInfoGameObject.GetComponent<ItemInfo>();
                    newItemInfo.HandleNewSlot(slots[i]);
                    ArchAction.Yield(() => { newItemInfo.ReturnToSlot(); });
                }
            }
        }
        public List<InventorySlot> Slots()
        {
            return slots;
        }

        public struct ItemBinData
        {
            public List<ItemData> items;
            public int maxSlots;
            public string title;
            public Sprite moduleIcon;
        }
    }
}
