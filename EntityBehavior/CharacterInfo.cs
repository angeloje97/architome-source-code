using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;
using Architome;

public class CharacterInfo : EntityProp
{

    
    [Serializable]
    public struct CharProperties
    {
        public bool combatSheath;
    }

    public CharProperties properties;


    [Serializable]
    public struct Modules
    {
        public GearModuleManager gearModule;
    }

    // Start is called before the first frame update
    public GameObject entityObject;
    public Movement movement;
    public AbilityManager abilityManager;
    public AbilityInfo currentCasting;
    public List<GameObject> characterProperties;
    public Stats totalEquipmentStats;
    public Modules modules;

    public bool sheathed;
    public bool isCasting;
        
    public List<EquipmentSlot> equipment;

    public Vector3 currentDirection;

    CharacterTaskHandler taskHandler;
    CharacterAbilityHandler abilityHandler;

    public Action<EquipmentSlot, Equipment, Equipment> OnChangeEquipment;
    public Action<bool> OnChangeSheath;

    [Serializable]
    public struct CharacterRotation
    {
        public bool isActive;
        public float yRotation;
        public float deltaRotation;
        public Vector3 targetVector;

    }

    public CharacterRotation rotation;

    bool sheathCheck;
    public new void GetDependencies()
    {
        base.GetDependencies();

        if (entityInfo)
        {
            movement = entityInfo.Movement();
            abilityManager = entityInfo.AbilityManager();
            entityInfo.OnHiddenChange += OnHiddenChange;
        }

        if (movement)
        {

        }

        if (abilityManager)
        {

        }
        abilityHandler.SetCharacter(this);

    }

    

    public void GetProperties()
    {
        equipment = new List<EquipmentSlot>();
        foreach(Transform child in transform)
        {
            characterProperties.Add(child.gameObject);
            if(child.GetComponent<EquipmentSlot>())
            {
                equipment.Add(child.GetComponent<EquipmentSlot>());
            }
        }
        
    }

    public Transform PetSpot()
    {
        var petSpot = GetComponentInChildren<ArchitomePetSpot>();

        if (petSpot)
        {
            return petSpot.transform;
        }

        return null;
    }

    void Start()
    {
        GetProperties();
        GetDependencies();
        taskHandler.SetCharacter(this);
        Invoke("UpdateEquipmentStats", .125f);
    }
    // Update is called once per frame
    void Update()
    {
        if(entityInfo && !entityInfo.isAlive) { return; }
        HandleLookAt();
        UpdateMovementDirection();
        HandleEvents();
        HandleMetrics();
    }

    async public void CopyRotation(Vector3 targetVector, float smoothening = 4f)
    {
        targetVector.x = 0;
        targetVector.y = 0;

        rotation.targetVector = targetVector;

        if (rotation.isActive) return;

        rotation.isActive = true;
        while (transform.eulerAngles != rotation.targetVector)
        {
            await Task.Yield();
            transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, rotation.targetVector, 1/smoothening);
            
            if (movement.isMoving) break;
        }

