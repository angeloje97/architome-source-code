using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Architome.Enums;
using UnityEngine.UI;
using System;
using System.Linq;


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
        [Range(0, 1)]
        public float releasePercent;


        public bool isCataling;
        public AbilityType catalyingAbilityType;

        public EntityInfo entityInfo;
        public AbilityInfo abilityInfo;

        private int ticks;
        public float currentRange;
        public float liveTime;
        public float distanceFromTarget;

        public List<EntityInfo> enemiesHit;
        public List<EntityInfo> alliesHealed;
        public List<EntityInfo> alliesAssisted;
        public EntityInfo lastTargetHit;

        public float value { get { return metrics.value; } set { metrics.value = value; } }
        public bool requiresLockOnTarget { get; set; }
        public GameObject target { get { return metrics.target; } set { metrics.target = value; } }
        public Vector3 location { get { return metrics.location; } set { metrics.location = value; } }

        [Serializable]
        public struct Metrics
        {
            [Serializable]
            public struct AccelerationBenchmark
            {
                [Range(0, 1)]
                public float benchmark;
                [Range(0, 1)]
                public float smoothness;
            }

            public GameObject target;

            public bool lockOn;

            public Vector3 location, startingLocation, startDirectionRange, currentPosition;

            public float value, startingHeight, currentRange, liveTime, inertia, inertiaFallOff, intervals;

            public int ticks;

            [Header("Growing Properties")]

            public Vector3 growthDirection;

            public float maxGrowth, growthSpeed, startSpeed;

            public float startDistance;


            [Header("Change of Speed")]

            public bool stops;

            public List<AccelerationBenchmark> accelBenchmarks;


            public Vector3 targetLocation
            {
                get
                {
                    return lockOn ? target.transform.position : location;
                }
            }
        }

        public Metrics metrics;
        [SerializeField] CatalystKinematics kinematics;

        [Header("Catalyst Set Values")]
        public DamageType damageType;
        public float range;
        public float castTime;
        public float speed;
        public CatalystType catalystType;

        [Serializable]
        public class CatalystEffects
        {
            [Serializable]
            public struct Catalyst
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
            public struct Ability
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
            

            public BodyPart startingBodyPart;

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
        public AudioSource audioSource;
        public Vector3 startPosition;
        public bool isDestroyed;

        //Events
        public Action<GameObject> OnDamage;
        public Action<GameObject> OnAssist;
        public Action<GameObject> OnHeal;
        public Action<GameObject> OnHit { get; set; }
        public Action<GameObject> OnWrongTarget;
        public Action<GameObject> OnDeadTarget { get; set; }
        public Action<GameObject, GameObject> OnNewTarget;
        public Action<CatalystInfo> OnReturn;
        public Action<CatalystInfo> OnCantFindEntity;
        public Action<CatalystInfo, int> OnTickChange;
        public Action<CatalystInfo, GameObject> OnCloseToTarget;
        public Action<CatalystInfo, GameObject> OnWrongTargetHit { get; set; }
        public Action<GameObject, bool> OnPhysicsInteraction;
        public Action<CatalystKinematics> OnCatalystStop;
        public Action<CatalystKinematics> OnCatalystMaxSpeed;
        public Action<GameObject> OnIntercept;
        public Action<CatalystInfo> OnInterval;

        public Action<CatalystInfo, CatalystInfo> OnCatalingRelease { get; set; }
        public Action<CatalystDeathCondition> OnCatalystDestroy { get; set; }

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

                GetMetrics();
            }

            if (GetComponent<AudioSource>() == null)
            {
                gameObject.AddComponent<AudioSource>();
                audioSource = GetComponent<AudioSource>();
            }
            else
            {
                audioSource = GetComponent<AudioSource>();
            }

            void GetMetrics()
            {
                location = abilityInfo.locationLocked;


                if (abilityInfo.targetLocked)
                {
                    target = abilityInfo.targetLocked;
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

                if (abilityInfo.bounce.enable)
                {
                    gameObject.AddComponent<CatalystBounce>();
                }

                if (abilityInfo.splash.enable)
                {
                    gameObject.AddComponent<CatalystSplash>();
                }

                if (abilityInfo.returns)
                {
                    gameObject.AddComponent<CatalystReturn>();

                }

                if (abilityInfo.buffProperties.selfBuffOnDestroy) gameObject.AddComponent<CatalystBuffOnDestroy>();

                if (abilityInfo.summoning.enabled) gameObject.AddComponent<CatalystSummon>();

                if (abilityInfo.cataling.enable && abilityInfo.cataling.catalyst) gameObject.AddComponent<CatalingHandler>();

                requiresLockOnTarget = abilityInfo.requiresLockOnTarget;
                ticks = abilityInfo.ticksOfDamage;
            }

            void HandleCatalyng()
            {
                if (!isCataling) { return; }

                var catalingType = abilityInfo.cataling.catalingType;

                if (catalingType == AbilityType.LockOn)
                {
                    gameObject.AddComponent<CatalystLockOn>();

                }

                value = abilityInfo.value * abilityInfo.cataling.valueContribution;

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
            SpawnBodyPart();
            kinematics.SetKinematics(this);

        }

        void SpawnBodyPart()
        {
            if (isCataling) return;
            if (entityObject == target) return;
            if (effects.startingBodyPart == BodyPart.Root) return;
            if (abilityInfo.abilityType == AbilityType.Spawn) return;

            var bodyPart = entityInfo.GetComponentInChildren<CharacterBodyParts>();

            if (bodyPart == null) return;

            var trans = bodyPart.BodyPartTransform(effects.startingBodyPart);

            
            transform.position = trans.position;
            transform.LookAt(location);
        }

        public void OnTriggerEnter(Collider other)
        {
            OnPhysicsInteraction?.Invoke(other.gameObject, true);
        }

        public void OnTriggerExit(Collider other)
        {
            OnPhysicsInteraction?.Invoke(other.gameObject, false);
        }

        void SpawnCatalystAudio()
        {
            var catalystPrefab = CatalystManager.active.CatalystAudioManager();

            if (catalystPrefab)
            {
                catalystPrefab.GetComponent<CatalystAudio>().Activate(this);
            }
        }
        void Update()
        {
            UpdateMetrics();
            HandleEvents();
        }


        public void UpdateMetrics()
        {
            metrics.currentRange = V3Helper.Distance(metrics.startingLocation, transform.position);
            
            liveTime += Time.deltaTime;

            if (target)
            {
                distanceFromTarget = V3Helper.Distance(target.transform.position, transform.position);
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

            if (abilityInfo.bounce.enable)
            {
                GetComponent<CatalystBounce>().LookForNewTarget();
            }
        }
        public List<GameObject> EntitiesWithinRadius(float radius = 0f)
        {
            if (radius == 0) radius = range;

            List<GameObject> entities = new List<GameObject>();

            var structureLayer = GMHelper.LayerMasks().structureLayerMask;
            var entityLayer = GMHelper.LayerMasks().entityLayerMask;

            Collider[] collisions = Physics.OverlapSphere(transform.position, radius, entityLayer)
                                            .OrderBy(entity => V3Helper.Distance(entity.transform.position, transform.position))
                                            .ToArray();

            foreach (var i in collisions)
            {
                var direction = V3Helper.Direction(i.transform.position, transform.position);
                var distance = V3Helper.Distance(i.transform.position, transform.position);


                if (abilityInfo.requiresLineOfSight)
                {
                    if (!Physics.Raycast(transform.position, direction, distance, structureLayer))
                    {
                        entities.Add(i.gameObject);
                    }
                }
                else
                {
                    entities.Add(i.gameObject);
                }

            }

            return entities.Where(entity => entity.GetComponent<EntityInfo>() != null).ToList();
        }
        public List<GameObject> AlliesWithinRange()
        {
            var allies = new List<GameObject>();

            foreach (GameObject ally in EntitiesWithinRadius())
            {
                if (abilityInfo.entityInfo.CanHelp(ally))
                {
                    allies.Add(ally);
                }
            }

            return allies;
        }
        public List<GameObject> EnemiesWithinRange()
        {
            var enemies = new List<GameObject>();

            foreach (var i in EntitiesWithinRadius())
            {
                if (abilityInfo.entityInfo.CanAttack(i))
                {
                    enemies.Add(i);
                }
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
        public void IncreaseTicks(bool triggerEvent = true)
        {
            ticks++;
            if (triggerEvent)
            {
                OnTickChange?.Invoke(this, ticks);
            }
        }
        public int Ticks()
        {
            return ticks;
        }
    }

}

