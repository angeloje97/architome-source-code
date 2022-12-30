using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;
using Architome.Serialization;
using Architome.Enums;

namespace Architome
{
    [RequireComponent(typeof(AudioSource))]
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(ModuleSaveHandler))]
    public class ModuleInfo : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IPointerExitHandler
    {
        // Start is called before the first frame update
        
        

        public CanvasGroup canvasGroup;

        public bool isActive;
        public bool isHovering;
        public bool blocksInput;
        public bool forceActive;
        public bool selfEscape;
        public bool haltIGGUI;
        public bool haltInput;
        public bool persistantModule;
        public TextMeshProUGUI title;

        public Transform itemBin;

        public AudioClip openSound;
        public AudioClip closeSound;
        public AudioSource audioSource;

        public Action<bool> OnActiveChange;

        public event Action<EntityInfo> OnSelectEntity;

        ArchInput archInput;

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
        public EntityInfo currentEntitySelected;
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

            HandleConflicts();

        }
        void Start()
        {
            GetDependencies();
        }

        void HandleConflicts()
        {
            bool safeEscapeActive = false;

            HandleSelfEscape();
            HandleHaltInput();
            HandleHaltIGGUI();

            if (persistantModule)
            {
                var sceneManager = ArchSceneManager.active;

                sceneManager.AddListener(SceneEvent.OnLoadSceneLate, () =>
                {
                    HandleSelfEscape();
                    HandleHaltInput();
                    HandleHaltIGGUI();
                }, this);
            }

            void HandleHaltIGGUI()
            {
                if (!haltIGGUI) return;
                var iggui = IGGUIInfo.active;

                if (!iggui) return;


                iggui.OnClosingModulesCheck += HandleClosingModuleCheck;

                void HandleClosingModuleCheck(IGGUIInfo igInfo, List<bool> checks)
                {
                    if (!isActive) return;
                    checks.Add(false);
                }
            }
            void HandleHaltInput()
            {
                if (!haltInput) return;
                var archInput = ArchInput.active;


                archInput = ArchInput.active;
                if (archInput == null) return;

                OnActiveChange += HandleActiveChange;

                void HandleActiveChange(bool isActive)
                {
                    if (!isActive) return;

                    archInput.HaltInput((obj) => { return this.isActive; });
                }
            }
            void HandleSelfEscape()
            {
                if (!selfEscape) return;
                HandleInput();

                var pauseMenu = PauseMenu.active;


                if (pauseMenu)
                {

                    pauseMenu.OnTryOpenPause += (PauseMenu menu) => {

                        if (isActive)
                        {
                            Debugger.UI(7549, $"{this} is Active. Will block menu.");
                            menu.pauseBlocked = true;

                        }
                    };
                }

                async void HandleInput()
                {
                    if (safeEscapeActive) return;
                    safeEscapeActive = true;
                    while (this)
                    {
                        await Task.Yield();
                        if (Input.GetKeyUp(KeyCode.Escape)) OnEscape();
                    }
                }

                void OnEscape()
                {
                    if (!isActive) return;
                    Debugger.UI(5439, $"Self Escaping {this}");
                    SetActive(false, true);
                }
            }

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
        public void SelectEntity(EntityInfo entity)
        {
            //if (entity == currentEntitySelected) return;
            //entity = currentEntitySelected;
            OnSelectEntity?.Invoke(entity);
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
            SetActive(isActive, false, true);
        }
        public void Toggle()
        {
            SetActive(!isActive);
        }
        public void SetActive(bool val, bool playSound = true, bool OnValidate = false)
        {
            if (isActive == val && !OnValidate) return;

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

            ReturnAllItemsFromBin();
        }
        public void ReturnAllItemsFromBin()
        {
            if (itemBin == null) return;
            foreach (var item in itemBin.GetComponentsInChildren<ItemInfo>())
            {
                item.ReturnToSlot();
            }
        }
        public ItemInfo CreateItem(ItemData data, bool cloned = false)
        {
            var item = data.item;
            if (!prefabs.item) return null;
            if (item == null) return null;
            if (itemBin == null) return null;

            //var itemData = cloned ? Instantiate(item) : item;


            var newItem = Instantiate(prefabs.item, itemBin).GetComponent<ItemInfo>();


            newItem.ManifestItem(data, cloned);

            return newItem;
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


        void Update()
        {
        }
    }

}