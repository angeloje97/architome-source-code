using Architome.Enums;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using UltimateClean;

namespace Architome
{
    [Serializable]
    [RequireComponent(typeof(TargetableEntity))]
    public class EntityInfo : MonoActor
    {
        #region Identity
        [SerializeField] int id;
        public int _id
        {
            get
            {
                return id != 0 ? id : 99999999;
            }

        }
        [SerializeField] bool idSet;
        public int SaveIndex { get { return saveIndex; } set { saveIndex = value; } }
        public void SetId(int id, bool forceSet = false)
        {
            if (idSet && !forceSet) return;
            idSet = true;
            this.id = id;
        }

        int saveIndex = -1;

        [Serializable]
        public class Properties
        {
            public bool unlocked, custom, created;
        }

        public Properties properties;
        #endregion

        #region Common Data
        public string entityName;
        [Multiline]
        public string entityDescription;
        public ArchClass archClass;
        [SerializeField] Sprite entityPortrait;
        public EntityFXPack entityFX;

        public EntityControlType entityControlType;
        [SerializeField] public NPCType npcType;
        [Header("Entity Properties")]

        public EntityRarity rarity;
        public List<string> objectives;
        public bool isAlive;
        public bool isInCombat;
        public bool isHidden = true;
        public bool isRegening = false;
        public bool disappeared = false;
        public bool isHover = false;
        public Transform target { get; private set; }
        public EntityInfo combatTarget;
        public Role role;
        public RoomInfo currentRoom;
        public List<EntityState> stateImmunities;
        public List<EntityState> states;
        public WorkerState workerState;

        #endregion


        [Serializable]
        public struct SummonedEntity
        {
            public bool isSummoned;
            public EntityInfo master;
            public AbilityInfo sourceAbility;
            public float timeRemaining;

            public bool diesOnMasterDeath, diesOnMasterCombatFalse;

            public void SetSummoned(SpawnerInfo.SummonData summonData)
            {
                isSummoned = true;

                master = summonData.master;

                timeRemaining = summonData.liveTime;

                diesOnMasterDeath = summonData.masterDeath;
                diesOnMasterCombatFalse = summonData.masterCombatFalse;
            }
        }
        public SummonedEntity summon;

        public PresetStats presetStats;
        public bool fixedStats;
        public Role roleFixed;
        public Stats entityStats;
        public Stats stats;


        [Header("Resources")]
        public float maxHealth;
        public float maxMana;
        public float shield;
        public float healAbsorbShield;
        public float health;
        public float mana;

        [Header("Regen Percent")]
        public float healthRegenPercent = .01f;
        public float manaRegenPercent = .01f;

        [Header("Experience")]
        public bool canLevel;

        public LayerMask walkableLayer;

        [SerializeField]
        ComponentManager components;


        #region Events
        public struct PartyEvents
        {
            public Action<GroupFormationBehavior> OnRotationFormationStart { get; set; }
            public Action<GroupFormationBehavior> OnRotateFormationEnd { get; set; }

            public Action<PartyInfo> OnSelectedAction { get; set; }
            public Action<PartyInfo> OnAddedToParty { get; set; }
            public Action<PartyInfo> OnRemovedFromParty { get; set; }

            public PartyInfo currentParty { get; set; }
        }

        public struct SceneEvents
        {
            public Action<string> OnTransferScene { get; set; }
        }

        public  Action<CombatEventData> OnDamageTaken { get;set; }
        public Action<CombatEventData> OnDamageDone { get; set; }
        public Action<CombatEventData> OnHealingTaken { get; set; }
        public Action<CombatEventData> OnHealingDone { get; set; }
        public Action<CombatEventData> OnReviveOther { get; set; }
        public Action<CombatEventData> OnReviveThis { get; set; }
        public event Action<CombatEventData> OnDeath;
        public event Action<CombatEventData> OnKill;
        public Action<CombatEventData> OnDamagePreventedFromShields;
        public Action<BuffInfo, EntityInfo> OnNewBuff;
        public Action<BuffInfo, EntityInfo> OnBuffApply;
        public Action<float> OnExperienceGain;
        public Action<object, float> OnExperienceGainOutside;
        public Action<int> OnLevelUp;
        public Action<float, float, float> OnHealthChange { get; set; }
        public Action<float, float> OnManaChange;
        public Action<bool> OnCombatChange;
        public Action<bool> OnLifeChange { get; set; }
        public Action<bool> OnHiddenChange { get; set; }
        //public Action<SocialEventData> OnReceiveInteraction;
        //public Action<SocialEventData> OnReactToInteraction;
        public Action<RoomInfo, RoomInfo> OnRoomChange;
        public Action<RoomInfo, bool> OnCurrentShowRoom;
        
