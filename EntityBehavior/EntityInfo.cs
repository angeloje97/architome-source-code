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

        #endregion

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


        #region Events that need to change to using archeventHandler
        public Action<float> OnExperienceGain { get; set; }
        public Action<object, float> OnExperienceGainOutside { get; set; }

        #endregion

        public Action<BuffInfo, EntityInfo> OnNewBuff { get; set; }
        public Action<BuffInfo, EntityInfo> OnBuffApply { get; set; }
        public Action<int> OnLevelUp { get; set; }
        public Action<float, float, float> OnHealthChange { get; set; }

        public Action<float, float> OnManaChange { get; set; }

        public Action<bool> OnCombatChange { get; set; }
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
        public CombatEvents combatEvents;
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

        public bool initiated { get; private set; }
        public bool gatheredDependenies { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            abilityEvents ??= new();
            //infoEvents = new();
            //combatEvents = new();

            infoEvents.Initiate(this);
            combatEvents.Initiate(this);
        }

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

            HandleRegeneration();
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

            isAlive = true;

            GetPartyControls();
            SetEntityStats();
            UpdateCurrentStats();



            UpdateHealthRegen();
        }

        public void UpdateGatheredDependencies()
        {
            if (!infoEvents.InvokeCheck(InfoEvents.EventType.OnHasGatheredDependenciesCheck, new(this))) return;
            gatheredDependenies = true;
        }

        public async Task UntilGatheredDependencies()
        {
            await ArchAction.WaitUntil((float deltaTime) =>
            {
                return gatheredDependenies;
            }, true);
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
                ContainerTargetables.active.RemoveSelected(this);
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

        #region Update Events
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
                    EntityDeathHandler.active.HandleLifeChange(this);
                }

                OnLifeChange?.Invoke(isAliveCheck);
            }

            if (combatTargetCheck != combatTarget)
            {
                combatEvents.InvokeGeneral(eCombatEvent.OnNewCombatTarget, new(this, combatTargetCheck, combatTarget));
                combatTargetCheck = combatTarget;
            }

            if(npcType != npcTypeCheck)
            {
                OnChangeNPCType?.Invoke(npcTypeCheck, npcType);

                npcTypeCheck = npcType;
            }
        }
        void HandleRarityEvents()
        {
            infoEvents.OnRarityChange += (EntityRarity before, EntityRarity after) => {
                ArchAction.Yield(() => UpdateHealthRegen());
            };
        }

        #endregion
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

        public void GainExperience(object sender, float amount)
        {
            OnExperienceGainOutside?.Invoke(sender, amount);
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
        public void Damage(HealthEvent combatData)
        {
            if (!isAlive) return;
            combatData.SetTarget(this);
            var source = combatData.source;
            var damageType = combatData.DataDamageType();

            var sourceLevel = combatData.source ? combatData.source.stats.Level : stats.Level;

            if (sourceLevel <= 0) sourceLevel = 1;


            HandleValue();
            HandleDamage();


            void HandleValue()
            {
                var originalValue = combatData.value;
                var damageResult = combatData.value;
                if (states.Contains(EntityState.Immune))
                {
                    damageResult = 0;
                    combatData.SetValue(damageResult);
                    combatEvents.InvokeHealthChange(eHealthEvent.OnImmuneDamage, combatData);

                    return;
                }

                float criticalRole = UnityEngine.Random.Range(0, 100);
                

                if (source)
                {
                    Debugger.InConsole(47438, $"Critical Role is {criticalRole} entity crit chance is {source.stats.criticalChance * 100}");
                    if (source.stats.criticalChance * 100 > criticalRole)
                    {
                        combatData.SetCritical(true);
                        damageResult *= source.stats.criticalDamage;
                    }


                    damageResult *= source.stats.damageMultiplier;
                }

                damageResult *= stats.damageTakenMultiplier;

                damageResult -= combatData.value * stats.damageReduction;


                if (damageType == DamageType.Physical)
                {
                    var reduction = ReductionPercent(stats.armor);

                    damageResult -= combatData.value * reduction;
                }
                else if (damageType == DamageType.Magical)
                {
                    var reduction = ReductionPercent(stats.magicResist);
                    damageResult -= combatData.value * reduction;
                }

                if (combatData.value < 0)
                {
                    damageResult = 0;
                }
                
                if (combatData.value < originalValue && damageType == DamageType.True)
                {
                    damageResult = originalValue;
                }

                if (combatData.value > health + shield)
                {
                    damageResult = health + shield;
                }

                combatData.SetValue(damageResult);

            }
            void HandleDamage()
            {
                if (combatData.value >= health + shield)
                {
                    combatData.lethalDamage = true;
                }

                if (source)
                {
                    source.combatEvents.InvokeHealthChange(eHealthEvent.BeforeDamageDone, combatData);
                }

                combatEvents.InvokeHealthChange(eHealthEvent.BeforeDamageTaken, combatData);

                DamageHealth();

                if (source != null) source.combatEvents.InvokeHealthChange(eHealthEvent.OnDamageDone, combatData);
                //Change On Damage Done
                combatEvents.InvokeHealthChange(eHealthEvent.OnDamageTaken, combatData);

                void DamageHealth()
                {
                    if (health - combatData.value <= 0)
                    {
                        Die();
                        EntityDeathHandler.active.HandleDeadEntity(combatData);

                        if (IsPlayer())
                        {
                            combatEvents.InvokeHealthChange(eHealthEvent.OnKillPlayer, combatData);
                        }

                        combatEvents.InvokeGeneral(eCombatEvent.OnDeath, combatData);
                        //OnDeath?.Invoke(combatData);
                        source.combatEvents.InvokeHealthChange(eHealthEvent.OnKill, combatData);

                        //source.OnKill?.Invoke(combatData);
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

        public void Heal(HealthEvent healthEvent)
        {
            if (!isAlive) { return; }
            healthEvent.SetTarget(this);
            var source = healthEvent.source;

            combatEvents.InvokeHealthChange(eHealthEvent.BeforeHealingTaken, healthEvent);
            source.combatEvents.InvokeHealthChange(eHealthEvent.BeforeHealingDone, healthEvent);

            HandleValue();
            HandleHealing();


            void HandleValue()
            {
                var newValue = healthEvent.value;
                newValue *= stats.healingReceivedMultiplier;

                float criticalRole = UnityEngine.Random.Range(0, 100);

                if (source.stats.criticalChance * 100 > criticalRole)
                {
                    healthEvent.SetCritical(true);
                    newValue *= source.stats.criticalDamage;
                }

                if (newValue + health > maxHealth)
                {
                    newValue = maxHealth - health;
                }

                healthEvent.SetValue(newValue);
            }

            void HandleHealing()
            {
                var currentValue = healthEvent.value;
                var healingDone = currentValue;
                if (health + currentValue > maxHealth)
                {
                    healingDone = maxHealth - health;
                    health = maxHealth;
                }
                else
                {
                    health += currentValue;
                }

                healthEvent.SetValue(healingDone);
                HandleEvents();
            }

            void HandleEvents()
            {
                combatEvents.InvokeHealthChange(eHealthEvent.OnHealingTaken, healthEvent);
                source.combatEvents.InvokeHealthChange(eHealthEvent.OnHealingDone, healthEvent);
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

        public bool SetRandomRarity()
        {
            bool isDifferent = false;
            var tries = 0;
            var maxTries = 10;
            do
            {
                var rarityList = Enum.GetValues(typeof(EntityRarity));
                var newRarity = ArchGeneric.RandomItem((IEnumerable<EntityRarity>) rarityList);

                if(newRarity != this.rarity)
                {
                    isDifferent = true;
                    this.rarity = newRarity;
                }
            } while (!isDifferent && tries < maxTries);

            return isDifferent;
        }
        public Transform Target()
        {
            return target;
        }

        public void ReactToSocial(SocialEventData eventData)
        {
            if (eventData.target == this)
            {
                infoEvents.InvokeSocial(eSocialEvent.OnReceiveInteraction, eventData);
            }
            else
            {
                infoEvents.InvokeSocial(eSocialEvent.OnReactToInteraction, eventData);
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
            combatEvents.InvokeThreat(eThreatEvent.OnPingThreat, new(entity, this, 20));
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
                var healthEvent = new HealthEvent(source, this, maxHealth + shield);
                Damage(healthEvent);

                await Task.Yield();

                tries -= 1;
                if (tries <= 0) break;
            }

            if (!isAlive) return;

            health = 0;
            isAlive = false;
        }
        public void Revive(HealthEvent combatData)
        {
            combatData.SetValue(maxHealth * combatData.percentValue);
            combatData.SetTarget(this);
            this.health = combatData.value;
            var source = combatData.source;
            isAlive = true;


            if(source)
            {
                //source.OnReviveOther?.Invoke(combatData);
                source.combatEvents.InvokeHealthChange(eHealthEvent.OnGiveRevive, combatData);
            }

            //OnReviveThis?.Invoke(combatData);
            combatEvents.InvokeHealthChange(eHealthEvent.OnGetRevive, combatData);
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

        #region Can Interact With Items
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
        #endregion

        #region Dynamic Properties (CanAttack/CanHelp)

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
        public bool CanAttack(EntityInfo target)
        {
            if (target == null) return false;
            var combatEvent = new CombatEvent(this, target);


            if (!target.CanBeAttacked()) return false;
            var checkCombatEvent = combatEvents.InvokeCheckGeneral(eCombatEvent.OnCanAttackCheck, combatEvent);

            if (!checkCombatEvent) return false;

            if (target.npcType == NPCType.Untargetable) { return false; }
            if (npcType == NPCType.Neutral) { return true; }



            if (target.npcType != npcType) { return true; }

            return false;
        }
        public bool CanHelp(EntityInfo target)
        {
            if (target == null) return false;
            var combatEvent = new CombatEvent(this, target);

            if (!target.CanBeHelped()) return false;


            var checkCombat = combatEvents.InvokeCheckGeneral(eCombatEvent.OnCanHelpCheck, combatEvent);

            if (!checkCombat) return false;

            if (target.npcType == NPCType.Untargetable) { return false; }

            if (npcType == NPCType.Neutral) { return true; }

            if (target.npcType != npcType)
            {
                return false;
            }

            return true;
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
            return combatEvents.InvokeCheckGeneral(eCombatEvent.OnCanBeAttackedCheck, new(this));
        }

        public bool CanBeHelped()
        {
            return combatEvents.InvokeCheckGeneral(eCombatEvent.OnCanBeHelpedCheck, new(this));
        }

        #endregion
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

        public void AddEventTrigger(Action action, EntityEvent trigger, EntityProp prop)
        {

            switch (trigger)
            {
                case EntityEvent.OnDeath:
                    combatEvents.AddListenerHealth(eHealthEvent.OnDeath, (eventData) => action(), prop);
                    break;
                case EntityEvent.OnRevive:
                    combatEvents.AddListenerHealth(eHealthEvent.OnGetRevive, (eventData) => action(), prop);
                    break;
                case EntityEvent.OnLevelUp:
                    OnLevelUp += (newLevel) => { action(); };
                    break;
                case EntityEvent.OnDamageTaken:
                    combatEvents.AddListenerHealth(eHealthEvent.OnDamageTaken, (eventData) => action(), prop);
                    break;
                case EntityEvent.OnDetectPlayer:
                    combatEvents.AddListenerThreat(eThreatEvent.OnFirstThreatWithPlayer, (threatEvent) => action(), prop);
                    break;
                case EntityEvent.OnKillPlayer:
                    combatEvents.AddListenerHealth(eHealthEvent.OnKillPlayer, (eventData) => action(), prop);
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
        public async void HandleRegeneration()
        {

            while (true)
            {
                await Task.Delay(1000);
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
            return GetComponent<AIPath>();
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

