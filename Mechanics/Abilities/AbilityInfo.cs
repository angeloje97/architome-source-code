using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.UI;
using System;
using System.Threading.Tasks;
using Language.Lua;
using DungeonArchitect.Editors.LaunchPad.Actions.Impl;

namespace Architome 
{
    public enum AbilityState
    {
        Ready,
        CoolingDown,
        CheckingCast,
        MovingToRange,
        MovingToLOS,
        Casting,
        AlternateCasting,
        AugmentAbility,
    }
    public class AbilityInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        [SerializeField] int id;
        public int _id
        {
            get
            {
                return idSet ? id : 99999;
            }
        }
        bool idSet;
        public void SetId(int id, bool forceId = false)
        {
            if (idSet && !forceId) return;
            idSet = true;
            this.id = id;
        }

        public string abilityName;
        [Multiline]
        public string abilityDescription;

        public GameObject entityObject;
        public GameObject catalyst;
        public Sprite abilityIcon;

        [SerializeField] AbilityState currentState;
        public AbilityState CurrentState
        {
            get
            {
                return currentState;
            }
        }

        public List<GameObject> buffs;
        public List<ItemData> augmentsData;
        public Dictionary<AugmentItem, Augment> augments;
        //public BuffProperties buffProperties;

        public AbilityManager abilityManager;
        public EntityInfo entityInfo;
        public Movement movement;
        public CatalystInfo catalystInfo;
        public LineOfSight lineOfSight;
        public Transform sourceTransform;

        public AbilityType abilityType;
        public AbilityType2 abilityType2;
        public Role appropriateRole;
        public ClassType appropriateClassType;

        public EntityInfo target;
        public EntityInfo targetLocked;
        public Vector3 location;
        public Vector3 locationLocked;
        public Vector3 directionLocked;

        [Header("Catalyst Info")]
        public float speed = 35;
        public float range = 15;
        public float castTime = 1f;


        public AugmentProp.Restrictions restrictions;
        public AbilityRestrictionHandler restrictionHandler;
        public AugmentProp.DestroyConditions destroyConditions;
        public AugmentProp.RecastProperties recastProperties;
        public AugmentProp.Threat threat;

        [Serializable]
        public class StatsContribution
        {
            public float strength, wisdom, dexterity = 1f;
        }



        public StatsContribution coreContribution;

        [Header("Ability Properties")]

        public float knockBackValue = 1.0f;
        public float baseValue = 5.0f;
        public float value;
        public float originalCastTime = 1f;
        public float liveTime = 3;
        public int ticksOfDamage = 1;
        public float endDelay;

        [Serializable]
        public struct AbilityVisualEffects
        {
            public bool showCastBar;
            public bool showTargetIconOnTarget;
            public bool showAbilityName;
            public bool activateWeaponParticles;
        }

        public AbilityVisualEffects vfx;


        public AbilityResources resources;
        public AbilityCoolDown coolDown;


        [Header("Scanner Properties")]
        public bool scannerRadialAoe;

        [Header("Grow Properties")]
        public bool grows;
        public bool growsForward;
        public bool growsWidth;

        [Header("Return Properties")]
        public bool returnAppliesBuffs;
        public bool returnAppliesHeals;
        public bool returns;


        [Header("Hold to Cast Properties")]
        public float holdToCastValue;

        //These are depricated ways of handling movements.
        [Header("Movement Casting Properties")]
        public bool cancelCastIfMoved;
        public bool cantMoveWhenCasting;

        //[Header("AbilityRestrictions")]
        public bool activated; //If the ability is casting or channeling.

        [Header("Legacy Restrictions(Still in Use)")]
        [SerializeField] bool validate;
        public void OnValidate()
        {
            //restrictions.UpdateSelf(this);
            //UnityEditor.EditorUtility.SetDirty(this);

        }
        public bool isHealing { get { return restrictionHandler.restrictions.isHealing; } }
        public bool isAssisting { get { return restrictionHandler.restrictions.isAssisting; } }
        public bool isHarming { get { return restrictionHandler.restrictions.isHarming; } }
        public bool targetsDead { get; set; }
        public bool requiresLockOnTarget;
        public bool requiresLineOfSight;
        public bool canHitSameTarget;
        public bool canAssistSameTarget;
        public bool canBeIntercepted;
        public bool nullifyDamage;
        public bool interruptable;
        public bool isAttack;
        public bool active;

        //State if the ability can be casted.
        public bool usesWeaponAttackDamage { get; set; }

        [Header("Ability Timers")]
        public float castTimer;
        public bool canceledCast { get; set; }
        public bool isCasting;
        public bool timerPercentActivated;
        public bool alternateCastingActive;
        public bool cancelAlternateCasting;
        [SerializeField] bool useProgressMileStone;
        public float progress;
        public float progressTimer;



        [Header("Lock On Behaviors")]
        [SerializeField] private bool wantsToCast;
        public bool isAutoAttacking;

        public Action<CatalystInfo> OnCatalystRelease { get; set; }
        public Action<AbilityInfo> OnSuccessfulCast;
        public Action<AbilityInfo, bool> OnAbilityStartEnd;
        public Action<AbilityInfo> OnUpdateRestrictions;
        public Action<AbilityInfo> WhileCasting;
        public Action<AbilityInfo, List<(string, bool)>> OnReadyCheck { get; set; }
        public Action<AbilityInfo, List<bool>> OnBusyCheck { get; set; }
        public Action<AbilityInfo, EntityInfo, List<bool>, List<bool>> OnCorrectTargetCheck { get; set; }
        public Action<AbilityInfo> OnInterrupt { get; set; }
        public List<Func<Task<bool>>> augmentAbilities { get; set; }