        public Action<NPCType, NPCType> OnChangeNPCType;
        public Action<EntityInfo, Collider, bool> OnTriggerEvent;
        public Action<EntityInfo, Collision, bool> OnCollisionEvent;
        public Action<EntityInfo, GameObject, bool> OnPhysicsEvent;
        public Action<EntityInfo> OnChangeStats { get; set; }
        public List<bool> checks { get; private set; }

        public AbilityManager.Events abilityEvents { get; set; }
        public PartyEvents partyEvents;
        public InfoEvents infoEvents;
        public TaskEvents taskEvents = new();
        public TargetableEvents targetableEvents = new();
        public CombatEvents combatEvents = new();
        public PortalEvents portalEvents;
        public SceneEvents sceneEvents;
        public SocialEvents socialEvents;
        public RoomInfo.Events roomEvents;

        //Non Player Events
        public Action<EntityInfo, PartyInfo> OnPlayerLineOfSight;
        public Action<EntityInfo, PartyInfo> OnPlayerOutOfRange;
        public Action<EntityInfo, PartyInfo> OnPlayerLOSBreak;

        //Event Triggers
        private float healthCheck;
        private float maxHealthCheck;
        private float manaCheck;
        private float maxManaCheck;
        private float shieldCheck;
        private bool combatCheck;
        private bool isAliveCheck;
        EntityInfo combatTargetCheck;
        private NPCType npcTypeCheck;
        #endregion

        #region Initialization

        protected override void Awake()
        {
            base.Awake();
            abilityEvents ??= new();
            infoEvents.Initiate(this);
            combatEvents.Initiate(this);
        }

        public bool initiated { get; private set; }
        void Start()
        {
            EntityStart();
            HandleFalling();
            HandleRarityEvents();

            initiated = true;
        }

        public void GetDependencies()
        {

        }
        public void GetPartyControls()
        {
            if (transform.parent)
            {
                if (GetComponentInParent<PartyInfo>())
                {
                    entityControlType = EntityControlType.PartyControl;
                }
            }
        }

        public virtual void EntityStart()
        {
            if (!properties.created)
            {
                GetDependencies();
                StartUp();
            }

            components.properties = GetComponentsInChildren<EntityProp>();

            StartCoroutine(HandleRegeneration());
        }

        public void StartUp()
        {

            if (fixedStats)
            {
                stats = entityStats;
                UpdateResources(true);
            }
            else if (presetStats)
            {
                presetStats = Instantiate(presetStats);
                entityName = presetStats.name;
                role = presetStats.Role;
                entityStats = presetStats.Stats;
                //npcType = presetStats.npcType;


                canLevel = presetStats.canLevel;
                stats += entityStats;
                UpdateResources(true);
            }
            else
            {
                entityStats.Level = 1;
                entityStats.Vitality = 10;
                entityStats.Strength = 10;
                entityStats.Dexterity = 10;
                entityStats.Wisdom = 10;
                role = Role.Damage;
                stats += entityStats;
                UpdateResources(true);

            }
            if (npcType == NPCType.Hostile)
            {
                entityControlType = EntityControlType.NoControl;
            }

            //health = maxHealth;
            //mana = maxMana;
            //shield = 0;
            isAlive = true;

            GetPartyControls();
            //Invoke("SetEntityStats", .125f);
            //Invoke("UpdateCurrentStats", .250f);
            SetEntityStats();
            UpdateCurrentStats();



            UpdateHealthRegen();
        }
        public void UpdateHealthRegen()
        {
            if (rarity != EntityRarity.Player)
            {
                healthRegenPercent = .25f;
            }
            else
            {
                healthRegenPercent = .01f;
            }
        }

        #endregion
        void Update()
        {
            HandleEventTriggers();
        }
        public void OnDestroy()
        {
            infoEvents.OnDestroy?.Invoke(this);
            if (GMHelper.TargetManager())
            {
                if (GMHelper.TargetManager().selectedTargets.Contains(gameObject))
                {
                    GMHelper.TargetManager().selectedTargets.Remove(gameObject);
                }
            }
        }

        

        #region Mouse Over Events

        bool enableMouseOvers = false;
        private void OnMouseEnter()
        {
            if (!enableMouseOvers) return;
            infoEvents.OnMouseHover?.Invoke(this, true, gameObject);


        }
        private void OnMouseExit()
        {
            if (!enableMouseOvers) return;
            infoEvents.OnMouseHover?.Invoke(this, false, gameObject);

        }
        #endregion

        #region Physics Events

