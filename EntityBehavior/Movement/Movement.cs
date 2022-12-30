using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;
using System.Linq;
using Architome.Enums;
using System.Threading.Tasks;
using Pathfinding.RVO;

using UnityEngine.Events;

namespace Architome
{
    public class Movement : MonoBehaviour
    {
        // Start is called before the first frame update
        public GameObject entityObject;
        public GameObject location;

        public EntityInfo entityInfo;
        private AIDestinationSetter destinationSetter;
        private AIPath path;
        public Rigidbody rigidBody;
        public RigidbodyConstraints originalConstraints;
        public AbilityManager abilityManager;
        public AIBehavior behavior;


        //public float baseMovementSpeed;
        //public float entityMovementSpeed;

        [Header("Speed")]
        public float speed;
        public float maxSpeed;
        [Header("Metrics")]
        public float endReachDistance;
        public float distanceFromTarget;
        public Vector3 velocity;
        public bool isMoving;
        public bool canMove = true;
        public bool hasArrived;
        public bool walking;

        bool checkedPrevious;
        float realSpeedTimer;


        [Header("Entity Prop Handlers")]
        AbilityHandler abilityHandler;

        //Events
        public Action<Movement> OnStartMove;
        public Action<Movement> OnEndMove;
        public Action<Movement> OnTryMove;
        public UnityEvent OnTryMoveEvent;
        public Action<Movement> OnChangePath;
        public Action<Movement, Transform> OnArrival;
        public Action<Movement, Transform> OnAway;
        public Action<Movement, Transform, Transform> OnNewPathTarget;
        public Action<Movement> OnQuickMove;

        //Event Triggers
        private bool isMovingChange;
        private bool hasArrivedCheck;
        private Transform currentPathTarget;
        Vector3 previousPosition { get; set; }
        public void GetDependencies()
        {
            if (GetComponentInParent<EntityInfo>())
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;
                rigidBody = entityObject.GetComponent<Rigidbody>();

                originalConstraints = rigidBody.constraints;


                //if (rvoController == null)
                //{
                //    rvoController = entityInfo.gameObject.AddComponent<RVOController>();
                //}

                if (entityInfo.AIDestinationSetter())
                {
                    destinationSetter = entityInfo.AIDestinationSetter();
                }

                if (entityInfo.Path())
                {
                    path = entityInfo.Path();

                }

                if (entityInfo.AbilityManager())
                {
                    abilityManager = entityInfo.AbilityManager();
                }

                entityInfo.OnChangeStats += OnChangeStats;
                entityInfo.OnLifeChange += OnLifeCheck;
                entityInfo.combatEvents.OnStatesChange += OnStatesChange;
                entityInfo.infoEvents.OnSignificantMovementChange += OnSignificantMovementChange;
            }

            if (destinationSetter.target == null)
            {
                foreach (Transform child in transform)
                {
                    if (child.CompareTag("GameController"))
                    {
                        location = child.gameObject;
                        if (destinationSetter)
                        {
                            destinationSetter.target = location.transform;
                        }
                    }
                }
            }

            entityInfo.sceneEvents.OnTransferScene += OnTransferScene;


            if (behavior == null && entityInfo && entityInfo.AIBehavior())
            {
                behavior = entityInfo.AIBehavior();
            }


        }
        private void Awake()
        {
            GetDependencies();
        }
        void Start()
        {
            abilityHandler.Initiate(this);
            previousPosition = transform.position;

        }
        void FixedUpdate()
        {
            //GetDependencies();
            UpdateMetrics();
            HandleEvents();
        }
        public void OnTransferScene(string sceneName)
        {
            StopMoving(true);
        }
        public void OnLifeCheck(bool isAlive)
        {
            if (isAlive)
            {
                SetValues(isAlive, 1f);
            }
            else
            {
                SetValues(isAlive);
            }
            
            StopMoving(true);
        }
        public void OnStatesChange(List<EntityState> previous, List<EntityState> states)
        {
            if (!entityInfo.isAlive) { return; }
            var immobilizedStates = new List<EntityState>()
            {
                EntityState.Stunned,
                EntityState.Immobalized
            };

            var intersection = states.Intersect(immobilizedStates).ToList();

            if (intersection.Count > 0)
            {
                SetValues(false);
                return;
            }

            SetValues(true);
        }
        public async void SetValues(bool val, float timer = 0f)
        {
            await Task.Delay((int)(timer * 1000));
            StopMoving();
            canMove = val;
            destinationSetter.enabled = val;
            path.enabled = val;
            rigidBody.constraints = val ? originalConstraints : RigidbodyConstraints.FreezeAll;
        }
        public void RestrictMovements(bool restrict)
        {
            rigidBody.constraints = restrict ? RigidbodyConstraints.FreezeAll : originalConstraints;
        }
        void HandleEvents()
        {
            if (destinationSetter == null) { return; }
            if (isMovingChange != isMoving)
            {
                isMovingChange = isMoving;
                if (isMoving)
                {
                    OnStartMove?.Invoke(this);
                }
                else
                {
                    OnEndMove?.Invoke(this);

                }
            }
            if (hasArrived != hasArrivedCheck)
            {
                hasArrivedCheck = hasArrived;
                if (hasArrived)
                {
                    OnArrival?.Invoke(this, destinationSetter.target);
                }
                else
                {
                    OnAway?.Invoke(this, destinationSetter.target);
                }

            }
            if (currentPathTarget != destinationSetter.target)
            {
                OnNewPathTarget?.Invoke(this, currentPathTarget, destinationSetter.target);
                currentPathTarget = destinationSetter.target;
                entityInfo.SetTarget(destinationSetter.target);
                
            }
        }
        void OnChangeStats(EntityInfo entity)
        {
            UpdateMovementSpeed();
        }