        public Action<AbilityInfo, List<Func<Task<bool>>>> OnActivateAugmentAbilites { get; set; }
        public Action<AbilityInfo, bool> OnActiveChange { get; set; }
        public Action<AbilityInfo, int> OnChargesChange;
        public Action<AbilityInfo, ToolTipData> OnAcquireToolTip;
        public Action<AbilityInfo> OnRemoveAbility;
        public Action<AbilityInfo, List<bool>> OnAlternativeCastCheck { get; set; }
        public Action<AbilityInfo, List<Task<bool>>> OnAugmentAbilityStart;
        public Action<AbilityInfo, EntityInfo> OnHandlingTargetLocked { get; set; }
        public Action<AbilityInfo, List<bool>> OnCanShowCheck { get; set; }
        public Action<AbilityInfo> OnCanCastCheck { get; set; }
        public Action<AbilityInfo, EntityInfo, List<bool>> OnCanHarmCheck { get; set; }
        public Action<AbilityInfo, EntityInfo, List<bool>> OnCanHealCheck { get; set; }
        public Action<AbilityInfo, EntityInfo> OnCanCastAtCheck { get; set; }
        public Action<AbilityInfo, List<Func<Task>>> BeforeCastStart;
        public Action<AbilityInfo> OnCastStart { get; set; }
        public Action<AbilityInfo> OnCastEnd { get; set; }

        public float floatCheck { get; set; }
        public Action<AbilityInfo> OnSplashRadiusCheck { get; set; }

        public List<bool> checks { get; set; }
        [HideInInspector] public List<string> reasons;
        //Augments
        public AugmentChannel currentChannel { get; set; }

        AbilityManager.Events abilityEvents
        {
            get
            {
                return abilityManager.events;
            }
        }
        public void GetDependencies()
        {
            entityInfo = GetComponentInParent<EntityInfo>();


            abilityManager = GetComponentInParent<AbilityManager>();
            if (abilityManager)
            {
                abilityManager = GetComponentInParent<AbilityManager>();
                abilityManager.OnTryCast += OnTryCast;
            
            }

            if(entityInfo)
            {
                movement = entityInfo.Movement();
                lineOfSight = entityInfo.LineOfSight();

                entityObject = entityInfo.gameObject;
                entityInfo.OnChangeNPCType += OnChangeNPCType;
            }

            sourceTransform = entityInfo ? entityInfo.transform : transform;

            if(movement)
            {
                movement.OnStartMove += OnStartMove;
                movement.OnEndMove += OnEndMove;
                movement.OnTryMove += OnTryMove;
                movement.OnNewPathTarget += OnNewPathTarget;
            }

            if (catalyst)
            {
                catalystInfo = catalyst.GetComponent<CatalystInfo>();

                if (catalystInfo)
                {
                    abilityIcon = catalystInfo.catalystIcon ? catalystInfo.catalystIcon : abilityIcon;
                }

            }

            UpdateAbility();
        }
        void Start()
        {
            GetDependencies();
            HandleInitiates();
            HandleEvents();

            active = true;

            void HandleInitiates()
            {
                coolDown.Initiate(this);
                resources.Initiate(this);
                restrictionHandler.Initiate(this);
            }
        }

        void Update()
        {
            HandleDeadTarget();
        }

        public override string ToString()
        {
            if (abilityName.Length == 0)
            {
                if (name.Length == 0)
                {
                    if (catalyst)
                    {
                        return catalyst.name;
                    }
                }

                return name;

            }

            return abilityName;
        }

        public async void HandleEvents()
        {
            var activeCheck = !active;
            var chargeCheck = -1;
            while (this)
            {
                if (activeCheck != active)
                {
                    activeCheck = active;
                    OnActiveChange?.Invoke(this, active);
                }

                if (chargeCheck != coolDown.charges)
                {
                    chargeCheck = coolDown.charges;
                    OnChargesChange?.Invoke(this, coolDown.charges);
                }

                await Task.Yield();
            }
        }
        public void OnStartMove(Movement movement)
        {

            if (cancelCastIfMoved && isCasting)
            {
                CancelCast("Moved On Cast");
            }
        }
        public void OnEndMove(Movement movement)
        {

        }
        public void OnTryCast(AbilityInfo ability)
        {
            if (!ability.IsReady()) return;
            if (ability.isAttack) { return; }
            if(isAttack)
            {
                isAutoAttacking = false;


                if(isCasting)
                {
                    CancelCast("a non attack ability is trying to be casted.");
                }
            }
        }
        public void OnTryMove(Movement movement)
        {
            if (isAutoAttacking)
            {
                isAutoAttacking = false;
            }

            if (wantsToCast)
            {
                DeactivateWantsToCast("Player Try Moving", true);
            }

            if (isCasting)
            {
                CancelCast("Player tried to move");
            }

        
        }
        public void OnChangeNPCType(NPCType before, NPCType after)
        {
            if(WantsToCast())
            {
                DeactivateWantsToCast("From NPCType change", true);
                ClearTargets();
            }
        }
        public void OnNewPathTarget(Movement movement, Transform before, Transform after)
        {
            if (abilityType != AbilityType.LockOn) return;
            if(target && after == target.transform) { return; }
            if (wantsToCast)
            {
                DeactivateWantsToCast("From New Path Target", true);
            }

            if (isAutoAttacking)
            {
                CancelCast("New Path Target");
            }


            ClearTargets();
            isAutoAttacking = false;
        

            //if(target != null && after != target.transform)
            //{
            //    if (wantsToCast)
            //    {
            //        DeactivateWantsToCast();
                
            //    }

            //    if(isAttack)
            //    {
            //        target = null;
            //        targetLocked = null;
            //        isAutoAttacking = false;
            //    }
            //}
        }
        // Augments
        public async void AddAugment(AugmentItem augmentItem)
        {
            if (augments == null) augments = new();
            if (augments.ContainsKey(augmentItem))
            {

                await Task.Yield();

                if (augments.ContainsKey(augmentItem))
                {
                    return;
                }
            }

            var newAugment = Instantiate(augmentItem.augment, transform);

            newAugment.spawnedByItem = true;        

            newAugment.gameObject.name = augmentItem.itemName;
        
            augments.Add(augmentItem, newAugment);
        }