        public void OnTriggerEnter(Collider other)
        {
            OnTriggerEvent?.Invoke(this, other, true);
            OnPhysicsEvent?.Invoke(this, other.gameObject, true);
        }

        public void OnTriggerExit(Collider other)
        {

            OnPhysicsEvent?.Invoke(this, other.gameObject, false);
            OnTriggerEvent?.Invoke(this, other, false);
        }
        public void OnCollisionEnter(Collision collision)
        {
            OnPhysicsEvent?.Invoke(this, collision.gameObject, true);
            OnCollisionEvent?.Invoke(this, collision, true);
        }
        public void OnCollisionExit(Collision collision)
        {

            OnPhysicsEvent?.Invoke(this, collision.gameObject, false);
            OnCollisionEvent?.Invoke(this, collision, true);
        }
        
        #endregion
        async void HandleFalling()
        {
            var height = transform.position.y;

            var manager = GameManager.active;

            if (manager.GameState != GameState.Play) return;

            while (Application.isPlaying)
            {
                if (this == null) return;
                if (height != transform.position.y)
                {
                    height = transform.position.y;
                    
                    if (height < -100)
                    {
                        Die();
                    }
                }
                await Task.Delay(1000);
            }
        }

        public void UpdateObjectives(object sender)
        {
            objectives = new();
            infoEvents.OnUpdateObjectives?.Invoke(objectives);
        }

        void UpdateResources(bool val)
        {
            if (role == Role.Tank && rarity == EntityRarity.Player)
            {
                maxHealth = stats.Vitality * 10 * GMHelper.Difficulty().settings.tankHealthMultiplier;
                entityStats.damageReduction = .25f;
            }
            else
            {
                maxHealth = stats.Vitality * 10;
            }

            maxMana = stats.Wisdom * 10;

            if (val)
            {
                health = maxHealth;
                mana = maxMana;
            }
        }

        void HandleRarityEvents()
        {
            infoEvents.OnRarityChange += (EntityRarity before, EntityRarity after) => {
                ArchAction.Yield(() => UpdateHealthRegen());
            };
        }

        
        public void GainExperience(object sender, float amount)
        {
            OnExperienceGainOutside?.Invoke(sender, amount);
        }

        protected void HandleEventTriggers()
        {
            if (healthCheck != health || maxHealthCheck != maxHealth || shieldCheck != shield)
            {
                shieldCheck = shield;
                healthCheck = health;
                maxHealthCheck = maxHealth;

                OnHealthChange?.Invoke(health, shield, maxHealth);
            }

            if (manaCheck != mana || maxManaCheck != maxMana)
            {
                manaCheck = mana;
                OnManaChange?.Invoke(mana, maxMana);
            }

            if (combatCheck != isInCombat)
            {
                combatCheck = isInCombat;

                OnCombatChange?.Invoke(combatCheck);
            }

            if (isAlive != isAliveCheck)
            {
                isAliveCheck = isAlive;

                if (EntityDeathHandler.active)
                {
                    EntityDeathHandler.active.HandleLifeChange(gameObject);
                }

                OnLifeChange?.Invoke(isAliveCheck);
            }

            if (combatTargetCheck != combatTarget)
            {
                combatEvents.OnNewCombatTarget?.Invoke(combatTargetCheck, combatTarget);
                combatTargetCheck = combatTarget;
            }

            if(npcType != npcTypeCheck)
            {
                OnChangeNPCType?.Invoke(npcTypeCheck, npcType);

                npcTypeCheck = npcType;
            }
        }

        public string ObjectivesDescription()
        {
            if (objectives == null) objectives = new();
            //infoEvents.OnObjectiveCheck?.Invoke(objectives);
            return ArchString.NextLineList(objectives);
        }
        public void SetEntityStats()
        {
            entityStats.movementSpeed = 1;
            entityStats.healingReceivedMultiplier = 1;
            entityStats.damageMultiplier = 1;
            entityStats.damageTakenMultiplier = 1;
        }
        public void UpdateCurrentStats()
        {
            if (entityStats == null) { return; }

            var currentStats = entityStats;
            var buffs = Buffs();


            if (buffs)
            {
                currentStats += buffs.stats;
            }

            if (CharacterInfo())
            {
                CharacterInfo().UpdateEquipmentStats();
                currentStats += CharacterInfo().totalEquipmentStats;
            }

            stats = currentStats;
            UpdateResources(false);

            OnChangeStats?.Invoke(this);

        }