        void OnSignificantMovementChange(Vector3 newPosition)
        {
            StopMoving(true);
        }
        void UpdateMovementSpeed()
        {
            var baseMovementSpeed = walking ? GMHelper.WorldSettings().baseWalkSpeed : GMHelper.WorldSettings().baseMovementSpeed;

            path.maxSpeed = entityInfo.stats.movementSpeed * baseMovementSpeed;
        }
        public void SetWalk(bool val)
        {
            walking = val;
            UpdateMovementSpeed();
        }
        public void UpdateMetrics()
        {
            if (path == null) { return; }
            if (destinationSetter == null) { return; }

            var desiredVelocity = path.desiredVelocity.magnitude;
            speed = desiredVelocity;

            var realSpeed = Vector3.Distance(previousPosition, transform.position) * (1 / Time.deltaTime);

            previousPosition = transform.position;

            if(realSpeedTimer >= 0f)
            {
                realSpeedTimer -= Time.deltaTime;
            }
            else
            {

                if(realSpeed > .50f)
                {
                    realSpeedTimer = .25f;
                }
                else
                {
                    speed = 0f;
                }
            }

            //if(Mathf.Abs(desiredVelocity - realSpeed) > 4f)
            //{
            //    if(realSpeed > maxSpeed){
            //        realSpeed = maxSpeed;
            //    }
            //    speed = realSpeed;
            //}
            //else
            //{
            //    speed = desiredVelocity;
            //}



            //if (!checkedPrevious)
            //{
            //    previousPosition = transform.position;
            //    checkedPrevious = true;
            //}
            //else
            //{
            //    checkedPrevious = false;
            //}


            maxSpeed = path.maxSpeed;
            endReachDistance = path.endReachedDistance;
            velocity = path.desiredVelocity;
            isMoving = velocity.magnitude > 1;

            var ySpeed = rigidBody.velocity.y;
            rigidBody.velocity = new Vector3(0, ySpeed, 0);


            if (destinationSetter.target)
            {
                distanceFromTarget = V3Helper.Distance(destinationSetter.target.position, entityObject.transform.position);
            }

            if (destinationSetter.target)
            {
                hasArrived = distanceFromTarget < endReachDistance + 2f;
            }

        }


        public bool HasReachedTarget(float minimumDistance = 1f)
        {
            return distanceFromTarget <= minimumDistance + endReachDistance;
        }
        async public Task<bool> MoveToAsync(Transform locationTransform, float endReachDistance = 0f)
        {
            if (!entityInfo.isAlive) return false;
            MoveTo(locationTransform, endReachDistance);
            isMoving = true;
            var target = locationTransform;

            await Task.Delay(62);

            while (isMoving || distanceFromTarget > endReachDistance + 2f)
            {
                if (!Application.isPlaying) return false;
                if (destinationSetter.target != target) return false;
                if (!entityInfo.isAlive) return false;

                await Task.Yield();
            }

            return true;
        }