        public void RemoveAugment(AugmentItem augmentItem)
        {
            if (!augments.ContainsKey(augmentItem)) return;
            augments[augmentItem].RemoveAugment();

            augments.Remove(augmentItem);

        }
        public string Name()
        {
            return ToString();
        }

        public Sprite Icon()
        {
            if (abilityIcon != null)
            {
                return abilityIcon;
            }


            if (catalyst)
            {
                var info = catalyst.GetComponent<CatalystInfo>();

                return info.Icon();
            }

            return abilityIcon;
        }
        public string Description()
        {
            var description = "";

            if (abilityDescription != null && abilityDescription.Length > 0)
            {
                description += $"{abilityDescription}\n";
            }

            return description;
        }
        public string PropertiesDescription()
        {
            string properties = "";

            if (!nullifyDamage && (isHarming || isHealing))
            {
            
                if (isHealing)
                {
                    properties = "Heals health ";
                    if (isHarming)
                    {
                        properties += "or ";
                    }
                }

                if (isHarming)
                {
                    properties += "Deals damage ";
                }

                properties += "equal to ";

                if (entityInfo)
                {
                    properties += $"{value} value. ";
                }

                var listString = new List<string>();

                if (coreContribution.dexterity > 0)
                {
                    listString.Add($"{ArchString.FloatToSimple(coreContribution.dexterity*100)}% Dexterity");
                }

                if (coreContribution.strength > 0)
                {
                    listString.Add($"{ArchString.FloatToSimple(coreContribution.strength*100)}% Strength");
                }

                if (coreContribution.wisdom > 0)
                {
                    listString.Add($"{ArchString.FloatToSimple(coreContribution.wisdom*100)}% Wisdom");
                }

            

                properties += $"({ArchString.StringList(listString)}).\n";
            }

            if (ticksOfDamage > 1)
            {
                properties += $"Base Ticks: {ticksOfDamage}";
            }

            return properties;
        }
        public string ResourceDescription()
        {
            string resourceDescription = "";
            var info = catalyst.GetComponent<CatalystInfo>();

            if (vfx.showCastBar && info)
            {
                var castTime = info.castTime;

                var alteredCastTime = castTime - (castTime * Haste());
                resourceDescription += $"{ArchString.FloatToSimple(alteredCastTime)}s Cast ";
            }

            if (resources.requiredAmount > 0 || resources.requiredPercent > 0)
            {
                var requiredAmount = $"";
                if (resources.requiredAmount > 0)
                {
                    requiredAmount += $"{resources.requiredAmount}";

                    if (resources.requiredPercent > 0)
                    {
                        requiredAmount += " + ";
                    }
                }

                if (resources.requiredPercent > 0)
                {
                    requiredAmount += $"({resources.requiredPercent * 100}%)";
                }

                resourceDescription += $"{requiredAmount} mana required ";
            }

            if (resources.producedAmount > 0 || resources.producedPercent > 0)
            {
                var producedAmount = "";

                if (resources.producedAmount > 0)
                {
                    producedAmount += $"{resources.producedAmount}";

                    if (resources.producedPercent > 0)
                    {
                        producedAmount += " + ";
                    }
                }

                if (resources.producedPercent > 0)
                {
                    producedAmount += $"({resources.producedPercent*100}%)";
                }

                resourceDescription += $"{producedAmount} mana produced ";
            }

            if (resourceDescription.Length > 0)
            {
                resourceDescription += "\n";
            }

            return resourceDescription;
        }
        public string RestrictionDescription()
        {
            var properties = "";

        

            properties += restrictionHandler.Description();

            UpdateFromCatalyst();

            return properties;

            void UpdateFromCatalyst()
            {
                if (catalyst == null) return;
                var info = catalyst.GetComponent<CatalystInfo>();
                if (info == null) return;
                if (!info.requiresWeapon) return;

                properties += $"Requires weapon : {ArchString.CamelToTitle(info.weaponType.ToString())}.\n";

            }
        }
        public string BuffList()
        {
            var buffDescription = "";

            if (buffs.Count > 0)
            {
                var assistBuffs = new List<BuffInfo>();
                var harmBuffs = new List<BuffInfo>();
                var selfBuffs = new List<BuffInfo>();

                foreach (var buff in buffs)
                {
                    var info = buff.GetComponent<BuffInfo>();
                    if (info == null) continue;

                    var list = assistBuffs;


                    if (info.buffTargetType == BuffTargetType.Harm)
                    {
                        list = harmBuffs;
                    }

                    list.Add(info);
                }

                if (assistBuffs.Count > 0)
                {
                    buffDescription += $"Applies buff(s): ";

                    var list = new List<string>();

                    foreach (var buff in assistBuffs)
                    {
                        list.Add(buff.name);
                    }


                    buffDescription += $"{ArchString.StringList(list)}\n";
                }

                if (harmBuffs.Count > 0)
                {
                    buffDescription += $"Applies debuff(s) to enemies: ";

                    var list = new List<string>();

                    foreach (var buff in harmBuffs)
                    {
                        list.Add(buff.name);
                    }

                    buffDescription += $"{ArchString.StringList(list)}\n";
                }

                if (selfBuffs.Count > 0)
                {
                    buffDescription += $"Applies buff(s) to self: ";

                    var list = new List<string>();

                    foreach (var buff in selfBuffs)
                    {
                        list.Add(buff.name);
                    }


                    buffDescription += $"{ArchString.StringList(list)}\n";
                }
            }

            return buffDescription;
        }
        public string BuffDescriptions()
        {
            var buffDescription = "";
            var entityExists = entityInfo != null;
            foreach (var buff in buffs)
            {
                var info = buff.GetComponent<BuffInfo>();

                if (info == null) continue;

                var time = info.properties.time > 0 ? $"({info.properties.time}s)" : "";

                var name = info.name + time;

                var generalDescription = "";
            

                if (!entityExists)
                {
                    generalDescription += info.TypesDescriptionGeneral();
                }
                else
                {
                    generalDescription += info.TypeDescriptionFace(value);
                }

                generalDescription += $"{info.PropertiesDescription()}";

                if (generalDescription.Length == 0) continue;

                buffDescription += $"{name}: {generalDescription}";
            }

            return buffDescription;
        }

