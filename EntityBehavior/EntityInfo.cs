using Architome.Enums;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;

namespace Architome
{
    public class EntityInfo : MonoBehaviour
    {
        // Start is called before the first frame update
        public string entityName;
        [Multiline]
        public string entityDescription;
        public ArchClass archClass;
        public Sprite entityPortrait;

        public EntityFXPack entityFX;

        public EntityControlType entityControlType;
        [Header("Entity Properties")]
        public NPCType npcType;
        public EntityRarity rarity;
        public List<EntityState> stateImmunities;
        public bool isPlayer;
        public bool isAlive;
        public bool isInCombat;
        public bool created = false;
        public bool isRegening = false;
        public bool disappeared = false;
        public bool isHover = false;
        public Role role;
        public RoomInfo currentRoom;
        public List<EntityState> states;
        public WorkerState workerState;
        
        [Serializable]
        public struct SummonedEntity
        {
            public bool isSummoned;
            public EntityInfo master;
            public float timeRemaining;




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
        public float health;
        public float mana;

        [Header("Regen Percent")]
        public float healthRegenPercent = .01f;
        public float manaRegenPercent = .01f;

        [Header("Experience")]
        public bool canLevel;

        public LayerMask walkableLayer;





        //Events
        public struct CombatEvents
        {
            public Action<CombatEventData, bool> OnFixate;
            public Action<List<EntityState>, List<EntityState>> OnStatesChange;
            public Action<List<EntityState>, EntityState> OnStateNegated;
        }

        public struct PartyEvents
        {
            public Action<GroupFormationBehavior> OnRotationFormationStart;
            public Action<GroupFormationBehavior> OnRotateFormationEnd;
        }

        public event Action<CombatEventData> OnDamageTaken;
        public event Action<CombatEventData> OnDamageDone;
        public event Action<CombatEventData> OnHealingTaken;
        public event Action<CombatEventData> OnHealingDone;
        public event Action<CombatEventData> OnReviveOther;
        public event Action<CombatEventData> OnReviveThis;
        public event Action<CombatEventData> OnDeath;
        public event Action<CombatEventData> OnKill;
        public Action<CombatEventData> OnDamagePreventedFromShields;
        public Action<BuffInfo, EntityInfo> OnNewBuff;
        public Action<BuffInfo, EntityInfo> OnBuffApply;
        public Action<float> OnExperienceGain;
        public Action<int> OnLevelUp;
        public Action<float, float, float> OnHealthChange;
        public Action<float, float> OnManaChange;
        public Action<bool> OnCombatChange;
        public Action<bool> OnLifeChange;
        public Action<SocialEventData> OnReceiveInteraction;
        public Action<SocialEventData> OnReactToInteraction;
        public Action<RoomInfo, RoomInfo> OnRoomChange;
        public Action<RoomInfo, bool> OnCurrentShowRoom;
        
        public Action<NPCType, NPCType> OnChangeNPCType;
        public Action<EntityInfo, Collider, bool> OnTriggerEvent;
        public Action<EntityInfo, Collision, bool> OnCollisionEvent;
        public Action<EntityInfo, GameObject, bool> OnPhysicsEvent;
        public Action<EntityInfo> OnChangeStats;
        public TaskEvents taskEvents = new();
        public TargetableEvents targetableEvents = new();
        public CombatEvents combatEvents;
        public PortalEvents portalEvents;

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
        private NPCType npcTypeCheck;

        //Private Variables
        //Methods
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
                entityStats = new Stats().Sum(entityStats,presetStats.Stats);
                npcType = presetStats.npcType;


                canLevel = presetStats.canLevel;
                stats = stats.Sum(stats, entityStats);
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
                stats = stats.Sum(stats, entityStats);
                UpdateResources(true);

            }
            if (npcType == NPCType.Hostile)
            {
                entityControlType = EntityControlType.NoControl;
            }


            health = maxHealth;
            mana = maxMana;
            shield = 0;
            isAlive = true;

            GetPartyControls();
            Invoke("SetEntityStats", .125f);
            Invoke("UpdateCurrentStats", .250f);

            if(!isPlayer)
            {
                healthRegenPercent = .25f;
            }
        }
        void UpdateResources(bool val)
        {
            if (role == Role.Tank)
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
        void Start()
        {
            if (!created)
            {
                GetDependencies();
                StartUp();
            }

            StartCoroutine(HandleRegeneration());
            currentRoom = CurrentRoom();
        }
        void Update()
        {
            HandleEventTriggers();
        }
        void HandleEventTriggers()
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

                EntityDeathHandler.active.HandleLifeChange(gameObject);
                OnLifeChange?.Invoke(isAliveCheck);
            }