        async public Task<bool> MoveToAsyncLOS(Transform locationTransform)
        {
            if (!entityInfo.isAlive) return false;
            MoveTo(locationTransform);

            isMoving = true;
            var target = locationTransform;

            await Task.Delay(62);

            while (isMoving)
            {
                if (!this) return false;

                if (destinationSetter.target != target) return false;
                if (!entityInfo.isAlive) return false;


                if (entityInfo.CanSee(locationTransform)) break;

                await Task.Delay(250);
            }

            return true;
        }
        async public Task<Transform> NextPathTarget()
        {
            var currentTarget = destinationSetter.target;

            while (destinationSetter.target == currentTarget)
            {
                await Task.Yield();
            }

            return destinationSetter.target;
        }
        public void MoveTo(Vector3 location)
        {
            if (!entityInfo.isAlive) { return; }
            OnTryMove?.Invoke(this);
            OnTryMoveEvent?.Invoke();
            if (!canMove) { return; }
            hasArrivedCheck = false;
            this.location.transform.position = location;
            destinationSetter.target = this.location.transform;
            realSpeedTimer = .25f;

            TriggerEvents();
            path.endReachedDistance = 0;
            OnChangePath?.Invoke(this);

        }
        public void MoveTo(Transform locationTransform,  float endReachDistance = 0f)
        {
            if (!entityInfo.isAlive) { return; }
            path.endReachedDistance = endReachDistance;
            hasArrivedCheck = !hasArrived;
            isMovingChange = true;
            realSpeedTimer = .25f;

            if (destinationSetter.target != locationTransform)
            {
                destinationSetter.target = locationTransform;
            }

        }
        public void MoveTo(Vector3 location, float endReachDistance = 0f)
        {
            if(!entityInfo.isAlive) return;
            path.endReachedDistance = endReachDistance;

            hasArrived = !hasArrived;

            realSpeedTimer = .25f;

            isMovingChange = true;
            this.location.transform.position = location;
            destinationSetter.target = this.location.transform;


        }
        public void TriggerEvents()
        {
            hasArrivedCheck = !hasArrived;
            isMovingChange = !isMoving;
        }
        public bool IsInRangeFromTarget()
        {
            if (destinationSetter.target == null)
            {
                return false;
            }

            var distanceFromTarget = DistanceFromTarget();
            var endReachDistance = path.endReachedDistance;

            if (distanceFromTarget <= endReachDistance + 3f)
            {
                return true;
            }


            return false;
        }
        public float DistanceFromTarget()
        {
            if (destinationSetter.target == null) { return float.PositiveInfinity; }

            return V3Helper.Distance(destinationSetter.target.position, entityObject.transform.position);
        }
        public Vector3 TargetPosition()
        {
            if (destinationSetter.target)
            {
                return destinationSetter.target.position;
            }

            return new Vector3();
        }
        public Transform Target()
        {
            if (destinationSetter)
            {
                return destinationSetter.target;
            }
            return null;
        }
        public void StopMoving(bool targetSelf = false)
        {
            if (targetSelf)
            {
                destinationSetter.target = entityObject.transform;
                MoveTo(entityObject.transform, float.PositiveInfinity);
            }

            path.endReachedDistance = float.PositiveInfinity;


        }
        public GameObject Location()
        {
            if (location)
            {
                return location;
            }
            return null;
        }
    }
    public struct AbilityHandler
    {
        public EntityInfo entity;
        public AbilityManager abilities;
        public Movement movement;

        public AbilityManager.Events abilityEvents
        {
            get
            {
                return entity.abilityEvents;
            }
        }
        public void Initiate(Movement movement)
        {
            if (movement.entityInfo == null) return;

            this.movement = movement;
            entity = movement.entityInfo;

            if (entity == null) return;

            abilities = entity.AbilityManager();

            if (abilities == null) return;


            abilityEvents.OnCastStart += OnCastStart;
            abilityEvents.OnCastEnd += OnCastEnd;

            //abilities.OnCastStart += OnCastStart;
            //abilities.OnCastEnd += OnCastEnd;
            abilities.OnChannelStart += OnChannelStart;
            abilities.OnChannelEnd += OnChannelEnd;
            abilities.OnTryAttackTarget += OnTryAttackTarget;


        }

        void OnTryAttackTarget(AbilityInfo attackAbility, EntityInfo target)
        {
            var currentlyCasting = abilities.currentlyCasting;

            if (currentlyCasting && currentlyCasting != attackAbility)
            {
                return;
            }

            if (attackAbility.CanCastAt(target))
            {
                var range = attackAbility.range;
                movement.MoveTo(target.transform, range);
                
            }
        }
        async void OnCastStart(AbilityInfo ability)
        {
            if (!ability.cancelCastIfMoved) return;
            var movementTimer = .25f;

            while (ability.isCasting)
            {
                await Task.Yield();
                movementTimer -= Time.deltaTime;

                if (movementTimer < 0) break;
                if (movement.isMoving)
                {
                    movement.StopMoving();
                }
            }
        }
        void OnCastEnd(AbilityInfo ability)
        {

        }
        void OnChannelStart(AbilityInfo ability, AugmentChannel augment)
        {
            if (augment.cancelChannelOnMove)
            {
                movement.StopMoving();
            }
        }
        void OnChannelEnd(AbilityInfo ability, AugmentChannel augment)
        {

        }
    }

}