        public string AugmentDescriptions()
        {
            var result = "";

            foreach (Transform child in transform)
            {
                var augment = child.GetComponent<Augment>();
                if (augment == null) continue;
                if (augment.spawnedByItem) continue;
                var description = "";

                if(!augment.name.Trim().Equals(""))
                {
                    description += $"{augment.name} : ";
                }

                description += $"{augment.TypeDescription()}";

                if (result.Length > 0 && description.Length > 0)
                {
                    result += "\n";
                }

                result += description;
            }

            return result;
        }
        public ToolTipData ToolTipData(bool showResources = true)
        {
            var attributesList = new List<string>()
            {
                PropertiesDescription(),
                BuffDescriptions(),
                AugmentDescriptions(),
            };

            var attributes = "";

            foreach (var attribute in attributesList)
            {
                if (attributes.Length > 0 && attribute.Length > 0)
                {
                    attributes += "\n";
                }

                attributes += $"{attribute}";
            }

            var value = showResources ? ResourceDescription() : "";

            var newToolTip = new ToolTipData() {
                icon = Icon(),
                subeHeadline = ArchString.CamelToTitle(abilityType2.ToString()),
                name = Name(),
                description = Description(),
                attributes = attributes,
                requirements = RestrictionDescription(),
                value = value
            };

            OnAcquireToolTip?.Invoke(this, newToolTip);

            return newToolTip;

        }
        //async void OnGlobalCoolDown(AbilityInfo ability)
        //{
        //    if (!coolDown.usesGlobal) return;
        //    if (coolDown.globalCoolDownActive) return;
        //    if (coolDown.charges == 0) return;


        //    coolDown.globalCoolDownActive = true;

        //    float timer = 1 - Haste() * 1;


        //    while (timer > 0)
        //    {
        //        await Task.Yield();
        //        timer -= Time.deltaTime;
        //        coolDown.progress = 1 - timer; 
        //    }


        //    coolDown.progress = 1;

        //    coolDown.globalCoolDownActive = false;
        
        //}
        async public Task Cast()
        {
            if (isAttack)
            {
                isAutoAttacking = true;


                while (isAutoAttacking)
                {
                    if (!isAutoAttacking) break;
                    var success = await Activate();

                    if (!success)
                    {
                        break; 
                    }
                    await coolDown.NextAvailableCharge();

                }
            }
            else
            {
                await Activate();
            }

            SetState(coolDown.CurrentIdleState());
        }
        public void Recast()
        {
            if (!recastProperties.isActive) return;
            if (recastProperties.maxRecast == -1)
            {
                recastProperties.currentRecast += 1;
                recastProperties.OnRecast?.Invoke(this);
                return;
            }

            if (recastProperties.currentRecast < recastProperties.maxRecast) return;

            recastProperties.currentRecast += 1;

            recastProperties.OnRecast?.Invoke(this);
        }
        async void SetRecast()
        {
            if (!recastProperties.enabled) return;
            if (recastProperties.isActive) return;

            recastProperties.isActive = true;

            var recastTimer = recastProperties.recastTimeFrame;

            while (recastTimer > 0)
            {
                await Task.Yield();
                recastTimer -= Time.deltaTime;

            }

            recastProperties.isActive = false;

        }
        bool SpawnHasLineOfSight()
        {
            if (abilityType != AbilityType.Spawn) return true;

        

            if (locationLocked == new Vector3()) return false;

            var distance = Vector3.Distance(locationLocked, transform.position);

            if (distance > catalystInfo.range)
            {
                locationLocked = Vector3.Lerp(transform.position, locationLocked, catalystInfo.range / distance);
            }

            if (!lineOfSight) return true;
            if (!requiresLineOfSight) return true;

        


            if (!lineOfSight.HasLineOfSight(locationLocked))
            {

                locationLocked = V3Helper.InterceptionPoint(locationLocked, transform.position, LayerMasksData.active.structureLayerMask, 2f);
            }


            return true;
        }
        async Task<bool> HandleRequiresTargetLocked()
        {
            if (location.x == 0 && location.z == 0)
            {
                location = Mouse.RelativePosition(entityObject.transform.position);
            }

            locationLocked = location;

            if (abilityType != AbilityType.LockOn) return true;

            OnHandlingTargetLocked?.Invoke(this, target);

            if (!target)
            {
                return false;
            }

            if (target)
            {
                targetLocked = target;
                if (!IsCorrectTarget(targetLocked)) { return false; }
                if (targetLocked == null) { return false; }
            }


            if (!await IsInRange()) return false;
            if (!await HasLineOfSight()) return false;

            return true;
        }

