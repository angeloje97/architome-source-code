using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Threading.Tasks;


namespace Architome
{
    [RequireComponent(typeof(CatalystDeathCondition))]
    [RequireComponent(typeof(CatalystHit))]
    [RequireComponent(typeof(CatalystFXHandler))]
    public class CatalystInfo : MonoBehaviour
    {
        
        // Start is called before the first frame update
        [Multiline] public string description;
        public GameObject entityObject;
        public Sprite catalystIcon;

        

        [Header("Catalyst Animations")]
        public List<int> animationSequence;
        public Vector2 catalystStyle;
        public bool useAlternateCasting;
        [Range(0, 1)]
        public float releasePercent;


        public bool isCataling;

        public EntityInfo entityInfo;
        public AbilityInfo abilityInfo;

        private int ticks;

        public List<EntityInfo> enemiesHit;
        public List<EntityInfo> alliesHealed;
        public List<EntityInfo> alliesAssisted;

        public AugmentProp.Restrictions restrictions;
        
        public EntityInfo lastTargetHit { get; set; }
        LayerMask entityLayer { get; set; }
        LayerMask structureLayer { get; set; }

        public float liveTime { get { return metrics.liveTime; } set { metrics.liveTime = value; } }
        public float distanceFromTarget { get { return metrics.distanceFromTarget; } set { metrics.distanceFromTarget = value; } }  
        public float value { get { return metrics.value; } set { metrics.value = value; } }
        public bool requiresLockOnTarget { get; set; }
        public GameObject target { get { return metrics.target; } set { metrics.target = value; } }
        public Vector3 location { get { return metrics.location; } set { metrics.location = value; } }

        public CMetrics metrics;
        [SerializeField] CatalystKinematics kinematics;

        [Header("Catalyst Set Values")]
        public DamageType damageType;
        public float range;
        public float castTime;
        public float speed;
        public bool requiresWeapon;
        public WeaponType weaponType;
        public CatalystType catalystType;

        [Serializable]
        public class CatalystEffects
        {
            [Serializable]
            public class Catalyst
            {
                public CatalystEvent playTrigger;
                public CatalystParticleTarget target;
                public BodyPart bodyPart;
                public RadiusType manifestRadius;
                public Vector3 offsetPosition, offsetScale, offsetRotation;
                public GameObject particleObj;
                public AudioClip audioClip;
                
                public bool loops;
                public bool looksAtTarget;

            }

            [Serializable]
            public class Ability
            {
                public AbilityEvent trigger;
                public GameObject particle;
                public BodyPart bodyPart;
                public BodyPart bodyPart2;
                public RadiusType manifestRadius;
                public CatalystParticleTarget target;
                public CatalystParticleTarget secondTarget;
                public bool looksAtTarget;
                public Vector3 offsetPosition, offsetScale, offsetRotation;

                [Header("Audio")]
                public AudioClip audioClip;
                public float volume = 1f;
                public bool loops;
            }

            [Serializable]
            public struct CatalystLight
            {
                public bool enable;
                public Color color;
                public float intensity;
                public float range;
            }


            public List<Catalyst> catalystsEffects;
            public List<Ability> abilityEffects;
            public CatalystLight light;
            

            public BodyPart startingBodyPart { get; set; }

            [Header("Transform Effects")]
            public bool collapseOnDeath;
            public bool growsOnAwake;
            public bool startFromGround;



            public float MaxCatalystDuration()
            {
                var max = 0f;

                foreach (var effect in catalystsEffects)
                {
                    var particle = effect.particleObj;
                    var audioClip = effect.audioClip;

                    if (particle)
                    {
                        if (particle.GetComponent<ParticleSystem>().main.duration > max)
                        {
                            max = particle.GetComponent<ParticleSystem>().main.duration;
                        }
                    }

                    if (audioClip)
                    {
                        if (audioClip.length > max)
                        {
                            max = audioClip.length;
                        }
                    }
                }

                return max;
            }
        }

        public CatalystEffects effects;
        public AudioManager catalystAudio;
        public Vector3 startPosition;
        public bool isDestroyed;

        //Events
        public Action<CatalystInfo, EntityInfo> OnDamage;
        public Action<CatalystInfo, EntityInfo> OnAssist;
        public Action<CatalystInfo, EntityInfo> OnHeal;

        public Action<CatalystInfo, Collider> OnStructureHit;
        public Action<CatalystInfo, EntityInfo> OnHit { get; set; }
        public Action<GameObject> OnWrongTarget;
        public Action<GameObject> OnDeadTarget { get; set; }
        public Action<GameObject, GameObject> OnNewTarget;
        public Action<CatalystInfo> OnReturn;
        public Action<CatalystInfo> OnCantFindEntity;
        public Action<CatalystInfo, int> OnTickChange;
        public Action<CatalystInfo, GameObject> OnCloseToTarget;
        public Action<CatalystInfo, GameObject> OnWrongTargetHit { get; set; }
        public Action<CatalystInfo, Collider, bool> OnCatalystTrigger;
        public Action<CatalystKinematics> OnCatalystStop;
        public Action<CatalystKinematics> OnCatalystMaxSpeed;
        public Action<GameObject> OnIntercept;
        public Action<CatalystInfo> OnInterval;

