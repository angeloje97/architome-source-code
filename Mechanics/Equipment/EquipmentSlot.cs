using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
public class EquipmentSlot : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;
    public CharacterInfo charInfo;
    public CharacterBodyParts bodyParts;


    public GameObject bodyPart;
    public GameObject bodyPart2;
    
    public EquipmentSlotType equipmentSlotType;
    public Equipment equipment;


    [Header("Sheath Properties")]
    public bool sheathed;
    public GameObject sheathObject;
    public Transform sheathPosition;

    public bool savePosition;
    public bool toggleSheath;

    //Private variables
    bool requiresUpdate;
    public Equipment previousEquipment;

    public void GetDependencies()
    {

        if (GetComponentInParent<CharacterInfo>())
        {
            charInfo = GetComponentInParent<CharacterInfo>();

            if(charInfo && charInfo.BodyParts())
            {
                bodyParts = charInfo.BodyParts();
            }

            charInfo.OnChangeSheath += OnSheathChange;
        }

        if(GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            entityObject = entityInfo.gameObject;
        }
        
    }
    void Start()
    {
        GetDependencies();
    }

    public void OnSheathChange(bool val)
    {
        if(equipmentSlotType != EquipmentSlotType.MainHand && equipmentSlotType != EquipmentSlotType.OffHand) { return; }

        Sheath(val);
    }


    void Update()
    {
        HandleNewEquipment();
        if(Item.IsWeapon(equipment))
        {
            HandleWeapon();
        }
    }
    void HandleWeapon()
    {
        if (!IsWeapon()) { return; }
        HandleSavePosition();
        HandleToggleSheeth();
        HandleSaveSheathPosition();
        StayOnPart();
        StayOnMidPoint();
        StayOnSheath();
    }
    void HandleNewEquipment()
    {
        CheckForNewEquipment();
        HandleRequiresUpdate();

        void CheckForNewEquipment()
        {
            if (previousEquipment != equipment)
            {
                charInfo.OnChangeEquipment?.Invoke(this, previousEquipment, equipment);
                previousEquipment = equipment;
                requiresUpdate = true;
            }
        }
        void HandleRequiresUpdate()
        {
            if(requiresUpdate)
            {
                requiresUpdate = false;
                UpdateNewEquipment();
                HandleNullEquipment();
            }

            void UpdateNewEquipment()
            {
                ShowWeapon();
                UpdateEntity();
                UpdateCharacter();
                UpdateCatalyst();
                

                void UpdateCharacter()
                {
                    if(charInfo)
                    {
                        charInfo.UpdateCharacterModel();
                    }
                }

                void UpdateEntity()
                {
                    if (entityInfo != null)
                    {
                        entityInfo.UpdateCurrentStats();
                    }
                }


                void UpdateCatalyst()
                {
                    //if(equipmentSlotType != EquipmentSlotType.MainHand) { return; }
                    //if(entityInfo == null) { return; }
                    //if(entityInfo.AbilityManager() == null) { return; }

                    //var attackAbility = entityInfo.AbilityManager().attackAbility;

                    

                    //var weapon = (Weapon) equipment ;
                    
                    
                    //if(attackAbility == null) { return; }
                    //if(weapon == null || weapon.weaponCatalyst == null)
                    //{
                    //    var defaultCatalyst = GMHelper.WorldSettings().defaultCatalyst;

                    //    attackAbility.catalyst = defaultCatalyst;
                    //    attackAbility.UpdateAbility();

                    //    return; 
                    //}

                    //attackAbility.catalyst = weapon.weaponCatalyst;
                    //attackAbility.UpdateAbility();
                }

            }


            void HandleNullEquipment()
            {
                if(equipment != null) { return; }
                foreach(Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
                


            }
        }
        
    }
    void StayOnPart()
    {
        if(bodyPart2 != null) { return; }
        if(sheathed && sheathObject != null) { return; }
        if(bodyPart)
        {
            transform.position = bodyPart.transform.position;
            transform.rotation = bodyPart.transform.rotation;
        }
        
    }
    public void HandleSavePosition()
    {
        if(equipment == null) { return; }
        if (sheathed) { return; }
        if (savePosition)
        {
            var weapon = (Weapon)equipment;
            var child = new GameObject();
            foreach(Transform children in transform)
            {
                Destroy(child);
                child = children.gameObject;
            }
            weapon.unsheathPosition = CurrentWeapon().transform.localPosition;
            weapon.unsheathRotation = CurrentWeapon().transform.localRotation;

            weapon.itemObject.transform.localPosition = CurrentWeapon().transform.localPosition;
            weapon.itemObject.transform.localRotation = CurrentWeapon().transform.localRotation;

            savePosition = false;
        }
    }
    public void HandleSaveSheathPosition()
    {
        if(equipment == null) { return; }
        if(!sheathed) { return; }

        if(savePosition)
        {
            var weapon = (Weapon)equipment;
            var child = new GameObject();

            foreach(Transform children in transform)
            {
                Destroy(child);
                child = children.gameObject;
            }
            weapon.sheathPosition = CurrentWeapon().transform.localPosition;
            weapon.sheathRotation = CurrentWeapon().transform.localRotation;


            savePosition = false;
        }
    }
    void StayOnSheath()
    {
        if (!sheathed) { return; }
        if (sheathObject == null) { return; }

        transform.position = sheathObject.transform.position;
        transform.rotation = sheathObject.transform.rotation;
    }
    public void HandleToggleSheeth()
    {
        if (!CurrentWeapon()) { return; }
        if (!CurrentPrefab()) { return; }
        if(toggleSheath)
        {
            toggleSheath = false;
            sheathed = !sheathed;
            UpdatePosition();
        }
    }

    public void Sheath(bool val)
    {
        if (!CurrentWeapon()) { return; }
        if (!CurrentPrefab()) { return; }
        sheathed = val;
        UpdatePosition();
    }
    void UpdatePosition()
    {
        if (!IsWeapon()) { return; }
        var weapon = (Weapon)equipment;
        if (sheathed)
        {
            CurrentWeapon().transform.localPosition = weapon.sheathPosition;
            CurrentWeapon().transform.localRotation = weapon.sheathRotation;
        }
        else
        {
            CurrentWeapon().transform.localPosition = weapon.unsheathPosition;
            CurrentWeapon().transform.localRotation = weapon.unsheathRotation;
        }
    }
    public void ShowWeapon()
    {
        if(equipmentSlotType == EquipmentSlotType.MainHand)
        {
            if(equipment == null)
            {
                
            }
        }

        if (equipment == null || entityObject == null || entityInfo == null)
        {
            return;
        }
        if (!IsWeapon()) { return; }
        if(equipmentSlotType == equipment.equipmentSlotType || equipmentSlotType == equipment.secondarySlotType)
        {
            

            Instantiate(equipment.itemObject, transform);
            DetermineSheathObject();
            DetermineUnSheathObject();
            UpdatePosition();
        }

        void DetermineSheathObject()
        {
            
            var weapon = (Weapon)equipment;
            if(weapon.sheathType == SheathType.None) { return; }
            if(bodyParts == null) { return; }

            switch(weapon.sheathType)
            {
                case SheathType.Back:
                    if(bodyParts.backAttachment) { sheathObject = bodyParts.backAttachment; };
                    break;
                case SheathType.Hips:
                    if(bodyParts.hips) { sheathObject = bodyParts.hips; };
                    break;
                case SheathType.Cape:
                    if(bodyParts.capeAttachment) { sheathObject = bodyParts.capeAttachment; }
                    break;
                case SheathType.HipsAttachment:
                    if (bodyParts.hipsAttachment) { sheathObject = bodyParts.hipsAttachment; };
                    break;
                case SheathType.None:
                    sheathObject = null;
                    break;
            }
        }

        void DetermineUnSheathObject()
        {
            var weapon = (Weapon)equipment;

            switch(weapon.weaponHolder)
            {
                case WeaponHolder.LeftHand:
                    if(bodyParts.leftHand) { bodyPart = bodyParts.leftHand; }
                    bodyPart2 = null;
                    break;
                case WeaponHolder.LeftRight:
                    if(bodyParts.leftHand && bodyParts.rightHand)
                    {
                        bodyPart = bodyParts.leftHand;
                        bodyPart2 = bodyParts.rightHand;
                    }
                    break;

                case WeaponHolder.RightLeft:
                    if(bodyParts.leftHand && bodyParts.rightHand)
                    {

                        bodyPart = bodyParts.rightHand;
                        bodyPart2 = bodyParts.leftHand;
                    }
                    break;
                default:
                    if(bodyParts.rightHand) { bodyPart = bodyParts.rightHand; }
                    bodyPart2 = null;
                    break;
            }
        }
    }
    void StayOnMidPoint()
    {
        if(bodyPart2 == null || bodyPart == null) { return; }
        if (sheathed) { return; }
        transform.position = bodyPart.transform.position;

        transform.LookAt(bodyPart2.transform);
    }
    public bool IsWeapon()
    {
        if(equipment == null) { return false; }
        var value = equipment.GetType();
        if (value == typeof(Weapon)) { return true; }
        return false;
    }
    public bool IsShield()
    {
        if (!IsWeapon()) { return false; }

        var weapon = (Weapon)equipment;

        if(weapon.weaponType == WeaponType.Shield)
        {
            return true;
        }

        return false;
    }
    public GameObject CurrentWeapon()
    {
        if(transform.childCount > 0)
        {
            return transform.GetChild(0).gameObject;
        }
        return null;
    }
    public GameObject CurrentPrefab()
    {
        if(equipment && equipment.itemObject)
        {
            return equipment.itemObject;
        }
        return null;
    }
}