        public async Task HandleTargetLocation(Action<Vector3> action)
        {
            if (!activated) return;
            EntityInfo currentTarget = abilityType == AbilityType.LockOn ? targetLocked : null;
            Vector3 currentLocation;
            while (activated)
            {
                currentLocation = locationLocked;
                UpdateTargetLocation(currentTarget);
                action?.Invoke(currentLocation);

                await Task.Yield();

            }

            void UpdateTargetLocation(EntityInfo target)
            {
                if (target == null) return;
                currentLocation = target.transform.position;
            }
        }


        #region Flags
        async Task<bool> HasLineOfSight(bool moveToTarget = true)
        {
            if (range == -1) return true;
            if (!lineOfSight)
            {
                return true;
            }

            if (!requiresLineOfSight)
            {
                return true;
            }

            if (abilityType == AbilityType.LockOn && target == null) return false;


            if (!entityInfo.CanSee(target.transform))
            {
                if (movement && moveToTarget)
                {
                    Debugger.Combat(7562, $"Moving closer to target because no line of sight");
                    SetState(AbilityState.MovingToLOS);
                    var result = await movement.MoveToAsyncLOS(target.transform);
                    return result;
                }
                return false;
            }
            return true;
        }
        async Task<bool> IsInRange(bool useOffset = false, bool moveToTarget = true)
        {
            if (range == -1)
            {
                return true;
            }

            var offset = useOffset ? range * .25f : 0;

            if (abilityType == AbilityType.LockOn && target == null) return false;

            if (V3Helper.Distance(target.transform.position, entityObject.transform.position) > range + offset)
            {
                if (moveToTarget)
                {
                    Debugger.Combat(7561, $"Moving closter to target because out of range");
                    SetState(AbilityState.MovingToRange);
                    var result = await movement.MoveToAsync(target.transform, range - 1f);
                    return result;
                }
                //ActivateWantsToCast("Target is out of range");
                return false;
            }
            return true;
        }
        public bool IsBusy()
        {
            if (isCasting) return true;

            var busyList = new List<bool>();

            OnBusyCheck?.Invoke(this, busyList);


            foreach (var busy in busyList)
            {
                if (busy) return true;
            }

            return false;
        }
        async Task<bool> CanCast()
        {
            if (!IsReady())
            {
                CantCastReason("Not Ready");
                return false;
            }

            if (!await AbilityManagerCanCast())
            {
                CantCastReason("Ability Manager Can't Cast");
                return false;
            }
            if (!await HandleRequiresTargetLocked())
            {
                CantCastReason("Incorrect Target");
                return false;
            }

            if (!SpawnHasLineOfSight())
            {
                CantCastReason("Don't have line of sight");
                return false;
            }

            checks = new();
            reasons = new();
            OnCanCastCheck?.Invoke(this);

            foreach(var check in checks)
            {
                if (!check)
                {
                    CantCastReason(ArchString.NextLineList(reasons));
                    return false;
                }
            }



            return true;

            async Task<bool> AbilityManagerCanCast()
            {
                var currentAbility = abilityManager.currentlyCasting;
                if (!currentAbility) return true;
                if (!currentAbility.isAttack) return false;

                currentAbility.isAutoAttacking = false;

                while (currentAbility.activated) await Task.Yield();

                return true;

            }

            void CantCastReason(string reason)
            {
                Debugger.InConsole(5329, $"{this} can't cast because {reason}");
            }
        }

        public bool IsChanneling()
        {
            if (currentChannel == null) return false;

            return currentChannel.active;
        }
        public bool CanCastAt(GameObject target)
        {
            if (entityInfo.CanAttack(target) && isHarming)
            {
                return true;
            }

            if (entityInfo.CanHelp(target) && (isHealing || isAssisting))
            {
                return true;
            }

            return false;
        }