        public Action<CatalystInfo, CatalystInfo> OnCatalingRelease { get; set; }
        public Action<CatalystDeathCondition> OnCatalystDestroy { get; set; }

        public Action<CatalystHit, EntityInfo> OnCanHitCheck;

        private GameObject targetCheck;

        public void GetDependencies()
        {
            if (abilityInfo)
            {
                if (abilityInfo.entityObject)
                {
                    entityObject = abilityInfo.entityObject;
                    entityInfo = abilityInfo.entityInfo;
                }

                restrictions = abilityInfo.restrictions;


                if (damageType != DamageType.True)
                {
                    if (abilityInfo.coreContribution.wisdom > abilityInfo.coreContribution.strength + abilityInfo.coreContribution.dexterity)
                    {
                        damageType = DamageType.Magical;
                    }
                    else
                    {
                        damageType = DamageType.Physical;
                    }
                }

                entityLayer = GMHelper.LayerMasks().entityLayerMask;
                structureLayer = GMHelper.LayerMasks().structureLayerMask;

                GetMetrics();
            }

            void GetMetrics()
            {
                location = abilityInfo.locationLocked;


                if (abilityInfo.targetLocked)
                {
                    target = abilityInfo.targetLocked.gameObject;
                }

                metrics.startingHeight = V3Helper.HeightFromGround(transform.position, LayerMasksData.active.walkableLayer);

                metrics.startingLocation = transform.position;
                metrics.startDistance = Vector3.Distance(metrics.targetLocation, transform.position);

                CalculateValue();
            }

            void CalculateValue()
            {
                if (abilityInfo.nullifyDamage)
                {
                    value = 0;
                    return;
                }

                value = abilityInfo.value;
            }
        }
        public void HandleComponents()
        {
            if (abilityInfo)
            {
                HandleCatalyst();
                HandleCatalyng();
            }

            void HandleCatalyst()
            {
                if (isCataling) return;

                if (abilityInfo.abilityType == AbilityType.LockOn)
                {
                    gameObject.AddComponent<CatalystLockOn>();
                    metrics.lockOn = true;
                }

                if (abilityInfo.abilityType == AbilityType.Use)
                {
                    var catalystUse = gameObject.AddComponent<CatalystUse>();
                    catalystUse.abilityInfo = abilityInfo;
                    catalystUse.catalystInfo = this;
                }


                if (abilityInfo.returns)
                {
                    gameObject.AddComponent<CatalystReturn>();

                }


                //if (abilityInfo.summoning.enabled) gameObject.AddComponent<CatalystSummon>();

                //if (abilityInfo.cataling.enable && abilityInfo.cataling.catalyst) gameObject.AddComponent<CatalingHandler>();

                requiresLockOnTarget = abilityInfo.requiresLockOnTarget;
                ticks = abilityInfo.ticksOfDamage;
            }

            void HandleCatalyng()
            {
                if (!isCataling) { return; }

                ticks = 1;
            }
        }
        void Awake()
        {
            startPosition = transform.position;
            GetDependencies();
            HandleComponents();
            transform.SetParent(CatalystManager.active.transform);
        }
        private void Start()
        {
            SpawnCatalystAudio();
            kinematics.SetKinematics(this);
            metrics.Initialize(this);
            

        }

        void Update()
        {
            //UpdateMetrics();
            metrics.Update();
            kinematics.Update();
            HandleEvents();
        }
        
        //void SpawnBodyPart()
        //{
        //    return;
        //    if (isCataling) return;
        //    if (entityObject == target) return;
        //    if (effects.startingBodyPart == BodyPart.Root) return;
        //    if (abilityInfo.abilityType == AbilityType.Spawn) return;

        //    var bodyPart = entityInfo.GetComponentInChildren<CharacterBodyParts>();

        //    if (bodyPart == null) return;

        //    var trans = bodyPart.BodyPartTransform(effects.startingBodyPart);

            
        //    transform.position = trans.position;
        //    transform.LookAt(location);
        //}

        public void DisableKinematics()
        {
            kinematics.DisableKinematics();
        }
        public Sprite Icon()
        {
            return catalystIcon;
        }
        public void OnTriggerEnter(Collider other)
        {
            OnCatalystTrigger?.Invoke(this, other, true);
        }
        public void OnTriggerExit(Collider other)
        {
            OnCatalystTrigger?.Invoke(this, other, false);
        }
        void SpawnCatalystAudio()
        {
            var catalystPrefab = CatalystManager.active.CatalystAudioManager();
            

            if (catalystPrefab)
            {
                catalystPrefab.GetComponent<CatalystAudio>().Activate(this);
            }
        }
        public void HandleEvents()
        {
            if (targetCheck != target)
            {
                OnNewTarget?.Invoke(targetCheck, target);
                targetCheck = target;
            }
        }
        public void HandleDeadTarget()
        {
            if (!abilityInfo.requiresLockOnTarget) { return; }
            if (target == null) return;

            var targetInfo = target.GetComponent<EntityInfo>();

            if (targetInfo.isAlive) { return; }

        }