        rotation.isActive = false;
    }


    async void OnHiddenChange(bool isHidden)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer == null) continue;
            if (renderer.enabled != !isHidden)
            {
                if (renderer == null) return;
                renderer.enabled = !isHidden;
                await Task.Yield();
            }
        }
    }


    void HandleMetrics()
    {
        rotation.deltaRotation = transform.eulerAngles.y - rotation.yRotation;
        rotation.yRotation = transform.eulerAngles.y;
    }

    public void HandleEvents()
    {
        if(sheathed != sheathCheck)
        {

            sheathCheck = sheathed;
            OnChangeSheath?.Invoke(sheathed);
        }
    }
    void HandleLookAt()
    {
        LookAtMovementDirection();

        void LookAtMovementDirection()
        {
            if (isCasting) return;
            if (movement && movement.isMoving)
            {
                var currentVelocity = movement.velocity;
                var location = (currentVelocity * Time.deltaTime * 2) + transform.position;

                transform.LookAt(location);
            }
        }
    }

    public void LookAt(Transform target)
    {
        LookAt(target.position);
    }

    public void LookAt(Vector3 location)
    {
        var position = location;
        position.y = transform.position.y;

        transform.LookAt(position);
    }

    public void UpdateMovementDirection()
    {
        if(movement)
        {
            var velocity = movement.velocity;

            var locationOffset = (velocity * 2) + transform.position;

            var directionalMovement = V3Helper.Direction(locationOffset, transform.position);

            currentDirection = directionalMovement - transform.forward;
            
        }
    }

    public bool IsFacing(Vector3 position)
    {
        var angle = 90f;

        var angleFromTarget = AngleFromTarget(position);

        if (angle > angleFromTarget)
        {
            return true;
        }

        return false;
    }

    public float AngleFromTarget(Vector3 position)
    {
        var direction = V3Helper.Direction(position, transform.position);

        var angleDifference = direction - transform.forward;

        return V3Helper.Abs(angleDifference) * 180 / (float) Math.PI;
    }

    public void SheathWeapons(bool val)
    {
        sheathed = val;
    }
    public void UpdateEquipmentStats()
    {
        if (totalEquipmentStats == null) { totalEquipmentStats = new Stats(); }
        totalEquipmentStats.ZeroOut();
        UpdateStats();

        void UpdateStats()
        {
            foreach (var equipment in EquippedItems())
            {
                totalEquipmentStats += equipment.stats;
            }
        }

    }
    public Anim Animation()
    {
        foreach(Transform child in transform)
        {
            if(child.GetComponent<Anim>())
            {
                return child.GetComponent<Anim>();
            }
        }

        return null;
    }
    public Equipment EquipmentItem(EquipmentSlotType slotType)
    {
        foreach(GameObject child in characterProperties)
        {
            if(child.GetComponent<EquipmentSlot>())
            {
                if(child.GetComponent<EquipmentSlot>().equipmentSlotType == slotType)
                {
                    return child.GetComponent<EquipmentSlot>().equipment;
                }
            }
        }
        return null;
    }

    public List<Weapon> Weapons()
    {
        var weapons = new List<Weapon>();

        foreach (var slot in GetComponentsInChildren<EquipmentSlot>())
        {
            if (slot.equipment == null) continue;
            if (!Item.IsWeapon(slot.equipment)) continue;

            weapons.Add((Weapon)slot.equipment);
        }

        return weapons;
    }

    public List<Equipment> EquippedItems()
    {
        var equipments = GetComponentsInChildren<EquipmentSlot>().Where(slot => slot.equipment != null).Select(slot => slot.equipment).ToList();
        return equipments;

    }

    public Weapon WeaponItem(EquipmentSlotType slotType)
    {
        var equipments = GetComponentsInChildren<EquipmentSlot>();
        foreach (EquipmentSlot current in equipments)
        {
            if(current.equipment)
            {
                if(Item.IsWeapon(current.equipment) && current.equipmentSlotType == slotType)
                {
                    return (Weapon)current.equipment;
                }
            }
        }
        return null;
    }
    public EquipmentSlot EquipmentSlot(EquipmentSlotType slotType)
    {
        var equipments = GetComponentsInChildren<EquipmentSlot>();

        foreach (EquipmentSlot equipment in equipments)
        {
            if(equipment.equipmentSlotType == slotType)
            {
                return equipment;
            }
        }

        return null;
    }

    public List<EquipmentSlot> EquipmentSlots()
    {
        var equipmentSlots = new List<EquipmentSlot>();
        foreach(Transform child in transform)
        {
            if(child.GetComponent<EquipmentSlot>())
            {
                equipmentSlots.Add(child.GetComponent<EquipmentSlot>());
            }
        }

        return equipmentSlots;
    }

    public List<Equipment> EquipmentItems()
    {

        var itemList = new List<Equipment>();

        foreach(EquipmentSlot i in EquipmentSlots())
        {
            if(i.equipment != null)
            {
                itemList.Add(i.equipment);
            }
        }

        return itemList;
    }

    public ArchitomeCharacter ArchiChar()
    {
        foreach(GameObject property in characterProperties)
        {
            if(property.GetComponent<ArchitomeCharacter>())
            {
                return property.GetComponent<ArchitomeCharacter>();
            }
        }

        return null;
    }
    public CharacterBodyParts BodyParts()
    {
        if(GetComponentInChildren<CharacterBodyParts>())
        {
            return GetComponentInChildren<CharacterBodyParts>();
        }
        return null;
    }
    
}

public struct CharacterTaskHandler
{
    EntityInfo entityInfo;
    CharacterInfo character;

    public void SetCharacter(CharacterInfo character)
    {
        this.character = character; 
        entityInfo = character.GetComponentInParent<EntityInfo>();
        
        if(entityInfo == null) { return; }

        entityInfo.taskEvents.OnStartTask += OnStartTask;
        entityInfo.taskEvents.OnEndTask += OnEndTask;
        entityInfo.taskEvents.OnLingeringEnd += OnLingeringEnd;
    }

    public void OnStartTask(TaskEventData eventData)
    {
        
        character.SheathWeapons(true);

        if(eventData.task.properties.station)
        {
            character.LookAt(eventData.task.properties.station.transform);
        }
    }

    public void OnLingeringEnd(TaskEventData eventData)
    {
        character.SheathWeapons(false);
    }

    public void OnEndTask(TaskEventData eventData)
    {
        if (eventData.task.properties.allowLinger && eventData.TaskComplete) return;
        character.SheathWeapons(false);
    }
}

public struct CharacterAbilityHandler
{
    EntityInfo entityInfo;
    CharacterInfo character;
    AbilityManager abilityManager;
    public void SetCharacter(CharacterInfo character)
    {
        this.character = character;
        entityInfo = character.GetComponentInParent<EntityInfo>();

        if (entityInfo == null) return;
        abilityManager = entityInfo.AbilityManager();

        if (abilityManager == null) return;

        abilityManager.OnAbilityStart += OnAbilityStart;
        abilityManager.OnAbilityEnd += OnAbilityEnd;
        abilityManager.WhileCasting += WhileCasting;
        
        
    }

    public void OnAbilityStart(AbilityInfo ability)
    {
        character.isCasting = true;

        var target = ability.target != null ? ability.target.transform.position : ability.locationLocked;

        if (ability.abilityType == AbilityType.Use) return;
        character.LookAt(target);
    }

    public void WhileCasting(AbilityInfo ability)
    {
        var target = ability.locationLocked;

        if (ability.abilityType == AbilityType.LockOn && ability.target)
        {
            target = ability.target.transform.position;
        }

        if (ability.abilityType == AbilityType.Use) return;
        character.LookAt(target);
    }

    public void OnAbilityEnd(AbilityInfo ability)
    {
        character.isCasting = false;
    }
}