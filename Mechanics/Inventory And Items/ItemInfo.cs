using System.Collections;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using Architome;
using Architome.Enums;
public class ItemInfo : MonoBehaviour, IPointerUpHandler, IPointerDownHandler, IDropHandler
{
    public Item item;
    public int currentStacks { get; set; }
    
    [Header("UI Properties")]
    public InventorySlot currentSlot;
    public InventorySlot currentSlotHover;
    public ModuleInfo moduleHover;
    public Image itemIcon;
    public TextMeshProUGUI amountText;
    public bool isUI;
    bool manifested;
    bool blockLooting { get; set; }
    bool pickedUp;
    public bool thrown { get; private set; }

    EntityInfo entityPickedUp;

    [Serializable]
    public struct WorldProperties
    {
        public bool enable;
        public ParticleSystem ray;
        public ParticleSystem glow;

        public Collider trigger, worldCollider;
    }

    public WorldProperties worldProperties;

    public ItemFXHandler fxHandler;

    #region Events
    public Action<InventorySlot> OnNewSlot { get; set; }
    public Action<ItemInfo> OnUpdate { get; set; }
    public Action<ItemInfo> OnItemAction { get; set; }
    public Action<ItemInfo> OnDepleted { get; set; }
    public Action<ItemInfo> OnDestroy {get; set;}
    public Action<ItemInfo, UseData> OnUse { get; set; }
    public Action<ItemInfo, bool> OnDragChange {get; set;}
    public Action<ItemInfo, bool> OnEquipChange {get; set;}
    public Action<ItemInfo, EntityInfo> OnPickUp { get; set; }
    public Action<ItemInfo, ToolTipElement> BeforeShowItemToolTip { get; set; }

    #endregion

    bool isDestroyed;


    void Start()
    {
        var dragAndDrop = GetComponent<DragAndDrop>();

        if (dragAndDrop)
        {
            dragAndDrop.OnDragChange += (DragAndDrop drag, bool isDragging) => { OnDragChange?.Invoke(this, isDragging); };
        }

        if (item)
        {
            ManifestItem(new() { item = item, amount = currentStacks }, true);
        }

        HandleToolTipElement();
    }

    async public void ThrowRandomly()
    {
        if (isUI) return;

        var rigidBody = GetComponent<Rigidbody>();
        if (rigidBody == null) return;

        thrown = true;

        var groundLayer = LayerMasksData.active.walkableLayer;                                              //This part of the code is to make sure that the item isn't dropped in a weird area
        var structureLayer = LayerMasksData.active.structureLayerMask;
        //var randomPosition = transform.position + V3Helper.RandomVector3(new(-5, 0, -5), new(5, 0, 5));

        transform.Rotate(Vector3.up * UnityEngine.Random.Range(0, 360f));
        var randomPosition = transform.position + transform.forward * 5f;

        int tries = 20;
        while (!V3Helper.IsValidPosition(randomPosition, transform.position, structureLayer, groundLayer))
        {
            transform.Rotate(Vector3.up * UnityEngine.Random.Range(0, 360f));
            randomPosition = transform.position + transform.forward * 5f;

            tries--;

            if (tries <= 0) return;
        }


        transform.rotation = V3Helper.LerpLookAt(transform, randomPosition, 1);

        rigidBody.AddForce(transform.up * 5f, ForceMode.Impulse);
        rigidBody.AddForce(transform.forward * 5f, ForceMode.Impulse);

        worldProperties.worldCollider.isTrigger = true;

        float timer = 1f;
        float colliderTimer = .25f;
        bool colliderFinished = false;

        while (timer > 0)
        {
            if (this == null) return;
            timer -= Time.deltaTime;
            rigidBody.AddForce(20 * Vector3.down, ForceMode.Acceleration);


            if (colliderTimer > 0)
            {
                colliderTimer -= Time.deltaTime;
            }
            else if(!colliderFinished)
            {
                colliderFinished = true;
                worldProperties.worldCollider.isTrigger = false;
            }


            await Task.Yield();
        }
    }