        public void AddTarget(EntityInfo target)
        {
            enemiesHit.Add(target);
            alliesHealed.Add(target);
            alliesAssisted.Add(target);
        }

        public List<EntityInfo> EntitiesWithinRadius(float radius = 0f, bool requiresLineOfSight = true)
        {
            if (radius == 0) radius = range;

            var entities = new List<EntityInfo>();

            var entitiesCasted = Physics.OverlapSphere(transform.position, radius, entityLayer)
                                            .OrderBy(entity => V3Helper.Distance(entity.transform.position, transform.position));

            foreach (var entity in entitiesCasted)
            {
                var info = entity.GetComponent<EntityInfo>();
                if (info == null) continue;

                if (requiresLineOfSight)
                {
                    var direction = V3Helper.Direction(entity.transform.position, transform.position);
                    var distance = V3Helper.Distance(entity.transform.position, transform.position);
                    if (Physics.Raycast(transform.position, direction, distance, structureLayer)) continue;
                }

                entities.Add(info);

            }

            return entities;
        }
        public List<EntityInfo> AlliesWithinRange(float radius = 0f, bool requiresLineOfSight = true)
        {
            var allies = new List<EntityInfo>();

            foreach (var entity in EntitiesWithinRadius(radius, requiresLineOfSight))
            {
                if (abilityInfo.entityInfo.CanHelp(entity.gameObject))
                {
                    allies.Add(entity);
                }
            }

            return allies;
        }
        public List<EntityInfo> EnemiesWithinRange(float radius = 0f, bool requiresLineOfSight = true)
        {
            var enemies = new List<EntityInfo>();
            var catalystHit = GetComponent<CatalystHit>();
            if (catalystHit == null) return enemies;

            foreach (var i in EntitiesWithinRadius(radius, requiresLineOfSight))
            {
                if (!abilityInfo.entityInfo.CanAttack(i.gameObject)) continue;
                enemies.Add(i);
            }

            return enemies;
        }
        public List<GameObject> TargetableEntities(float radius, bool requiresLineOfSight = false)
        {
            var targetables = new List<GameObject>();
            var structureLayer = GMHelper.LayerMasks().structureLayerMask;
            var entityLayer = GMHelper.LayerMasks().entityLayerMask;

            var catalystHit = GetComponent<CatalystHit>();

            var collisions = Physics.OverlapSphere(transform.position, radius, entityLayer).OrderBy(entity => V3Helper.Distance(entity.transform.position, transform.position)).ToList();


            foreach (var collision in collisions)
            {
                if (collision.GetComponent<EntityInfo>() == null) continue;
                if (!catalystHit.CanHit(collision.GetComponent<EntityInfo>())) continue;

                var distance = V3Helper.Distance(collision.transform.position, transform.position);
                var direction = V3Helper.Direction(collision.transform.position, transform.position);

                if (requiresLineOfSight)
                {
                    if (!Physics.Raycast(transform.position, direction, distance, structureLayer))
                    {
                        targetables.Add(collision.gameObject);
                    }
                }
                else
                {
                    targetables.Add(collision.gameObject);
                }
            }
            

            return targetables;
        }
        public void ReduceTicks()
        {
            ticks--;
            OnTickChange?.Invoke(this, ticks);
        }

        public void DepleteTicks()
        {
            ticks = 0;
            OnTickChange?.Invoke(this, ticks);
        }
        public void IncreaseTicks(bool triggerEvent = true, int amount = 1)
        {
            ticks += amount;
            if (triggerEvent)
            {
                OnTickChange?.Invoke(this, ticks);
            }
        }
        public int Ticks()
        {
            return ticks;
        }

        public void AddEventAction(CatalystEvent eventTrigger, Action action)
        {
            switch (eventTrigger)
            {
                case CatalystEvent.OnAwake:
                    action();
                    break;
                case CatalystEvent.OnStop:
                    OnCatalystStop += (CatalystKinematics kinematics) => { action(); };
                    break;
                case CatalystEvent.OnAssist:
                    OnAssist += (CatalystInfo catalyst, EntityInfo target) => { action();};
                    break;
                case CatalystEvent.OnHarm:
                    OnDamage += (CatalystInfo catalyst, EntityInfo entity) => { action(); };
                    break;
                case CatalystEvent.OnHeal:
                    OnHeal += (CatalystInfo catalyst, EntityInfo target) => { action(); };
                    break;
                case CatalystEvent.OnHit:
                    OnHit += (CatalystInfo catalyst, EntityInfo target) => { action(); };
                    break;
                case CatalystEvent.OnInterval:
                    OnInterval += (CatalystInfo catalyst) => { action(); };
                    break;
                case CatalystEvent.OnDestroy:
                    if (isDestroyed) {
                        action();
                    }
                    else
                    {
                        OnCatalystDestroy += (CatalystDeathCondition condition) => { action(); };
                    }
                    break;
                case CatalystEvent.OnCatalingRelease:
                    OnCatalingRelease += (CatalystInfo catalyst, CatalystInfo cataling) => { action(); };
                    break;
            }
        }

        
    }

}

