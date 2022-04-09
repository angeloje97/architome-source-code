using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Architome.Enums
{
    public enum NPCType
    {
        Friendly,
        Neutral,
        Hostile,
        Untargetable
    }

    public enum ArchInputMode
    {
        Combat,
    }

    public enum ObjectType
    {
        Entity,
        Model,
        Structure
    }

    public enum ScannerType
    {
        LineOfSightScan,
        RadiusScan,
        ProjectileScanner
    }

    public enum AbilityType
    {
        LockOn,
        SkillShot,
        SkillShotPredict,
        Spawn,
        Use,
    }

    public enum AbilityType2
    {
        Passive,
        AutoAttack,
        PartyAbility,
        Utility,
        MainAbility,
        Item,
    }

    public enum AbilityFunction
    {
        Recastable,
        HoldToCharge,
        Channel,
    }

    public enum BuffTargetType
    {
        Neutral = 0,
        Assist = 1,
        Harm = 2,
    }
    public enum CatalystType
    {
        Cast,
        Shoot,
        Melee,
        Sweep,
        Radiate
    }

    public enum AttackTrigger
    {
        OnDetection,
        OnFocusChange,
        OnAssist,
        OnPassive,
        OnAggressive,
    }

   public enum SheathType
    {
        None,
        Spine,
        Hips,
        Back,
        Cape,
        HipsAttachment,
    }

    public enum WeaponHolder
    {
        RightHand,
        LeftHand,
        RightLeft,
        LeftRight,

    }

    

    public enum EntityState
    {
        Active = 0,
        Stunned = 1,
        Silenced = 2,
        Immobalized = 3,
        Taunted = 4,
        MindControlled = 5,
        Busy = 6,
        Casting = 7,
        Slowed = 8,
        Immune = 9,
    }

    public enum BehaviorState
    {
        Inactive,
        Idle,
        Attacking,
        Assisting,
        Casting,
        Moving,
        WantsToCast,
        Fleeing,
    }

    public enum Difficulty
    {
        Easy = 1,
        Medium = 2,
        Hard = 3,
        Extreme = 4,
    }

    public enum QuestState
    {
        Unavailable,
        Available,
        Active,
        Completed,
        Failed
    }

    public enum QuestType
    {
        Main,
        Side,
        Dungeon,
    }

    public enum CombatBehaviorType
    {
        Passive = 0,
        Reactive = 1,
        Proactive = 2,
        Aggressive = 3,
    }

    public enum CompletionType
    {
        Parallel,
        Linear,
        Radio
    }

    public enum SpecialTargeting
    {
        TargetsCurrent,
        TargetsFocus,
        TargetsRandom,
        SkillShotPredict,
        SkillShotRandom,
        SkillShotNormal,
        Use,
    }

    public enum EntityControlType
    {
        NoControl = 0,
        PartyControl = 1,
        RaidControl = 2,
        EntityControl = 3,
    }

    public enum EntityRarity
    {
        Common = 0,
        Player = 1,
        Rare = 2,
        Elite = 3,
        Boss = 4
    }

    public enum Rarity
    {
        Poor = 0,
        Common = 1,
        Uncommon = 2,
        Rare = 3,
        Epic = 4,
        Legendary = 5,
        Artifact = 6,
        Heirloom = 7,
    }

    public enum ProjectileType
    {
        LockOn,
        SkillShot
    }

    public enum AIBehaviorType
    {
        FullControl,
        HalfPlayerControl,
        NoControl,
    }

    public enum ResourceBarType
    {
        HealthBar,
        EnergyBar,
        ManaBar
    }

    public enum Role
    {
        Damage,
        Tank,
        Healer
    }
    public enum AOEType
    {
        FlatSpread,
        Distribute,
        Multiply
    }

    //How a buff changes it's values based on how many targets are hit or how many targets are assisted
    public enum BuffBehaviorType
    {
        None,

        IncreaseHelpOnAssist,       //Increase The value of a help buff the more it assists allies
        IncreaseHelpOnDamage,       //Increase the value of a help buff the more it hits enemies
        IncreaseHarmOnDamage,       //Increases the value of a harm buff the more it hits enemies
        IncreaseHarmOnAssist,       //Increase the value of a harm buff the more it assists allies.

        DecreaseHelpOnAssist,       //Decreases the value of a help buff the more it assists allies
        DecreaseHelpOnDamage,       //Decreases the value of a help buff the more it hits enemies    
        DecreaseHarmOnDamage,       //Decreases the value of a harm buff the more it damages enemies.
        DecreaseHarmOnAssist          //Decrease the value of a harm buff the more it assists allies.
    }

    public enum ItemType
    {
        Equipment,
        Consumable,
        QuestItem,
        Catalyst,
        Misc
    }

    public enum TargetableState
    {
        None,
        Selected,
        Hovering,
        Holding,
    }

    public enum EquipmentSlotType
    {
        None,
        Head,
        Chest,
        Back,
        Leg,
        MainHand,
        OffHand,
        Ring,
        Neck,
        Gloves,
        Boots,
        Shoulder
    }
    public enum WeaponType
    {
        TwoHandedSword,
        OneHandSword,
        Bow,
        Crossbow,
        Staff,
        Shield
    }

    public enum ArmorType
    {
        Plate = 4,
        Mail = 3,
        Leather = 2,
        Cloth = 1,
    }

    public enum DamageType
    {
        Physical,
        Magical,
        True
    }
    public enum Sex
    {
        Male,
        Female,
        Neutral
    }

    public enum WorkType
    {
        Use,
    }

    public enum TaskState
    {
        Inactive,
        Available,
        Occupied,
        Done,
    }

    public enum WorkerState
    {
        Idle,
        MovingToWork,
        Working,
        Lingering,
    }
    
}
public class Enums : MonoBehaviour
{

}