    void OnValidate()
    {
        if (item == null) { return; }

        UpdateItemInfo();
    }
    void HandleToolTipElement()
    {
        var toolTipElement = GetComponent<ToolTipElement>();

        if (toolTipElement)
        {
            toolTipElement.BeforeShowToolTip += (ToolTipElement element) => {
                element.data = item.ToolTipData(currentStacks);
                BeforeShowItemToolTip?.Invoke(this, element);
            };
        }
    }
    void HandleEvents()
    {
        if (item.effects == null) return;
        if (!isUI) return;
        OnDepleted += (ItemInfo item) => { item.fxHandler.HandleItemFX(item, ItemEvent.OnDeplete); };
        OnUse += (ItemInfo item, UseData data) => { item.fxHandler.HandleItemFX(item, ItemEvent.OnUse); };
        OnDestroy += (ItemInfo item) => { item.fxHandler.HandleItemFX(item, ItemEvent.OnDestroy); };
        OnDragChange += (ItemInfo item, bool isDragging) => {
            if (isDragging)
            {
                item.fxHandler.HandleItemFX(item, ItemEvent.OnDragStart);
            }
            else
            {
                item.fxHandler.HandleItemFX(item, ItemEvent.OnDragEnd);
            }
        };
        OnEquipChange += (ItemInfo item, bool equip) => {
            if (item.fxHandler == null) return;
            if (equip) item.fxHandler.HandleItemFX(item, ItemEvent.OnEquip);
            else item.fxHandler.HandleItemFX(item, ItemEvent.OnUnequip);
        };
    }
    #region 3d World Trigger
    private void OnTriggerEnter(Collider other)
    {
        HandlePickUp(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandlePickUp(collision.gameObject);
    }

    void HandlePickUp(GameObject other)
    {
        if (isUI) return;
        if (blockLooting) return;
        var entity = other.GetComponent<EntityInfo>();
        if (entity == null) return;
        if (!Entity.IsPlayer(entity.gameObject)) return;
        if (!entity.CanPickUp(new(this))) return;

        var success = entity.LootItem(this, true);

        entityPickedUp = entity;
        pickedUp = true;
        DestroySelf();

        //if (success)
        //{

        //}
    }
    #endregion

    #region Event System
    bool disableMove;
    DragAndDrop dragAndDrop;
    public void SetMovable(bool movable)
    {
        if(dragAndDrop == null)
        {
            dragAndDrop = GetComponent<DragAndDrop>();

            if (!dragAndDrop) return;
        }

        disableMove = !movable;

        dragAndDrop.enabled = movable;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (disableMove) return;
        ReturnToSlot();
        if (moduleHover != null || currentSlotHover != null) return;

        if (currentSlot && currentSlot.itemSlotHandler)
        {
            currentSlot.itemSlotHandler.NullHover(this);
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (disableMove) return;
        if(isUI == false) { return; }

        var module = GetComponentInParent<ModuleInfo>();
        var vault = GetComponentInParent<GuildVault>();
        var postDungeon = GetComponentInParent<PostDungeonManager>();
        


        if (module && module.itemBin)
        {
            transform.SetParent(module.itemBin);
        }
        else if(vault)
        {
            transform.SetParent(vault.transform);
        }
        else if(postDungeon)
        {
            transform.SetParent(postDungeon.foreground);
        }


        currentSlotHover = currentSlot;
        moduleHover = module;

    }
    public void OnDrop(PointerEventData eventData)
    {
        if (disableMove) return;
        var dragging = eventData.pointerDrag;
        if (dragging == null) return;
        var itemInfo = dragging.GetComponent<ItemInfo>();
        if (itemInfo == null) return;

        itemInfo.HandleItem(this);
       
    }
    #endregion

    public void ManifestItem(ItemData data, bool cloned)
    {
        if (manifested) return;
        manifested = true;
        
        if(data.amount <= 0)
        {
            data.amount = 1;
        }

        item = cloned ? Instantiate(data.item) : data.item;

        item.AdjustValue();
        currentStacks = data.amount;

        if (currentStacks <= 0) currentStacks = 1;
        if (currentStacks > item.MaxStacks && item.MaxStacks != -1 && item.MaxStacks > 0) currentStacks = item.MaxStacks;

        name = item.itemName;

        currentStacks = item.NewStacks(0, data.amount, out int leftover);
        UpdateItemInfo();
        //this.isInInventory = isInInventory;

        ArchAction.Yield(() => HandleEvents());
        //HandleEvents();
    }
    public void UpdateItemInfo()
    {
        if(item == null) { return; }

        //currentStacks = Mathf.Clamp(currentStacks, 1, maxStacks);

        UpdateStackText();
        UpdateItemIcon();

        UpdateParticles();


        OnUpdate?.Invoke(this);

        if (currentSlot)
        {
            currentSlot.previousItemInfo = null; //Forces the slot to trigger an event
        }

        void UpdateParticles()
        {
            if (!worldProperties.enable) return;
            if (!Application.isPlaying) return;

            var rarityInfo = World.active.RarityProperty(item.rarity);

            var color = rarityInfo.color;

            foreach (var particle in GetComponentsInChildren<ParticleSystem>())
            {
                var main = particle.main;
                main.startColor = color;
            }


            var room = Entity.Room(transform.position);
            if (room)
            {
                transform.SetParent(room.Misc);
            }
        }

        void UpdateItemIcon()
        {
            if(itemIcon == null) { return; }
            if(item && item.itemIcon == null) { return; }

            itemIcon.sprite = item.itemIcon;
        }

        void UpdateStackText()
        {
            if(amountText == null) { return; }

            amountText.gameObject.SetActive(item.MaxStacks > 1 || item.MaxStacks == -1);
            //if (maxStacks == 1)
            //{ 
            //    amountText.gameObject.SetActive(false);
            //    return;
            //}
            
            amountText.text = $"{ArchString.FloatToSimple(currentStacks, 0)}";
        }
    }

    public bool OpenToStack()
    {
        return item.ValidStacks(currentStacks + 1);
    }

    async public void DelayLooting(float seconds = 1f)
    {
        blockLooting = true;

        await Task.Delay((int)(seconds * 1000));

        blockLooting = false;
    }

    async public void ReturnToSlot(int iterations = 1)
    {
        if (currentSlot == null) return;
        if (isUI == false) { return; }
        if (this == null) return;

        //transform.position = currentSlot.transform.position;

        if (currentSlot)
        {
            transform.SetParent(currentSlot.transform);
            transform.localPosition = new(0, 0, 0);
        }

        var itemRect = GetComponent<RectTransform>();
        var slotRect = currentSlot.GetComponent<RectTransform>();

        for (int i = 0; i < iterations; i++)
        {
            //GetComponent<RectTransform>().sizeDelta = currentSlot.GetComponent<RectTransform>().sizeDelta;
            transform.localScale = new(1, 1, 1);
            itemRect.sizeDelta = slotRect.sizeDelta;

            await Task.Yield();
        }

        


        //if (currentSlot.GetComponentInParent<ModuleInfo>() && currentSlot.GetComponentInParent<ModuleInfo>().itemBin)
        //{
        //    transform.SetParent(currentSlot.transform);
            
        //    //transform.SetParent(currentSlot.GetComponentInParent<ModuleInfo>().itemBin);
        //}
    }

    public void HandleNewSlot(InventorySlot slot, bool forceInsert = false)
    {
        var previousSlot = currentSlot;
        var changedSlot = false;
        var canChange = true;

        if (slot == currentSlot) return;

        CheckPrevious();
        HandleInventorySlot();
        HandlePreviousSlot(changedSlot);
        ReturnToSlot();

        void CheckPrevious()
        {
            if (!previousSlot) return;
            if (forceInsert) return;
            if (!previousSlot.CanRemoveFromSlot(this))
            {
                canChange = false;
            }
        }
        void HandleInventorySlot()
        {
            if (!canChange) return;
            if (!slot.CanInsert(this) && !forceInsert) return;

            if (slot.GetType() == typeof(GearSlot))
            {
                OnEquipChange?.Invoke(this, true);
            }

            slot.currentItemInfo = this;
            currentSlot = slot;
            
            changedSlot = true;
            OnNewSlot?.Invoke(slot);
        }

        void HandlePreviousSlot(bool val)
        {
            if (!val) { return; }
            if(previousSlot == null) { return; }
            if (previousSlot.currentItemInfo != this) return;

            if (previousSlot.GetType() == typeof(GearSlot))
            {
                OnEquipChange?.Invoke(this, false);
            }

            previousSlot.currentItemInfo = null;
        }
    }
    public void HandleItem(ItemInfo item)
    {
        if (item == this) return;
        var sameItem = item.item.Equals(this.item);

        if (sameItem)
        {
            var leftOver = item.IncreaseStacks(currentStacks);
            SetStacks(leftOver);

        }
        else
        {
            var otherSlot = item.currentSlot;
            var currentSlot = this.currentSlot;

            if (!otherSlot.CanInsert(this)) return;
            if (!currentSlot.CanInsert(item)) return;

            HandleNewSlot(otherSlot);
            item.HandleNewSlot(currentSlot);
        }
    }
    public int IncreaseStacks(int amount = 1)
    {
        currentStacks = item.NewStacks(currentStacks, amount, out int leftover);
        //if (currentStacks + amount > maxStacks)
        //{
        //    var leftOver = currentStacks + amount - maxStacks;
        //    currentStacks = maxStacks;

        //    return leftOver;
        //}

        //currentStacks += amount;

        UpdateItemInfo();

        return leftover;
    }
    public bool SetStacks(int amount)
    {
        if (!item.ValidStacks(amount)) return false;
        //if (amount > maxStacks || amount < 0) return false;

        currentStacks = amount;

        if (currentStacks <= 0)
        {
            DestroySelf();
        }

        UpdateItemInfo();

        return true;
    }
    public bool ReduceStacks(int amount = 1)
    {
        if (currentStacks < amount) return false;

        currentStacks -= amount;

        if (currentStacks <= 0)
        {
            OnDepleted?.Invoke(this);

            DestroySelf(true);
        }

        
        UpdateItemInfo();

        return true;
    }
    public void DestroySelf(bool triggerDestroyEffect = false)
    {
        if (isDestroyed) return;
        if (currentSlot)
        {
            currentSlot.currentItemInfo = null;
        }

        isDestroyed = true;

        if (pickedUp)
        {
            OnPickUp?.Invoke(this, entityPickedUp);
        }

        if (triggerDestroyEffect)
        {
            OnDestroy?.Invoke(this);
        }

        ArchAction.Yield(() => {
            Destroy(gameObject); 
        });
    }
    async public Task<bool> SafeDestroy()
    {

        bool destroy = false;
        var userChoice = await PromptHandler.active.GeneralPrompt(new()
        {
            icon = item.itemIcon,
            title = $"{item.itemName}",
            question = $"Are you sure you want to destroy {item.itemName}?",
            options = new() {
                new("Destroy", (option) => HandleDestroy()),
                new("Cancel") {isEscape = true },
            },
            
            blocksScreen = true,
        });

        return destroy;

        void HandleDestroy()
        {
            destroy = true;
            DestroySelf(true);
        }
    }
    async public Task<ItemInfo> HandleSplit()
    {
        var promptHandler = PromptHandler.active;
        if (promptHandler == null) return null;

        bool split = false;

        var userChoice = await promptHandler.SliderPrompt(new()
        {
            title = $"Split {item.itemName}",
            question = "Choose amount to split",
            amountMin = 1,
            amountMax = currentStacks - 1,
            icon = item.itemIcon,
            options = new () {
                new("Split", (option) => { split = true; }),
                new("Cancel") { isEscape = true}
            },
        });

        if (split)
        {
            return SeperateItemStack(userChoice.amount);
        }

        return null;


    }
    public bool SameItem(ItemInfo otherItem)
    {
        if (otherItem.item == null) return false;

        if (otherItem.item._id != item._id) return false;

        return true;
    }
    public ItemInfo SplitHalf()
    {
        return SeperateItemStack(currentStacks / 2);
    }
    ItemInfo SeperateItemStack(int amount = 1)
    {
        if (amount > currentStacks) return null;

        var itemPrefab = World.active.prefabsUI.item;
        var worldActions = WorldActions.active;
        if (itemPrefab == null) return null;

        //var newItem = Instantiate(itemPrefab);
        var newItem = worldActions.CreateItemUI(new(item, amount), transform.parent, !disableMove);
        ReduceStacks(amount);

        return newItem;
    }
    public void ActivateAction()
    {
        OnItemAction?.Invoke(this);
        Debugger.UI(0142, $"User right clicked {this}");

        if (currentSlot && currentSlot.itemSlotHandler)
        {
            currentSlot.itemSlotHandler.ItemAction(this);
        }
    }
    public void Use(UseData data)
    {
        item.Use(data);
        OnUse?.Invoke(this, data);
    }
    public bool InsertIntoSlots(List<InventorySlot> slots, bool destroyFail = false)
    {
        var items = new List<ItemInfo>();
        var availableSlots = new List<InventorySlot>();

        foreach (var slot in slots)
        {
            if (slot.currentItemInfo == null)
            {
                if (slot.CanInsert(this))
                {
                    availableSlots.Add(slot);
                }
                continue;
            }
            var amount = slot.currentItemInfo.currentStacks;
            if (!slot.currentItemInfo.item.ValidStacks(amount + 1)) continue;
            if (!slot.currentItemInfo.SameItem(this)) continue;


            
            items.Add(slot.currentItemInfo);
        }

        foreach (var item in items)
        {
            HandleItem(item);
            if (currentStacks == 0) return true;
        }


        if (currentStacks == 0) return true;
        if (availableSlots.Count == 0)
        {
            if (destroyFail)
            {
                DestroySelf();
            }
            return false;
        }

        HandleNewSlot(availableSlots[0]);

        return true;
    }
}