            if(npcType != npcTypeCheck)
            {
                OnChangeNPCType?.Invoke(npcTypeCheck, npcType);

                npcTypeCheck = npcType;
            }



        }
        public void OnDestroy()
        {
            if (GMHelper.TargetManager())
            {
                if (GMHelper.TargetManager().selectedTargets.Contains(gameObject))
                {
                    GMHelper.TargetManager().selectedTargets.Remove(gameObject);
                }
            }
        }

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

            if (Buffs())
            {
                currentStats = currentStats.Sum(currentStats, Buffs().stats);
            }

            if (CharacterInfo())
            {
                CharacterInfo().UpdateEquipmentStats();
                currentStats = currentStats.Sum(currentStats, CharacterInfo().totalEquipmentStats);
            }

            stats = currentStats;
            UpdateResources(false);

            OnChangeStats?.Invoke(this);

        }
        public void Damage(CombatEventData combatData)
        {
            combatData.target = this;
            var source = combatData.source;
            var damageType = DamageType.True;

            if(combatData.catalyst)
            {
                damageType = combatData.catalyst.damageType;
            }
            else if(combatData.buff)
            {
                damageType = combatData.buff.damageType;
            }


            HandleValue();
            HandleDamage();

            void HandleValue()
            {
                

                float criticalRole = UnityEngine.Random.Range(0, 100);
                

                if (source)
                {
                    Debugger.InConsole(47438, $"Critical Role is {criticalRole} entity crit chance is {source.stats.criticalStrikeChance * 100}");
                    if (source.stats.criticalStrikeChance * 100 > criticalRole)
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
                    combatData.value -= stats.armor;
                }
                else if (damageType == DamageType.Magical)
                {
                    combatData.value -= stats.magicResist;
                }

                if (combatData.value < 0)
                {
                    combatData.value = 0;
                }

                if (combatData.value > health + shield)
                {
                    combatData.value = health + shield;
                }

            }
            void HandleDamage()
            {
                if (source != null) source.OnDamageDone?.Invoke(combatData);

                OnDamageTaken?.Invoke(combatData);
                DamageShield();
                DamageHealth();

                void DamageShield()
                {
                    
                    if (Buffs() == null) { return; }
                    var buffs = Buffs().GetComponentsInChildren<BuffShield>().ToList();

                    if(buffs.Count == 0) { return; }

                    foreach (var buff in buffs)
                    {
                        if (buff.shieldAmount == 0) continue;
                        if (combatData.value <= 0) break;

                        combatData.value = buff.DamageShield(combatData.value);

                    }
                }
                void DamageHealth()
                {
                    if (health - combatData.value <= 0)
                    {
                        Die();
                        EntityDeathHandler.active.HandleDeadEntity(combatData);

                        
                        OnDeath?.Invoke(combatData);
                        source.OnKill?.Invoke(combatData);
                    }
                    else
                    {
                        health -= combatData.value;
                    }
                }
            }
        }

        public void Heal(CombatEventData combatData)
        {
            if (!isAlive) { return; }
            combatData.target = this;
            var source = combatData.source;
            HandleValue();
            HandleHealing();


            void HandleValue()
            {
                combatData.value *= stats.healingReceivedMultiplier;

                float criticalRole = UnityEngine.Random.Range(0, 100);

                if (source.stats.criticalStrikeChance * 100 > criticalRole)
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

        public bool AddState(EntityState state)
        {
            if (stateImmunities.Contains(state))
            {
                combatEvents.OnStateNegated?.Invoke(states, state);
                return false;
            }
            var previousStates = states.ToList();

            states.Add(state);
            combatEvents.OnStatesChange?.Invoke(previousStates, states);

            return true;
        }

        public bool RemoveState(EntityState state)
        {
            if (!states.Contains(state))
            {
                return false;
            }

            var previousStates = states.ToList();

            states.Remove(state);

            combatEvents.OnStatesChange?.Invoke(previousStates, states);

            return true;
        }

        public Transform Target()
        {
            var movement = Movement();

            if (movement != null)
            {
                return movement.Target();
            }

            return null;
        }

        public bool IsEnemy(GameObject target)
        {
            if (!target.GetComponent<EntityInfo>()) return false;

            var targetNPCType = target.GetComponent<EntityInfo>().npcType;

            if(npcType == NPCType.Friendly && targetNPCType == NPCType.Hostile)
            {
                return true;
            }

            if(npcType == NPCType.Hostile && targetNPCType == NPCType.Friendly)
            {
                return true;
            }


            return false;

        }
        public void ReactToSocial(SocialEventData eventData)
        {
            if (eventData.target == this)
            {
                OnReceiveInteraction?.Invoke(eventData);
            }
            else
            {
                OnReactToInteraction?.Invoke(eventData);
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
        public void UpdateShield()
        {
            float totalShield = 0;
            if (Buffs())
            {
                foreach (BuffInfo buff in Buffs().Buffs())
                {
                    if (buff.GetComponent<BuffShield>())
                    {
                        totalShield += buff.GetComponent<BuffShield>().shieldAmount;
                    }
                }
            }

            shield = totalShield;
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

        }
        public void Die()
        {
            isAlive = false;

            health = 0;

            if(Entity.IsPlayer(gameObject))
            {

            }
            else
            {
                StartCoroutine(Decay());
            }
        }
        public void RestoreFull()
        {
            health = maxHealth;
            mana = maxMana;
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
        public void ChangeBehavior(NPCType npcType, AIBehaviorType behaviorType)
        {

            this.npcType = npcType;
            AIBehavior().behaviorType = behaviorType;
            
        }
        public IEnumerator Decay()
        {
            yield return new WaitForSeconds(5);
            disappeared = true;
            Destroy(gameObject);

        }
        public bool CanAttack(GameObject target)
        {
            if(target == null) { return false; }
            if (target.GetComponent<EntityInfo>() == null) { return false; }
            var targetInfo = target.GetComponent<EntityInfo>();

            if(targetInfo.npcType == NPCType.Untargetable) { return false; }

            if(this.npcType == NPCType.Neutral) { return true; }


            if (targetInfo.npcType != this.npcType) { return true; }
            else { return false; }
        }
        public bool CanHelp(GameObject target)
        {
            if(target == null) { return false; }
            if (target.GetComponent<EntityInfo>() == null) { return false; }
            var targetInfo = target.GetComponent<EntityInfo>();
            
            if(targetInfo.npcType == NPCType.Untargetable) { return false; }

            if(npcType == NPCType.Neutral) { return true; }

            if (targetInfo.npcType != this.npcType)
            {
                return false;
            }
            else
            {
                return true;
            }

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
            foreach (Transform child in transform)
            {
                if (child.GetComponent<PlayerController>())
                {
                    return child.GetComponent<PlayerController>();
                }
            }
            return null;
        }
        public AIBehavior AIBehavior()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<AIBehavior>())
                {
                    return child.GetComponent<AIBehavior>();
                }
            }
            return null;
        }
        public ThreatManager ThreatManager()
        {
            if (AIBehavior() && AIBehavior().ThreatManager())
            {
                return AIBehavior().ThreatManager();
            }
            return null;
        }
        public LineOfSight LineOfSight()
        {
            if (AIBehavior() && AIBehavior().LineOfSight())
            {
                return AIBehavior().LineOfSight();
            }
            return null;
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


            foreach (Transform child in transform)
            {
                if (child.GetComponent<BuffsManager>())
                {
                    return child.GetComponent<BuffsManager>();
                }
            }

            return null;
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
        public AudioManager SoundEffect()
        {
            return GetComponentsInChildren<AudioManager>().First(manager => manager.mixerGroup == GMHelper.Mixer().SoundEffect);
        }

        public AudioManager VoiceEffect()
        {
            return GetComponentsInChildren<AudioManager>().First(manager => manager.mixerGroup == GMHelper.Mixer().Voice);
        }

        public EntitySpeech Speech { get { return GetComponentInChildren<EntitySpeech>(); } }
        public Inventory Inventory()
        {
            foreach (Transform child in transform)
            {
                if (child.GetComponent<Inventory>())
                {
                    return child.GetComponent<Inventory>();
                }
            }

            return null;
        }
        public CombatBehavior CombatBehavior()
        {
            if (AIBehavior() && AIBehavior().CombatBehavior())
            {
                return AIBehavior().CombatBehavior();
            }
            return null;
        }
        public ProgressBarsBehavior ProgressBar()
        {
            if (GraphicsInfo() && GraphicsInfo().ProgressBars())
            {
                return GraphicsInfo().ProgressBars();
            }

            return null;
        }
        public RoomInfo CurrentRoom()
        {
            Ray ray = new Ray(transform.position, Vector3.down);

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
            return GetComponentInChildren<ETaskHandler>();
        }
        public async void ShowEntity(bool val, bool hideGraphics = true, bool hideRenders = true, bool hideLights = true, bool destroys = false)
        {
            var tasks = new List<Task>();

            if(hideGraphics)
            {
                tasks.Add(ShowGraphics(val));
            }

            if(hideRenders)
            {
                tasks.Add(ShowRenders(val));
            }

            if(hideLights)
            {
                tasks.Add(ShowLights (val));
            }

            await Task.WhenAll(tasks);
        }
        async Task ShowRenders(bool val)
        {
            var renders = GetComponentsInChildren<Renderer>();
            foreach (var render in renders)
            {
                if (render == null)
                {
                    return;
                }
                render.enabled = val;
                await Task.Yield();
            }

        }
        async Task ShowGraphics(bool val)
        {
            var canvases = GetComponentsInChildren<Canvas>();

            foreach (var canvas in canvases)
            {
                if (canvas == null)
                {
                    return;
                }
                canvas.enabled = val;

                await Task.Yield();
            }
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
                light.enabled = val;
                await Task.Yield();
            }
        }
    }
}