        public bool CanCastAt(EntityInfo target)
        {
            return IsCorrectTarget(target);
        }
        public bool AbilityIsInRange(GameObject target)
        {
            if (catalystInfo == null) return false;
            var distance = catalystInfo.range;
            if (target == null) return false;

            if (V3Helper.Distance(target.transform.position, entityObject.transform.position) <= distance)
            {
                return true;
            }


            return false;
        }
        public bool IsCorrectTarget(EntityInfo target)
        {
            var info = target;
            if(target == null) { return true; }

            var orChecks = new List<bool>();
            var andChecks = new List<bool>();

            OnCorrectTargetCheck?.Invoke(this, target, orChecks, andChecks);
            foreach(var check in orChecks) { if (check) return true; }
            foreach(var check in andChecks) { if (!check) return false; }

            if (CanHarm(target)) return true;
            if (CanHeal(target)) return true;

            return false;
        }
        public bool IsReady()
        {
            if (!active)
            {
                NotReadyReason("not active");
                return false;
            }

            if (IsBusy())
            {
                NotReadyReason("Already casting or already channeling");
                return false;
            }
            if (!HasCorrectWeapon())
            {
                return false;
            }

            var readyList = new List<(string, bool)>();

            OnReadyCheck?.Invoke(this, readyList);

            foreach (var (reason, success) in readyList)
            {
                if (!success)
                {
                    NotReadyReason(reason);
                    return false;

                }
            }

            return true;

            bool HasCorrectWeapon()
            {
                if (!catalystInfo.requiresWeapon) return true;
            
                var hasWeapon = abilityManager.HasWeaponType(catalystInfo.weaponType);

                if (hasWeapon == false)
                {
                    NotReadyReason($"Ability requires {catalystInfo.weaponType}");
                }

                return hasWeapon;
            }

            void NotReadyReason(string reason)
            {
                Debugger.InConsole(5330, $"{this} can't cast because {reason}");
            }
        }
        public bool CanHarm(EntityInfo target)
        {
            var checks = new List<bool>();

            OnCanHarmCheck?.Invoke(this, target, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }

        public bool CanHeal(EntityInfo target)
        {
            var checks = new List<bool>();
            OnCanHealCheck?.Invoke(this, target, checks);

            foreach (var check in checks)
            {
                if (check) return true;
            }

            return false;
        }
        public bool CanShow()
        {
            var checks = new List<bool>();

            OnCanShowCheck?.Invoke(this, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }
        #endregion
        public bool EndCast(bool checkRangeLos)
        {
            abilityEvents.OnCastRelease?.Invoke(this);
            HandleFullHealth();
            HandleAbilityType();
            SetRecast();

            return true;

            void HandleFullHealth()
            {
                if (!target) return;
                if (isHealing && entityInfo.CanHelp(target))
                {
                    if (target.health == target.maxHealth)
                    {
                        isAutoAttacking = false;
                    }
                }
            }

        }
        public void CancelCast(string reason = "")
        {
            if (!isCasting) return;

            LogCancel(reason);

            if(abilityManager)
            {
                abilityManager.OnCancelCast?.Invoke(this);
            }


            canceledCast = true;
        }

        #region WantsToCast
        //public void UseMana()
        //{
        //    if(entityInfo)
        //    {
        //        entityInfo.Use(resources.requiredAmount + entityInfo.maxMana*resources.requiredPercent);
        //    }
        //}
        public void ActivateWantsToCast(string reason)
        {
            if (wantsToCast) return;
            if (coolDown.charges <= 0) return;
            Debugger.InConsole(896537, $"{reason}");
            wantsToCast = true;
            abilityManager.wantsToCastAbility = wantsToCast;
            abilityManager.currentWantsToCast = this;
            abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);
        }
        public bool WantsToCast()
        {
            return wantsToCast;
        }
        public void DeactivateWantsToCast(string reason, bool clearTargets = false)
        {
            if (!wantsToCast) return;

            Debugger.Combat(4389645, $"{reason}");
            wantsToCast = false;
            if (abilityManager.currentWantsToCast == this)
            {
                abilityManager.currentWantsToCast = null;
            }
            abilityManager.wantsToCastAbility = wantsToCast;
            abilityManager.OnWantsToCastChange?.Invoke(this, wantsToCast);

            if (clearTargets)
            {
                ClearTargets();
            }
        }
        #endregion
        public void UpdateAbility()
        {
            if (abilityManager)
            {
                if (entityInfo)
                {
                    if (usesWeaponAttackDamage && abilityManager.currentWeapon)
                    {
                        value = abilityManager.currentWeapon.attackValue;
                    }
                    else
                    {
                        value = AbilityValue();
                    }
                

                }
            }

        
            if (catalyst)
            {
                catalystInfo = catalyst.GetComponent<CatalystInfo>();

                abilityIcon = catalystInfo.catalystIcon;
                range = catalystInfo.range;
                speed = catalystInfo.speed;

                var isAttack = abilityType2 == AbilityType2.AutoAttack;

                if(isAttack && entityInfo)
                {
                    var attackInterval = 1 / entityInfo.stats.attackSpeed;

                    if(catalystInfo.catalystType == CatalystType.Melee)
                    {
                        castTime = attackInterval *.25f;
                        coolDown.timePerCharge = attackInterval *.75f;
                    }
                    else
                    {
                        castTime = attackInterval *.25f;
                        coolDown.timePerCharge = attackInterval *.75f;
                    }
                }
                else
                {
                    castTime = catalystInfo.castTime;
                }
                originalCastTime = castTime;

                if (abilityType == AbilityType.SkillShot)
                {
                    requiresLineOfSight = false;
                    requiresLockOnTarget = false;
                }

                if (abilityType == AbilityType.LockOn)
                {
                    requiresLineOfSight = true;
                    requiresLockOnTarget = true;
                }

            }

            if (abilityManager)
            {
                abilityManager.OnAbilityUpdate?.Invoke(this);

            }
        }

        float AbilityValue()
        {
            var value = baseValue
                            + entityInfo.stats.Wisdom * coreContribution.wisdom
                            + entityInfo.stats.Strength * coreContribution.strength
                            + entityInfo.stats.Dexterity * coreContribution.dexterity;

            return value;
        }
        //public void HandleAutoAttack()
        //{
        //    if(isAutoAttacking)
        //    {
        //        Cast();
        //    }
        //}
        public void HandleDeadTarget()
        {
            if (abilityType != AbilityType.LockOn) return;
            if (targetsDead) return;

            if(target)
            {
                if(!target.isAlive)
                {
                    CancelCast("Cannot Target Dead");
                    DeactivateWantsToCast("Target is Dead", true);
                }
            }
        }
        public async void Use()
        {
            if(abilityManager)
            {
                await abilityManager.Cast(this, true);
                abilityManager.OnTryCast?.Invoke(this);
            }
        }
        public async void CastAt(Vector3 position)
        {
            if (IsBusy()) return;
            if (GetComponentInParent<AbilityManager>() == null) { return; }
            location = position;
            await Cast();
        }
        public async Task CastAt(EntityInfo target)
        {
            if (IsBusy()) return;
            if(abilityManager == null) { return; }
            this.target = target;
            await Cast();
        }
        public async Task<bool> Activate()
        {
            if (!await CanCast()) return false;
            if (activated) return false;
            DeactivateWantsToCast("From Start Cast");
            activated = true;

            abilityManager.OnAbilityStart?.Invoke(this);
            abilityManager.currentlyCasting = this;
            OnAbilityStartEnd?.Invoke(this, true);

            bool success = await Casting();

            if (success)
            {
                OnSuccessfulCast?.Invoke(this);
                if(abilityType2 == AbilityType2.AutoAttack)
                {
                    abilityEvents.OnAttack?.Invoke(this);
                }
                await AugmentAbilities();
                await Task.Delay((int)(1000 * endDelay));
                SetRecast();
            }


            if (abilityManager.currentlyCasting == this)
            {
                abilityManager.currentlyCasting = null;
                abilityManager.OnAbilityEnd?.Invoke(this);
            }

            OnAbilityStartEnd?.Invoke(this, false);
        

            if (!isAutoAttacking)
            {
                ClearTargets();
            }
        

            activated = false;
            
            return success;

            async Task<bool> Casting()
            {
                var success = true;
                float timeElapsed = 0f;
                canceledCast = false;
                SetState(AbilityState.Casting);
                
                if (UseAlternateCasting())
                {
                    BeginCast();
                    (success, timeElapsed) = await AlternateCasting();
                    EndCasting();
                    return success;
                }


                var timer = castTime - castTime * Haste();
                var startTime = timer;

                progress = 0;
                var progressMileStone = .25f;

                await HandleTasksBeforeCasting();
                BeginCast();

                while (timer > 0)
                {
                    await Task.Yield();
                    timer -= Time.deltaTime;
                    timeElapsed += Time.deltaTime;
                    

                    WhileCastingAbility();
                    Func<bool> canContinue = CanContinue;
                    progress = 1 - (timer / startTime);
                    progressTimer = timer;

                    if (useProgressMileStone)
                    {

                        if (progress > progressMileStone)
                        {
                            if (!canContinue())
                            {
                                success = false;
                                break;
                            }

                            progressMileStone += .25f;
                        }
                    }
                    else
                    {
                        if (!canContinue())
                        {
                            success = false;
                            break;
                        }
                    }

                    
                }


                EndCasting();

                return success;

                void BeginCast()
                {
                    isCasting = true;
                    timerPercentActivated = false;
                    abilityEvents.OnCastStart?.Invoke(this);
                    OnCastStart?.Invoke(this);
                }

                async Task HandleTasksBeforeCasting()
                {
                    List<Func<Task>> tasksBeforeCasting = new();
                    abilityEvents.BeforeCastStart?.Invoke(this, tasksBeforeCasting);
                    BeforeCastStart?.Invoke(this, tasksBeforeCasting);
                    foreach(var task in tasksBeforeCasting)
                    {
                        await task();
                    }
                }

                void EndCasting()
                {
                    if (success)
                    {
                        success = EndCast(abilityType == AbilityType.LockOn && abilityType2 != AbilityType2.AutoAttack && timeElapsed > .50f);
                    }

                    abilityEvents.OnCastEnd?.Invoke(this);
                    isCasting = false;
                    timerPercentActivated = false;
                    OnCastEnd?.Invoke(this);
                
                }

                void WhileCastingAbility()
                {

                    abilityManager.WhileCasting?.Invoke(this);
                    WhileCasting?.Invoke(this);
                    if (isAttack) return;
                    if (targetLocked != target)
                    {
                        targetLocked = target;
                    }
                    HandleTimerPercent();
                    HandleTracking();

                

                    void HandleTimerPercent()
                    {
                        if (timerPercentActivated) return;
                        if (progress < catalystInfo.releasePercent) return;

                        timerPercentActivated = true;


                        abilityEvents.OnCastReleasePercent?.Invoke(this);
                    }
                }

                bool UseAlternateCasting()
                {
                    var checks = new List<bool>();

                    OnAlternativeCastCheck?.Invoke(this, checks);

                    foreach (var check in checks)
                    {
                        if (check)
                        {
                            return true;

                        }
                    }

                    return false;
                }

                async Task<(bool, float)> AlternateCasting()
                {
                    var timeElapsed = 0f;
                    alternateCastingActive = true;
                    cancelAlternateCasting = false;

                    var successful = true;
                    SetState(AbilityState.AlternateCasting);
                    while (alternateCastingActive)
                    {
                        if (cancelAlternateCasting)
                        {
                            successful = false;
                        }

                        timeElapsed += Time.deltaTime;
                        await Task.Yield();
                    }

                    return (successful, timeElapsed) ;
                }
            }

            async Task AugmentAbilities()
            {
                augmentAbilities = new();
                OnActivateAugmentAbilites?.Invoke(this, augmentAbilities);
                if (augmentAbilities.Count == 0) return;
                SetState(AbilityState.AugmentAbility); 

                foreach(var augment in augmentAbilities)
                {
                    var success = await augment();
                    if (!success) break;
                }
            }
        }
        public async Task<AbilityInfo> EndActivation()
        {
            while (activated)
            {
                await Task.Yield();
            }

            return this;
        }
        public async Task<AbilityInfo> EndActivation(Action<AbilityInfo> action)
        {
            while (activated)
            {
                action(this);
                await Task.Yield();
            }

            return this;
        }

        public async Task<AbilityInfo> EndCasting()
        {
            while (isCasting) await Task.Yield();
            return this;
        }
        public void HandleAbilityType()                                 //This is where the magic happens.. IE where the catalyst gets released.
        {

            if (catalystInfo)
            {
                catalystInfo.abilityInfo = this;
                catalystInfo.isCataling = false;
            }
            else
            {
                return;
            }


            switch (abilityType)
            {
                case AbilityType.SkillShot: FreeCast(); break;
                case AbilityType.Spawn: Spawn(); break;
                case AbilityType.LockOn: LockOn(); break;
                case AbilityType.Use: Use(); break;
                default: FreeCast(); break;
            }

            void Spawn()
            {
                if (!catalyst)
                {
                    return;
                }
                directionLocked = V3Helper.Direction(location, entityObject.transform.position);

                var heightFromGround = V3Helper.HeightFromGround(entityObject.transform.position, GMHelper.LayerMasks().walkableLayer);
                var groundPos = V3Helper.GroundPosition(locationLocked, GMHelper.LayerMasks().walkableLayer);
                locationLocked.y = groundPos.y + heightFromGround;


                var newCatalyst = Instantiate(catalystInfo, locationLocked, transform.localRotation);



                newCatalyst.transform.rotation = V3Helper.LerpLookAt(newCatalyst.transform, locationLocked, 1f);


                abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
                OnCatalystRelease?.Invoke(newCatalyst);
            }

            void FreeCast()
            {
                if(!catalyst)
                {
                    return;
                }
                directionLocked = V3Helper.Direction(location, transform.position);
                var heightFromGround = V3Helper.HeightFromGround(transform.position, GMHelper.LayerMasks().walkableLayer);
                var groundPos = V3Helper.GroundPosition(locationLocked, GMHelper.LayerMasks().walkableLayer);
                locationLocked.y = groundPos.y + heightFromGround;

                var newCatalyst = Instantiate(catalystInfo, transform.position, transform.localRotation);



                abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
                OnCatalystRelease?.Invoke(newCatalyst);


            }
            void LockOn()
            {
                if(!catalyst)
                {
                    return;
                }
                var newCatalyst = Instantiate(catalystInfo, sourceTransform.position, transform.localRotation);


                abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
                OnCatalystRelease?.Invoke(newCatalyst);
            }
            void Use()
            {
                if (!catalyst) { return; }
                var newCatalyst = Instantiate(catalystInfo, sourceTransform.position, transform.localRotation);


                abilityManager.OnCatalystRelease?.Invoke(this, newCatalyst);
                OnCatalystRelease?.Invoke(newCatalyst);
            }
        }
        public bool HasChannel()
        {
            return GetComponentInChildren<AugmentChannel>() != null;
        }
        public float Haste()
        {
            if (entityInfo)
            {
                return entityInfo.stats.haste;
            }

            return 0;
        }
        public bool CanContinue()
        {
            Debugger.Combat(5915, $"Checking can continue interval for {this}");
            if (canceledCast && isCasting)
            {
                LogCancel("Canceled Cast");
                canceledCast = false;
                return false;
            }

            if (abilityManager.currentlyCasting != this && isAttack)
            {
                LogCancel("Not equal to ability manager currently casting");
                return false;
            }

            if (targetLocked && abilityType == AbilityType.LockOn)
            {
                if (!IsCorrectTarget(targetLocked) || target == null)
                {
                    LogCancel("Incorrect Target");
                    return false;
                }
            }



            if (!entityInfo.isAlive)
            {
                LogCancel("Entity Died");
                return false;
            }

        
            return true;

        
        }
        void LogCancel(string reason)
        {
            Debugger.Combat(9536, $"{entityInfo} stopped casting {this} because {reason}");
        }
        public void ClearTargets()
        {
            if (wantsToCast) return;
            target = null;
            targetLocked = null;
        }
        public void HandleTracking()
        {
            /*
             * This function has been implemented as AugmentTracking.
             */
        }
        public void RemoveSelf()
        {
            OnRemoveAbility?.Invoke(this);
            Destroy(gameObject);
        }
        public float Radius(RadiusType type)
        {
            return type switch
            {
                RadiusType.Catalyst => catalystInfo.range,
                RadiusType.Buff => BuffRadius(),
                RadiusType.Splash => SplashRadius(),
                RadiusType.Detection => lineOfSight != null ? lineOfSight.radius: 0f,
                RadiusType.CatalystSize => catalyst.transform.localScale.x,
                _ => 0f,
            };

            float BuffRadius()
            {
                float largest = 0;
                foreach (var buff in buffs)
                {
                    var info = buff.GetComponent<BuffInfo>();

                    if (info.properties.radius > largest)
                    {
                        largest = info.properties.radius;
                    }
                }

                return largest;
            }

            float SplashRadius()
            {
                floatCheck = 1f;
                OnSplashRadiusCheck?.Invoke(this);
                return floatCheck;
            }
        }

        public void SetState(AbilityState newState)
        {
            currentState = newState;
        }

        public async void CatalystAction(Action<CatalystInfo> action)
        {
            if(catalystInfo == null)
            {
                await Task.Delay(0625);
                if (!Application.isPlaying) return;
                if (catalystInfo == null) return;
            }
            action?.Invoke(catalystInfo);

        }
    }


}