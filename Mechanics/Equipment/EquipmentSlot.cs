using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using Architome;
using System;
public class EquipmentSlot : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject entityObject;
    public EntityInfo entityInfo;
    public CharacterInfo charInfo;
    public CharacterBodyParts bodyParts;


    public Transform bodyPart;
    public Transform bodyPart2;
    
    public EquipmentSlotType equipmentSlotType;
    public Equipment equipment;


    [Header("Weapon Properties")]
    public GameObject weaponObject;

    [Header("Sheath Properties")]
    public bool sheathed;
    public Transform sheathObject;

    public bool savePosition;
    public bool toggleSheath;

    //Private variables
    bool requiresUpdate;
    public Equipment previousEquipment;

    Equipment original;
    Weapon weapon;

    public Action<Equipment> OnLoadEquipment { get; set; }

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

        if (equipment != null)
        {
            original = equipment;

            //equipment = Instantiate(equipment);
            weapon = Item.IsWeapon(equipment) ? (Weapon)equipment : null;
        }
        
    }
    void Start()
    {
        GetDependencies();
    }

    public void OnSheathChange(bool val)
    {
        if(equipmentSlotType != EquipmentSlotType.MainHand && equipmentSlotType != EquipmentSlotType.OffHand) { return; }

        if(val)
        {
            ArchAction.Delay(() => {
                if (charInfo.properties.combatSheath)
                {

                    Sheath(true);
                    return;
                }

                Sheath(val);
            }, .125f);
        }
        else
        {
            if(charInfo.properties.combatSheath)
            {
                Sheath(true);
                return;
            }

            Sheath(val);
        }
        
        
    }


    void Update()
    {
        HandleNewEquipment();
        HandleWeapon();
    }
    void HandleWeapon()
    {
        if (!IsWeapon()) { return; }
        HandleSavePosition();
        HandleToggleSheeth();
        HandleSaveSheathPosition();

        if (!weapon.usesSecondDraw) return;
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
                if (weaponObject != null)
                {
                    Destroy(weaponObject);
                }
                previousEquipment = equipment;
                requiresUpdate = true;
                if (Item.IsWeapon(equipment))
                {
                    weapon = (Weapon)equipment;
                }

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
        if (weapon.usesSecondDraw) return;

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
            var weapon = (Weapon)original;
            var child = new GameObject();
            //foreach(Transform children in transform)
            //{
            //    Destroy(child);
            //    child = children.gameObject;
            //}

            weapon.SetUnsheath(weaponObject.transform);
            
            //weapon.itemObject.transform.localPosition = CurrentWeapon().transform.localPosition;
            //weapon.itemObject.transform.localRotation = CurrentWeapon().transform.localRotation;

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

            weapon.SetSheath(weaponObject.transform);


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
        if (weaponObject == null) return;
        var weapon = (Weapon)equipment;
        var sheathPart = bodyParts.BodyPartTransform(weapon.sheathPart);
        var drawPart = bodyParts.BodyPartTransform(weapon.drawPart);

        if (sheathed)
        {
            if (!weapon.usesSecondDraw)
            {
                weaponObject.transform.SetParent(sheathPart);
            }
            weaponObject.transform.localPosition = weapon.sheathPosition;
            weaponObject.transform.localRotation = weapon.sheathRotation;
        }
        else
        {
            if (!weapon.usesSecondDraw)
            {
                weaponObject.transform.SetParent(drawPart);
            }

            weaponObject.transform.localPosition = weapon.unsheathPosition;
            weaponObject.transform.localRotation = weapon.unsheathRotation;

        }
    }
    public void ShowWeapon()
    {
        if (equipment == null || entityObject == null || entityInfo == null)
        {
            return;
        }
        if (!IsWeapon()) { return; }
        if(equipmentSlotType == equipment.equipmentSlotType || equipmentSlotType == equipment.secondarySlotType)
        {

            var bodyPart = bodyParts.BodyPartTransform(weapon.drawPart);


            weaponObject = Instantiate(equipment.itemObject, transform);

            weaponObject.AddComponent<WeaponObject>().SetWeapon(this);

            

            if (bodyPart && !weapon.usesSecondDraw)
            {
                weaponObject.transform.SetParent(bodyPart);
            }


            DetermineSheathObject();
            DetermineUnSheathObject();
            UpdatePosition();
        }

        void DetermineSheathObject()
        {
            
            var weapon = (Weapon)equipment;
            if(bodyParts == null) { return; }
            

            sheathObject = bodyParts.BodyPartTransform(weapon.sheathPart);

        }

        void DetermineUnSheathObject()
        {
            var weapon = (Weapon)equipment;

            bodyPart = bodyParts.BodyPartTransform(weapon.drawPart);
            bodyPart2 = bodyParts.BodyPartTransform(weapon.secondDraw);

        }
    }
    void StayOnMidPoint()
    {
        if(bodyPart2 == null || bodyPart == null) { return; }
        if (sheathed) { return; }
        if (!weapon.usesSecondDraw) return;
        
        transform.position = bodyPart.transform.position;

        transform.LookAt(bodyPart2.transform);
    }
    public bool IsWeapon()
    {
        if(equipment == null) { return false; }
        if (!Item.IsWeapon(equipment)) return false;

        return true;
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
        return weaponObject;
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