        #region Combat Actions
        public void Damage(CombatEventData combatData)
        {
            if (!isAlive) return;
            combatData.target = this;
            var source = combatData.source;
            var damageType = combatData.DataDamageType();

            var sourceLevel = combatData.source ? combatData.source.stats.Level : stats.Level;

            if (sourceLevel <= 0) sourceLevel = 1;


            HandleValue();
            HandleDamage();


            void HandleValue()
            {
                var originalValue = combatData.value;
                if (states.Contains(EntityState.Immune))
                {
                    combatData.value = 0;
                    combatEvents.OnImmuneDamage?.Invoke(combatData);
                    return;
                }

                float criticalRole = UnityEngine.Random.Range(0, 100);
                

                if (source)
                {
                    Debugger.InConsole(47438, $"Critical Role is {criticalRole} entity crit chance is {source.stats.criticalChance * 100}");
                    if (source.stats.criticalChance * 100 > criticalRole)
                    {
                        combatData.critical = true;
                        combatData.value *= source.stats.criticalDamage;
                    }


                    combatData.value *= source.stats.damageMultiplier;
                }

                combatData.value *= stats.damageTakenMultiplier;

                combatData.value -= combatData.value * stats.damageReduction;


                if (damageType == DamageType.Physical)
                {
                    var reduction = ReductionPercent(stats.armor);

                    combatData.value -= combatData.value * reduction;
                }
                else if (damageType == DamageType.Magical)
                {
                    var reduction = ReductionPercent(stats.magicResist);
                    combatData.value -= combatData.value * reduction;
                }

                if (combatData.value < 0)
                {
                    combatData.value = 0;
                }
                
                if (combatData.value < originalValue && damageType == DamageType.True)
                {
                    combatData.value = originalValue;
                }

                if (combatData.value > health + shield)
                {
                    combatData.value = health + shield;
                }

            }
            void HandleDamage()
            {
                if (combatData.value >= health + shield)
                {
                    combatData.lethalDamage = true;
                }

                if (source)
                {
                    source.combatEvents.BeforeDamageDone?.Invoke(combatData);
                }
                combatEvents.BeforeDamageTaken?.Invoke(combatData);


                DamageHealth();

                if (source != null) source.OnDamageDone?.Invoke(combatData);
                OnDamageTaken?.Invoke(combatData);

                void DamageHealth()
                {
                    if (health - combatData.value <= 0)
                    {
                        Die();
                        EntityDeathHandler.active.HandleDeadEntity(combatData);

                        if (IsPlayer())
                        {
                            combatEvents.OnKillPlayer?.Invoke(combatData);
                        }
                        OnDeath?.Invoke(combatData);
                        source.OnKill?.Invoke(combatData);
                    }
                    else
                    {
                        health -= combatData.value;
                    }
                }
            }

            float ReductionPercent(float reductionSource)
            {
                var reduction = (float) Math.Pow(.99, (reductionSource / sourceLevel));

                var inverse = 1 - reduction;

                return inverse;
            }
        }
        public void Heal(CombatEventData combatData)
        {
            if (!isAlive) { return; }
            combatData.target = this;
            var source = combatData.source;
            combatEvents.BeforeHealingDone?.Invoke(combatData);
            combatEvents.BeforeHealingTaken?.Invoke(combatData);

            HandleValue();
            HandleHealing();


            void HandleValue()
            {
                combatData.value *= stats.healingReceivedMultiplier;

                float criticalRole = UnityEngine.Random.Range(0, 100);

                if (source.stats.criticalChance * 100 > criticalRole)
                {
                    combatData.critical = true;
                    combatData.value *= source.stats.criticalDamage;
                }

                if (combatData.value + health > maxHealth)
                {
                    combatData.value = maxHealth - health;
                }
            }

            void HandleHealing()
            {
                var healingDone = combatData.value;
                if (health + combatData.value > maxHealth)
                {
                    healingDone = maxHealth - health;
                    health = maxHealth;
                    
                }
                else
                {
                    health += combatData.value;
                }
                combatData.value = healingDone;
                HandleEvents();
            }

            void HandleEvents()
            {
                
                OnHealingTaken?.Invoke(combatData);
                source.OnHealingDone?.Invoke(combatData);
            }
        }
        public bool AddState(EntityState state, StateChangeEvent eventData)
        {
            if (stateImmunities.Contains(state) || states.Contains(EntityState.Immune))
            {
                Debugger.Combat(8341, $"State negated: {state}");
                combatEvents.InvokeStateEvent(eStateEvent.OnStatesNegated, eventData);
                return false;
            }

            states.Add(state);
            
            eventData.SetAfterStates(states);
            combatEvents.InvokeStateEvent(eStateEvent.OnStatesChange, eventData);
            return true;
        }
        public bool RemoveState(EntityState state, StateChangeEvent eventData)
        {

            bool removed = false;

            for (int i = 0; i < states.Count; i++)
            {
                if (states[i] == state)
                {
                    states.RemoveAt(i);
                    removed = true;
                    break;
                }
            }

            if (removed)
            {
                eventData.SetAfterStates(states);
                combatEvents.InvokeStateEvent(eStateEvent.OnStatesChange, eventData);
            }
            return removed;
        }
        public bool UseAutoOn(EntityInfo target)
        {
            var checks = new List<bool>();

            abilityEvents.OnUseAuto?.Invoke(target, checks);

            foreach(var check in checks)
            {
                if (check) return true;
            }

            return false;
        }
        public bool SetCombatTarget(EntityInfo target)
        {
            this.combatTarget = target;
            return true;
        }
        #endregion

