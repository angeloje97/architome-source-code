using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;



namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CanvasGroup))]
    public class ModuleInfo : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        // Start is called before the first frame update
        
        

        public CanvasGroup canvasGroup;

        public bool isActive;
        public bool isHovering;
        public bool blocksInput;
        public TextMeshProUGUI title;

        public Transform itemBin;

        public AudioClip openSound;
        public AudioClip closeSound;
        public AudioSource audioSource;

        public Action<bool> OnActiveChange;

        public Action<EntityInfo> OnSelectEntity;

        [Serializable]
        public struct Prefabs
        {
            public GameObject item;
            public GameObject ability;
            public GameObject entitySelector;
            public GameObject entityIcon;
            public GameObject inventorySlot;

        }
        public Prefabs prefabs;

        public void GetDependencies()
        {
            if (GetComponent<CanvasGroup>() == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
            else
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                audioSource.outputAudioMixerGroup = GMHelper.Mixer().UI;
                audioSource.playOnAwake = false;
            }
        }

        void Start()
        {
            GetDependencies();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {

            isHovering = true;
            if (!eventData.pointerDrag ||
                !eventData.pointerDrag.GetComponent<ItemInfo>() ||
                itemBin == null) { return; }

            eventData.pointerDrag.transform.SetParent(itemBin);
            var item = eventData.pointerDrag.GetComponent<ItemInfo>();
            item.moduleHover = this;
            transform.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;


            if (!eventData.pointerDrag || !eventData.pointerDrag.GetComponent<ItemInfo>()) return;

            var item = eventData.pointerDrag.GetComponent<ItemInfo>();

            ArchAction.Delay(() =>
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(GetComponent<RectTransform>(), Input.mousePosition)) return;
                //if (GetComponent<RectTransform>().rect.Contains(Input.mousePosition)) return;

                if (item.moduleHover == this)
                {
                    item.moduleHover = null;
                }
            }, .125f);

            
        }



        public void OnPointerDown(PointerEventData eventData)
        {
            transform.SetAsLastSibling();
        }

        public void DestroyBin()
        {
            if (itemBin)
            {
                foreach (Transform child in itemBin)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void Close()
        {
            SetActive(false);
        }

        public void OnValidate()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
            }
            SetActive(isActive, false);
        }

        public void SetActive(bool val, bool playSound = true)
        {
            if (canvasGroup)
            {
                canvasGroup.interactable = val;
                canvasGroup.blocksRaycasts = val;
                canvasGroup.alpha = val ? 1 : 0;
            }
            isActive = val;

            OnActiveChange?.Invoke(val);
            if (playSound)
            {
                if (openSound && closeSound)
                {
                    audioSource.clip = val ? openSound : closeSound;
                    audioSource.Play();

                }
            }

            var iggui = GetComponentInParent<IGGUIInfo>();

            if (iggui)
            {
                iggui.OnModuleEnableChange?.Invoke(this);
            }

            
        }

        public GameObject CreateItem(Item item, bool cloned = false)
        {
            if (!prefabs.item) return null;
            if (item == null) return null;
            if (itemBin == null) return null;

            //var itemData = cloned ? Instantiate(item) : item;


            var newItem = Instantiate(prefabs.item, itemBin).GetComponent<ItemInfo>();

            newItem.item = cloned ? Instantiate(item) : item;
            newItem.UpdateItemInfo();
            newItem.isInInventory = true;

            return newItem.gameObject;


            


        }

        public GameObject CreateEntityIcon(EntityInfo entity)
        {
            if (!prefabs.entityIcon) return null;
            if (entity == null) return null;

            return Instantiate(prefabs.entityIcon, transform);
        }

        public GameObject CreateEntitySelector(EntityInfo entity)
        {
            if (!prefabs.entitySelector) return null;
            if (entity == null) return null;


            return Instantiate(prefabs.entitySelector, transform);
        }


        // Update is called once per frame
        void Update()
        {

        }
    }

}