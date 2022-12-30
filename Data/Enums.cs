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
        Inactive,
        InGameUI,
        Combat,
        Colony,
    }

    public enum Trilogy
    {
        Dungeons,
        Story,
    }

    public enum Size { Small, Medium, Large }

    public enum GameState
    {
        Menu,
        Lobby,
        Play,
    }

    public enum ObjectType
    {
        Entity,
        Model,
        Structure
    }

    public enum MetricType
    {
        Value,
        Percent,
        PercentValue,
    }

    public enum RoomType
    {
        Any,
        Skeleton,
        Random,
        Entrance,
        Boss,
    }

    public enum CameraTarget
    {
        None,
        PartyCenter,
    }

    public enum ScannerType
    {
        LineOfSightScan,
        RadiusScan,
        ProjectileScanner
    }

    public enum ArchScene
    {
        Menu,
        DungeoneerMenu,
        Dungeon,
        PostDungeon,
        Tutorial
    }

    public enum EntitySlotType
    {
        Roster,
        Party,
    }

    public enum AbilityType
    {
        LockOn,
        SkillShot,
        SkillShotPredict,
        Spawn,
        Use,
    }

    public enum AugmentEvent
    {
        OnAugmentTrigger,
        OnAugmentActive,
    }

    public enum CatalystEvent
    {
        OnAwake,
        OnStop,
        OnDestroy,
        OnHit,
        OnHarm,
        OnAssist,
        OnHeal,
        OnInterval,
        OnCatalingRelease,
    }

    public enum EntityEvent
    {
        OnDeath,
        OnRevive,
        OnLevelUp,
        OnDamageTaken,
        OnDetectPlayer,
        OnKillPlayer,
        OnSpawned,
        OnCastStart,
        OnCastEnd,
        OnAttack,
    }

    public enum ItemEvent
    {
        OnDragStart,
        OnDragEnd,
        OnEquip,
        OnUnequip,
        OnUse,
        OnDestroy,
        OnDeplete,
        OnPickUp,
        OnDrop
    }

    public enum BuffEvents
    {
        OnStart,
        OnInterval,
        OnCleanse,
        OnComplete,
        OnEnd,
        OnDamageTaken,
        OnDamageImmune,
    }

    public enum AbilityEvent
    {
        OnCast,
        OnChannel,
        OnRelease,
        OnAbility,
    }

    public enum SpeechType
    {
        Whisper,
        Speak,
        Yell,
    }

    

    public enum CatalystParticleTarget
    {
        Self,
        Ground,
        BodyPart,
        BetweenBodyParts,
        Location
    }

    public enum CatalystTarget
    {
        Catalyst,
        Cataling,
    }

    public enum ParticleTarget
    {
        Self,
        Ground,
        BodyPart,
        BetweenBodyParts,
        Location,
        Target,
        Catalyst,
    }

    public enum PortalType
    {
        Entrance,
        Exit,
        NextLevel,
    }

    public enum NotificationEvent
    {
        OnStart,
        OnComplete,
        OnInterval,
        OnDismiss,
        OnAppear,
        OnFirstAppear,
        OnHide,
        BeforeHide,
        OnBump,
    }

    public enum AudioMixerType
    {
        SoundFX,
        Ambience,
        Music,
        Voice,
        UI,
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

    public enum BuffCategory
    {
        Neutral = 0,
        Assist = 1,
        Harm = 2,
        All = 3,
    }

    public enum CatalystType
    {
        Cast,
        Shoot,
        Melee,
        Sweep,
        Radiate
    }

    public enum RadiusType
    {
        None,
        Catalyst,
        Splash,
        Bounce,
        Cataling,
        Buff,
        Detection
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

    public enum RoomObjectType
    {
        Structure,
        Ground,
        LightSource,
        StructureLightSource,
        Paths,
        Stairs,
        BosPos,
        Prop,
        Probe,
        StructureProp,
        Tier1EnemyPos,
        Tier2EnemyPos,
        Tier3EnemyPos,
        Tier1NeutralPos,
        Tier2NeutralPos,
        Tier1ChestPos,
        Tier2ChestPos,
    }

    public enum EntityState
    {
        Active = 0,
        Stunned = 1,
        Silenced = 2,
        Immobalized = 3,
        Taunted = 4,
        MindControlled = 5,
        Immune,
        Invisible,
    }

    public enum NotificationType
    {
        Primary,
        Secondary,
        Success,
        Danger,
        Warning,
        Info,
        Light,
        Dark,
        Link
    }

    public enum MeterRecordingMode
    {
        CurrentFight,
        Dungeon
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

    public enum ClassType { Melee, Range, Caster }

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
        Use,
        RandomLocation,
        RandomBossLocation,
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
        Poor = 1,
        Common = 2,
        Uncommon = 3,
        Rare = 4,
        Epic = 6,
        Legendary = 7,
        Artifact = 8,
        Heirloom = 5,
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
        Weapon,
        Consumable,
        QuestItem,
        Catalyst,
        Currency,
        Augment,
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

    public enum BodyPart
    {
        RightHand,
        RightThumb,
        LeftHand,
        LeftThumb,
        LeftLeg,
        LeftFoot,
        RightLeg,
        RightFoot,
        Spine,
        Hips,
        Head,
        Root,
    }

    public enum WeaponType
    {
        TwoHanded,
        OneHand,
        Bow,
        Crossbow,
        Staff,
        Shield,
        Rifle,
        Pistol,
    }

    public enum ArmorType
    {
        Plate = 4,
        Mail = 3,
        Leather = 2,
        Cloth = 1,
        Gem = 0
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