        public bool SetRarity(EntityRarity rarity)
        {
            if (this.rarity == rarity) return false;
            infoEvents.OnRarityChange?.Invoke(this.rarity, rarity);
            this.rarity = rarity;


            return true;
        }
        public Transform Target()
        {
            return target;
        }

        public bool SetTarget(Transform target)
        {
            this.target = target;
            return true;
        }


        
        public bool IsEnemy(GameObject target)
        {
            if (!target.GetComponent<EntityInfo>()) return false;
            var targetNPCType = target.GetComponent<EntityInfo>().npcType;

            if (npcType == NPCType.Friendly && targetNPCType == NPCType.Hostile) return true;
            if (npcType == NPCType.Hostile && targetNPCType == NPCType.Friendly) return true;
            return false;

        }

        public bool CanMove()
        {
            if (!isAlive) return false;

            checks = new();

            infoEvents.OnCanMoveCheck?.Invoke(this, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }
        public void ReactToSocial(SocialEventData eventData)
        {
            if (eventData.target == this)
            {
                socialEvents.OnReceiveInteraction?.Invoke(eventData);
            }
            else
            {
                socialEvents.OnReactToInteraction?.Invoke(eventData);
            }
        }
        public void Use(float value)
        {
            if (mana - value < 0)
            {
                mana = 0;
            }
            else
            {
                mana -= value;
            }
        }

        public void GainResource(float value)
        {
            
            if (mana + value > maxMana)
            {
                mana = maxMana;
            }
            else
            {
                mana += value;
            }

            mana = Math.Clamp(mana, 0, maxMana);

        }
        public void Die()
        {
            if (!isAlive) return;
            isAlive = false;

            health = 0;

            if(Entity.IsPlayer(gameObject))
            {

            }
            else
            {
                var world = World.active;
                if (world.noDisappearOnDeath && !summon.isSummoned)
                {
                    return;
                }

                Decay();
            }
        }
        async void Decay()
        {
            await Task.Delay(5000);
            disappeared = true;
            await Task.Yield();

            Destroy(gameObject);

        }
        public void RestoreFull()
        {
            health = maxHealth;
            mana = maxMana;
        }

        public void PingThreat(EntityInfo entity)
        {
            combatEvents.OnPingThreat?.Invoke(entity, 20);
        }

        public bool CanSee(Transform target)
        {
            checks = new();

            infoEvents.OnCanSeeCheck?.Invoke(this, target);

            foreach(var check in checks)
            {
                if (check) return true;
            }

            return false;
        }
        public NPCType EnemyType()
        {
            if (npcType == NPCType.Hostile)
            {
                return NPCType.Friendly;
            }

            if (npcType == NPCType.Untargetable)
            {
                return NPCType.Untargetable;
            }

            return NPCType.Hostile;
        }
        public void SetPortrait(Sprite sprite)
        {
            entityPortrait = sprite;
        }
        public Sprite PortraitIcon()
        {

            if (entityPortrait != null)
            {
                return entityPortrait;
            }

            infoEvents.OnNullPortraitCheck?.Invoke(this);

            return entityPortrait;
        }

        #region Actions
        public bool LootItem(ItemInfo itemInfo, bool fromWorld = false)
        {
            var lootData = new Inventory.LootEventData(itemInfo) { fromWorld = fromWorld };

            var checks = new List<bool>();


            infoEvents.OnLootItemCheck?.Invoke(lootData, checks);

            Debugger.UI(8715, $"Loot item checks Count: {checks.Count}");
            foreach(var check in checks)
            {
                
                if (!check)
                {
                    return lootData.succesful;
                }
            }

            infoEvents.OnLootItem?.Invoke(lootData);

            if (fromWorld)
            {
                infoEvents.OnLootItemFromWorld?.Invoke(lootData);
            }



            return lootData.succesful;
        }
        public async void KillSelf (EntityInfo source = null)
        {
            if (!isAlive) return;
            int tries = 3;
            while (isAlive)
            {
                Damage(new(source? source: this)
                {
                    value = maxHealth + shield
                });

                await Task.Yield();

                tries -= 1;
                if (tries <= 0) break;
            }

            if (!isAlive) return;

            health = 0;
            isAlive = false;
        }
        public void Revive(CombatEventData combatData)
        {
            combatData.value = maxHealth * combatData.percentValue;
            combatData.target = this;
            this.health = combatData.value;
            var source = combatData.source;
            isAlive = true;


            if(source)
            {
                source.OnReviveOther?.Invoke(combatData);
            }
            
            OnReviveThis?.Invoke(combatData);
        }
        public void CompleteQuest(Quest quest)
        {
            infoEvents.OnQuestComplete?.Invoke(quest);
        }
        public void ChangeNPCType(NPCType type)
        {
            this.npcType = type;
        }
        public void ChangeBehavior(NPCType npcType, AIBehaviorType behaviorType)
        {

            ChangeNPCType(npcType);
            AIBehavior().behaviorType = behaviorType;
            
        }
        public void SetSummoned(SpawnerInfo.SummonData summonData)
        {
            summon.SetSummoned(summonData);
            

            ChangeNPCType(summonData.master.npcType);
            SetParent();

            AIBehavior().CreateBehavior<ArchSummonAgent>("Summon Behavior");

            void SetParent()
            {
                var entityGenerator = MapEntityGenerator.active;
                if (entityGenerator == null) return;
                transform.SetParent(entityGenerator.summons);
            }


        }
        public void Move(Vector3 position)
        {
            transform.position = position;
            infoEvents.OnSignificantMovementChange?.Invoke(position);
        }
        #endregion
        public override string ToString()
        {
            if (entityName == null || entityName.Trim() == "")
            {
                return name;
            }

            return entityName;
        }


        #region Event Flags
        public bool CanAttack(EntityInfo target)
        {
            if (target == null) return false;

            var checks = new List<bool>();

            if (!target.CanBeAttacked()) return false;
            combatEvents.OnCanAttackCheck?.Invoke(target, checks);

            foreach (var check in checks)
            {
                if (!check) return false;
            }

            if (target.npcType == NPCType.Untargetable) { return false; }

            if (npcType == NPCType.Neutral) { return true; }


            if (target.npcType != npcType) { return true; }

            return false;
        }
        public bool CanHelp(EntityInfo target)
        {
            if (target == null) return false;


            if (!target.CanBeHelped()) return false;

            var checks = new List<bool>();

            combatEvents.OnCanHelpCheck?.Invoke(target, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            if (target.npcType == NPCType.Untargetable) { return false; }

            if (npcType == NPCType.Neutral) { return true; }

            if (target.npcType != npcType)
            {
                return false;
            }

            return true;
        }
        public bool CanDrop(ItemData item)
        {
            var checks = new List<bool>();

            infoEvents.OnCanDropCheck?.Invoke(item, checks);

            foreach(var check in checks)
            {
                if (!check) return false;
            }

            return true;
        }
        public bool CanSpend(Currency currency, int amount)
        {
            var checks = new List<bool>();

            infoEvents.OnCanSpendCheck?.Invoke(currency, amount, checks);

            foreach (var check in checks)
            {
                if (check)
                {
                    return true;
                }
            }

            return false;
        }
        public bool CanPickUp(ItemData itemData)
        {
            var checks = new List<bool>();
            infoEvents.OnCanPickUpCheck?.Invoke(itemData, checks);

            foreach (var check in checks)
            {
                if (!check) continue;
                return true;
            }
            return false;
        }
        public bool CanAttack(GameObject target)
        {
            if(target == null) { return false; }
            var targetInfo = target.GetComponent<EntityInfo>();

            return CanAttack(targetInfo);
        }
        public bool CanHelp(GameObject target)
        {
            if(target == null) { return false; }
            var targetInfo = target.GetComponent<EntityInfo>();

            return CanHelp(targetInfo);

        }
        public bool CanAttack(NPCType npcType)
        {
            if(npcType == NPCType.Untargetable) { return false; }

            if(this.npcType == NPCType.Neutral) { return true; }

            if(npcType != this.npcType) { return true; }

            return false;
        }
        public bool CanHelp(NPCType npcType)
        {
            if(npcType == NPCType.Untargetable) { return false; }

            if(this.npcType == NPCType.Neutral) { return true; }

            if(npcType != this.npcType)
            {
                return false;
            }

            return true;
        }
        public bool CanBeAttacked()
        {
            var checks = new List<bool>();

            combatEvents.OnCanBeAttackedCheck?.Invoke(checks);

            foreach(var check in checks)
            {
                if (!check)
                {
                    return false;
                }
            }
            return true;
        }
        public bool IsPlayer()
        {

            var checks = new List<bool>();

            infoEvents.OnIsPlayerCheck?.Invoke(this, checks);

            foreach (var check in checks)
            {
                if (check) return true;
            }

            return false;
        }
        public bool CanBeHelped()
        {
            var checks = new List<bool>();

            combatEvents.OnCanBeHelpedCheck?.Invoke(checks);

            foreach (var check in checks)
            {
                if (!check)
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
        public ToolTipData ToolTipData()
        {


            var type = archClass != null ? $"{archClass.className} {role}" : $"{role}";


            var (health, maxHealth) = Health();


            return new()
            {
                icon = entityPortrait,
                name = $"{entityName}",
                subeHeadline = type,
                type = $"{health}/{maxHealth} Health",
                description = entityDescription,
                attributes = $"{npcType} {rarity}",
                requirements = ObjectivesDescription(),
                value = $"Level {entityStats.Level}",
            };
        }
        public (string, string) Health()
        {
            var health = ArchString.FloatToSimple(this.health);
            var maxHealth = ArchString.FloatToSimple(this.maxHealth);

            return (health, maxHealth);
        }

        public void AddEventTrigger(Action action, EntityEvent trigger)
        {
            switch (trigger)
            {
                case EntityEvent.OnDeath:
                    OnDeath += (eventData) => { action(); };
                    break;
                case EntityEvent.OnRevive:
                    OnReviveThis += (eventData) => { action(); };
                    break;
                case EntityEvent.OnLevelUp:
                    OnLevelUp += (newLevel) => { action(); };
                    break;
                case EntityEvent.OnDamageTaken:
                    OnDamageTaken += (eventData) => { action(); };
                    break;
                case EntityEvent.OnDetectPlayer:
                    combatEvents.OnFirstThreatWithPlayer += (threatInfo) => { action(); };
                    break;
                case EntityEvent.OnKillPlayer:
                    combatEvents.OnKillPlayer += (eventData) => { action(); };
                    break;
                case EntityEvent.OnCastStart:
                    abilityEvents.OnCastStart += (ability) => { if(!ability.isAttack) action(); };
                    break;
                case EntityEvent.OnCastEnd:
                    abilityEvents.OnCastEnd += (ability) => { if(!ability.isAttack) action(); };
                    break;
                case EntityEvent.OnAttack:
                    abilityEvents.OnAttack += (ability) => { action(); };
                    break;
                default:
                    if (summon.isSummoned) action();
                    break;
            }
        }

        #region Entity Components
        public IEnumerator HandleRegeneration()
        {

            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (isAlive)
                {
                    HandleHealthRegen();
                    HandleManaRegen();
                }

            }


            void HandleHealthRegen()
            {
                if(states.Contains(EntityState.MindControlled)) { return; }
                if(isInCombat) { return; }


                var nextHealth = health + maxHealth * healthRegenPercent;

                if (nextHealth < maxHealth)
                {
                    health = nextHealth;
                }
                else if (nextHealth > maxHealth)
                {
                    health = maxHealth;
                }
            }
            void HandleManaRegen()
            {
                var nextMana = mana + maxMana * manaRegenPercent;
                if (nextMana < maxMana)
                {
                    mana = nextMana;

                }
                else
                {
                    mana = maxMana;
                }

            }

        }
        public Movement Movement()
        {
            return GetComponentInChildren<Movement>();
        }
        public AbilityManager AbilityManager()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<AbilityManager>())
                {
                    return child.GetComponent<AbilityManager>();
                }
            }
            return null;
        }
        public AIDestinationSetter AIDestinationSetter()
        {
            if (gameObject.GetComponent<AIDestinationSetter>())
            {
                return gameObject.GetComponent<AIDestinationSetter>();
            }
            return null;
        }
        public AIPath Path()
        {
            if (gameObject.GetComponent<AIPath>() != null)
            {
                return gameObject.GetComponent<AIPath>();
            }
            return null;
        }
        public GraphicsInfo GraphicsInfo()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<GraphicsInfo>())
                {
                    return child.GetComponent<GraphicsInfo>();
                }
            }
            return null;
        }
        public CharacterInfo CharacterInfo()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<CharacterInfo>())
                {
                    return child.GetComponent<CharacterInfo>();
                }
            }
            return null;
        }
        public PlayerController PlayerController()
        {
            return EntityComponent<PlayerController>();
        }
        public AIBehavior AIBehavior()
        {
            return GetComponentInChildren<AIBehavior>();
        }
        public ThreatManager ThreatManager()
        {
            return EntityComponent<ThreatManager>();
        }
        public LineOfSight LineOfSight()
        {
            //var lineOfSight = components.Get<LineOfSight>();

            return EntityComponent<LineOfSight>();
            //var behavior = AIBehavior();

            //if (behavior == null) return null;

            //var lineOfSight = behavior.LineOfSight();
            //return lineOfSight;
        }
        public PartyInfo PartyInfo()
        {
            if (GetComponentInParent<PartyInfo>())
            {
                return GetComponentInParent<PartyInfo>();
            }

            return null;
        }
        public Anim Animation()
        {
            if (CharacterInfo() && CharacterInfo().Animation())
            {
                return CharacterInfo().Animation();
            }
            return null;
        }
        public BuffsManager Buffs()
        {
            if (gameObject == null) return null;

            return EntityComponent<BuffsManager>();
        }
        public AudioManager Voice()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<AudioManager>() &&
                    child.GetComponent<AudioManager>().mixerGroup == GMHelper.Mixer().Voice)
                {
                    return child.GetComponent<AudioManager>();
                }
            }
            return null;
        }
        public CombatInfo CombatInfo()
        {
            return EntityComponent<CombatInfo>();
        }
        public AudioManager SoundEffect()
        {
            return GetComponentsInChildren<AudioManager>().First(manager => manager.mixerGroup == GMHelper.Mixer().SoundEffect);
        }
        public CharacterBodyParts BodyParts()
        {
            return EntityComponent<CharacterBodyParts>();
        }
        public ParticleManager ParticleManager()
        {
            return GetComponentInChildren<ParticleManager>();
        }
        public AudioManager VoiceEffect()
        {
            return GetComponentsInChildren<AudioManager>().First(manager => manager.mixerGroup == GMHelper.Mixer().Voice);
        }
        public EntitySpeech Speech()
        {
            return EntityComponent<EntitySpeech>();
        }
        public Inventory Inventory()
        {

            return EntityComponent<Inventory>();
            
        }
        public CombatBehavior CombatBehavior()
        {
            return EntityComponent<CombatBehavior>();
        }
        public RoomInfo CurrentRoom()
        {
            var ray = new Ray(transform.position, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, GMHelper.LayerMasks().walkableLayer))
            {
                if (hit.transform.GetComponentInParent<RoomInfo>())
                {
                    return hit.transform.GetComponentInParent<RoomInfo>();
                }
            }

            return null;
        }
        public ETaskHandler TaskHandler()
        {
            return EntityComponent<ETaskHandler>();
        }


        [SerializeField] bool showEntity;
        private void OnValidate()
        {
            if (!showEntity) return;
            showEntity = false;
            var character = GetComponentInChildren<CharacterInfo>();
            character.ShowCharacter();
            var graphicsInfo = GetComponentInChildren<GraphicsInfo>();
            graphicsInfo.ShowGraphics();
        }
        public async void ShowEntity(bool val, bool hideLights = true, bool destroys = false)
        {
            var tasks = new List<Task>();

            isHidden = !val;

            OnHiddenChange?.Invoke(isHidden);


            if(hideLights)
            {
                tasks.Add(ShowLights (val));
            }

            await Task.WhenAll(tasks);
        }
        async Task ShowLights(bool val)
        {
            var lights = GetComponentsInChildren<Light>();

            foreach (var light in lights)
            {
                if (light == null)
                {
                    return;
                }
                var ignore = light.GetComponent<IgnoreProp>();

                if (ignore)
                {
                    var canSet = ignore.CanSet(val);
                    Debugger.Environment(1752, $"{this} Can set {light}: {canSet}");
                    if (!canSet) continue;
                }
                light.enabled = val;
                await Task.Yield();
            }
        }
        
        public T EntityComponent<T>() where T : Component
        {
            var component = components.Get<T>();

            if (component == null)
            {
                component = components.Add(GetComponentInChildren<T>());
            }

            return component;
        }

        [Serializable]
        public struct ComponentManager
        {
            public Dictionary<Type, object> components;
            public EntityProp[] properties;
            public List<string> activeComponents;
            public bool showActiveComponents;
            
            public T Add<T>(T component) where T : Component
            {
                if (components == null) components = new();
                
                if (!components.ContainsKey(typeof(T)))
                {
                    components.Add(typeof(T), component);
                }

                if (showActiveComponents)
                {
                    if (activeComponents == null) activeComponents = new();
                    activeComponents.Add(component.ToString());
                }

                return (T)components[typeof(T)];
            }

            public T Get<T>() where T : Component
            {
                if (components == null) components = new();
                if (!components.ContainsKey(typeof(T))) return null;

                return (T)components[typeof(T)];
            }
        }
        #endregion
    }
}

