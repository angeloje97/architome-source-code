using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using System;
using Architome;

public class CharacterInfo : MonoBehaviour
{
    [Serializable]
    public struct CharProperties
    {
        public bool combatSheath;
    }

    public CharProperties properties;

    // Start is called before the first frame update
    public GameObject entityObject;

    public EntityInfo entityInfo;
    public Movement movement;
    public AbilityManager abilityManager;
    public AbilityInfo currentCasting;
    public List<GameObject> characterProperties;
    public Stats totalEquipmentStats;
    public bool sheathed;
    public List<EquipmentSlot> equipment;

    public Vector3 currentDirection;

    CharacterTaskHandler taskHandler;

    public Action<EquipmentSlot, Equipment, Equipment> OnChangeEquipment;
    public Action<bool> OnChangeSheath;

    bool sheathCheck;
    public void GetDependencies()
    {
        if(movement == null)
        {
            if(entityInfo == null)
            {
                if(GetComponentInParent<EntityInfo>())
                {
                    entityInfo = GetComponentInParent<EntityInfo>();
                    entityObject = entityInfo.gameObject;

                    if(entityInfo.Movement())
                    {
                        movement = entityInfo.Movement();
                    }
                }
            }
        }

        if(abilityManager == null && entityInfo && entityInfo.AbilityManager())
        {
            abilityManager = entityInfo.AbilityManager();
        }
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

    void Start()
    {
        GetProperties();
        taskHandler.SetCharacter(this);
        Invoke("UpdateCharacterModel", .125f);
        Invoke("UpdateEquipmentStats", .125f);
    }
    // Update is called once per frame
    void Update()
    {
        if(entityInfo && !entityInfo.isAlive) { return; }
        GetDependencies();
        HandleLookAt();
        UpdateMovementDirection();
        HandleEvents();
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
        if(abilityManager.currentlyCasting == null)
        {
            currentCasting = null;
            LookAtMovementDirection();
            return;
        }

        currentCasting = abilityManager.currentlyCasting;

        if(currentCasting.requiresLockOnTarget){ LookAtTarget(); }
        else { LookAtLocation(); }



        void LookAtTarget()
        {
            if(currentCasting == null) { return; }
            if(currentCasting.targetLocked == null) { return; }
            LookAt(currentCasting.targetLocked.transform);
        }

        void LookAtLocation()
        {
            var location = currentCasting.location;

            location.y = transform.position.y;

            transform.LookAt(location);
        }

        void LookAtMovementDirection()
        {
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
        var position = target.position;

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
    public void UpdateCharacterModel()
    {
        ApplyOriginal();
        ApplyEquipment();

        void ApplyOriginal()
        {
            if (ArchiChar() == null) { return; }
            if (ArchiChar().originalParts == null) { return; }

            var original = ArchiChar().originalParts;

            foreach (Vector2 current in original)
            {
                ArchiChar().SetPart((int)current.x, (int)current.y);
            }
        }
        void ApplyEquipment()
        {
            if (ArchiChar() == null) { return; }
            foreach (EquipmentSlot current in equipment)
            {
                if (current.equipment == null) { continue; }

                var currentEquip = current.equipment;

                if (currentEquip.equipmentOverRide == null) { continue; }

                var overRide = currentEquip.equipmentOverRide;

                foreach (Vector2 currentOverRide in overRide)
                {
                    ArchiChar().SetPart((int)currentOverRide.x, (int)currentOverRide.y);
                }
            }
        }
    }
    public void UpdateEquipmentStats()
    {
        if (totalEquipmentStats == null) { totalEquipmentStats = new Stats(); }
        totalEquipmentStats.ZeroOut();
        UpdateStats();

        void UpdateStats()
        {
            foreach (EquipmentSlot currentEquip in equipment)
            {
                if (!currentEquip.equipment) { continue; }

                var current = currentEquip.equipment;
                totalEquipmentStats = totalEquipmentStats.Sum(totalEquipmentStats, current.stats);
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
    }

    public void OnStartTask(TaskEventData eventData)
    {
        
        character.SheathWeapons(true);

        if(eventData.task.properties.station)
        {
            character.LookAt(eventData.task.properties.station.transform);
        }
    }

    public void OnEndTask(TaskEventData eventData)
    {
        character.SheathWeapons(false);
    }
}
