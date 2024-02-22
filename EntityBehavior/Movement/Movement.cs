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
using Architome.Events;

namespace Architome
{
    public enum eMovementEvent
    {
        OnStartMove,
        OnEndMove,
        OnChangePath,
        OnNewPathTarget,
        OnQuickMove,
        OnTryMove,
        OnSignificantMovementChange,
        OnCanMoveCheck,
        OnCanChangeTargetCheck,
    }

    public class MovementEventData 
    {
        public eMovementEvent trigger;
        public Movement sourceMovement;
        public EntityInfo sourceEntity;

        public Vector3 location;
        public Transform target;

        public MovementEventData(eMovementEvent trigger, Movement sourceMovement, Transform target)
        {
            this.trigger = trigger;
            this.sourceMovement = sourceMovement;

            this.target = target;
            this.location = target.position;
        }

        public MovementEventData(eMovementEvent trigger, Movement sourceMovement, Vector3 location)
        {
            this.trigger = trigger;
            this.sourceMovement = sourceMovement;
            this.location = location;
            this.target = sourceMovement.Target();
        }
    }

    public class Movement : EntityProp
    {
        // Start is called before the first frame update

        #region Common Data
        public GameObject entityObject;
        public GameObject location;

        private AIDestinationSetter destinationSetter;
        private AIPath path;
        public Rigidbody rigidBody;
        public RigidbodyConstraints originalConstraints;
        public AbilityManager abilityManager;
        public AIBehavior behavior;

        ArchEventHandler<eMovementEvent, MovementEventData> eventHandler => entityInfo.infoEvents.movementEvents;

        //public float baseMovementSpeed;
        //public float entityMovementSpeed;

        [Header("Speed")]
        public float speed;
        public float maxSpeed;
        public float baseOffsetMaxSpeed;
        public Dictionary<string, float> offsetSources { get; set; }
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
        #endregion
        //Events
        public Action<Movement> OnTryMove { get; set; }
        public UnityEvent OnTryMoveEvent { get; set; }
        public Action<Movement> OnChangePath { get; set; }
        public Action<Movement, Transform> OnArrival { get; set; }
        public Action<Movement, Transform> OnAway { get; set; }
        public Action<Movement, Transform, Transform> OnNewPathTarget { get; set; }

        //Event Triggers
        private bool isMovingChange;
        private bool hasArrivedCheck;
        private Transform currentPathTarget;
        Vector3 previousPosition { get; set; }

        #region Initiation

        public override void GetDependencies()
        {
            entityObject = entityInfo.gameObject;
            rigidBody = entityObject.GetComponent<Rigidbody>();

            originalConstraints = rigidBody.constraints;


            destinationSetter = entityInfo.AIDestinationSetter();
            path = entityInfo.Path();

            abilityManager = entityInfo.AbilityManager();

            entityInfo.OnChangeStats += OnChangeStats;
            entityInfo.OnLifeChange += OnLifeCheck;
            entityInfo.combatEvents.OnStatesChange += OnStatesChange;
            entityInfo.infoEvents.OnSignificantMovementChange += OnSignificantMovementChange;

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

            abilityHandler.Initiate(this);
            previousPosition = transform.position;
        }
        #endregion

        #region Event Loop

        void FixedUpdate()
        {
            //GetDependencies();
            HandleEvents();
            UpdateMetrics();
        }
        void HandleEvents()
        {
            if (destinationSetter == null) { return; }
            if (isMovingChange != isMoving)
            {
                isMovingChange = isMoving;
                if (isMoving)
                {
                    Invoke(new(eMovementEvent.OnStartMove, this, Target()));
                }
                else
                {
                    Invoke(new(eMovementEvent.OnEndMove, this, Target()));

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

        #endregion

        #region Event Listeners
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


        void OnChangeStats(EntityInfo entity)
        {
            SetOffSetMovementSpeed(entityInfo.stats.movementSpeed, entityInfo.stats);
        }
        void OnSignificantMovementChange(Vector3 newPosition)
        {
            StopMoving(true);
        }
        readonly HashSet<EntityState> immobilizedStates = new HashSet<EntityState>() {
            EntityState.Stunned,
            EntityState.Immobalized
        };
        public void OnStatesChange(List<EntityState> previous, List<EntityState> states)
        {
            if (!entityInfo.isAlive) { return; }

            var intersection = states.Intersect(immobilizedStates).ToList();

            if (intersection.Count > 0)
            {
                SetValues(false);
                return;
            }

            SetValues(true);
        }

        #endregion

        #region ArchEventHandler Overrides

        public Action AddListener(eMovementEvent trigger, Action<MovementEventData> action, Component listener) => eventHandler.AddListener(trigger, action, listener);
        public Action AddListener(eMovementEvent trigger, Action action, Component listener) => eventHandler.AddListener(trigger, action, listener);

        public Action AddListenerLimit(eMovementEvent trigger, Action<MovementEventData> action, Component listener, int amount = 1) => eventHandler.AddListenerLimit(trigger, action, listener, amount);
        public Action AddListenerLimit(eMovementEvent trigger, Action action, Component listener, int amount = 1) => eventHandler.AddListenerLimit(trigger, action, listener, amount);
        public void Invoke(MovementEventData eventData)
        {
            eventHandler.Invoke(eventData.trigger, eventData);
        }

        public bool InvokeCheck(MovementEventData eventData)
        {
            return eventHandler.InvokeCheck(eventData.trigger, eventData);
        }

        public bool InvokeCheck(MovementEventData eventData, bool target, LogicType logicType)
        {
            return eventHandler.InvokeCheck(eventData.trigger, eventData, target, logicType);
        }

        #endregion

        #region State Changers 

        public async void SetValues(bool val, float timer = 0f)
        {
            await Task.Delay((int)(timer * 1000));
            if (!val)
            {
                StopMoving();

            }
            canMove = val;
            destinationSetter.enabled = val;
            path.enabled = val;
            rigidBody.constraints = val ? originalConstraints : RigidbodyConstraints.FreezeAll;
        }
        public void RestrictMovements(bool restrict)
        {
            rigidBody.constraints = restrict ? RigidbodyConstraints.FreezeAll : originalConstraints;
        }
        public void SetWalk(bool val)
        {
            walking = val;
            UpdateOffset();
        }
        public void SetSpeed(float speed = 1f)
        {
            var baseMovementSpeed = walking ? GMHelper.WorldSettings().baseWalkSpeed : GMHelper.WorldSettings().baseMovementSpeed;
            path.maxSpeed = speed * baseMovementSpeed;
        }
        public Action SetOffSetMovementSpeed(float offsetAmount, object sourceObj)
        {
            offsetSources ??= new();

            var source = sourceObj.ToString();
            bool changed = false;

            if (offsetSources.ContainsKey(source))
            {
                if (offsetSources[source] != offsetAmount)
                {
                    offsetSources[source] = offsetAmount;
                    changed = true;
                }

            }
            else
            {
                offsetSources.Add(source, offsetAmount);
                changed = true;
            }

            if (changed)
            {
                UpdateOffset();
            }


            return () => {
                baseOffsetMaxSpeed -= offsetAmount;
                offsetSources.Remove(source);
                UpdateOffset();
            };
        }
        public void UpdateOffset()
        {
            baseOffsetMaxSpeed = 0f;
            offsetSources ??= new();

            Debugger.System(8940, $"Offset sources ({entityInfo}): {offsetSources.Count}");

            foreach(KeyValuePair<string, float> pairs in offsetSources)
            {
                Debugger.System(8941, $"Offset({entityInfo}) {pairs.Key}: {pairs.Value}");
                baseOffsetMaxSpeed += pairs.Value;
            }

            SetSpeed(baseOffsetMaxSpeed);

        }
        #endregion

        #region Movement Actions
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

        async public Task<bool> MoveToAsync(Vector3 location)
        {
            MoveTo(location);

            isMoving = true;

            while (isMoving || distanceFromTarget > 2f)
            {
                if (!Application.isPlaying) return false;
                if (this.location.transform.position != location) return false;
                if (!entityInfo.isAlive) return false;
                await Task.Yield();
            }

            return true;
        }
        async public Task<bool> MoveToAsyncLOS(Transform locationTransform)
        {
            if (!entityInfo.isAlive) return false;

            if (entityInfo.CanSee(locationTransform)) return true;

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
        async public Task<bool> MoveToLosRange(Transform locationTransform, float endReachDistance = 0f)
        {
            if (!entityInfo.isAlive) return false;

            var success = false;

            while (!success)
            {
                var inRange = await MoveToLosRange(locationTransform, endReachDistance);
                var hasLOS = await MoveToAsyncLOS(locationTransform);

                success = hasLOS && inRange;
            }

            return true;
        }
        void MoveTo(Vector3 location)
        {
            OnTryMove?.Invoke(this);
            Invoke(new(eMovementEvent.OnTryMove, this, location));
            OnTryMoveEvent?.Invoke();


            if (!entityInfo.CanMove()) return;
            
            if (!canMove) { return; }
            hasArrivedCheck = false;
            this.location.transform.position = location;
            destinationSetter.target = this.location.transform;
            realSpeedTimer = .25f;

            TriggerEvents();
            path.endReachedDistance = 0;
            OnChangePath?.Invoke(this);
            Invoke(new(eMovementEvent.OnChangePath, this, location));

        }
        void MoveTo(Transform locationTransform,  float endReachDistance = 0f)
        {

            Invoke(new(eMovementEvent.OnTryMove, this, locationTransform));

            if (!entityInfo.CanMove())
            {
                return;
            }
            path.endReachedDistance = endReachDistance;
            hasArrivedCheck = !hasArrived;
            isMovingChange = true;
            realSpeedTimer = .25f;

            if (destinationSetter.target != locationTransform)
            {
                destinationSetter.target = locationTransform;
            }

        }
        void MoveTo(Vector3 location, float endReachDistance = 0f)
        {
            if(!entityInfo.isAlive) return;
            path.endReachedDistance = endReachDistance;

            hasArrived = !hasArrived;

            realSpeedTimer = .25f;

            isMovingChange = true;
            this.location.transform.position = location;
            destinationSetter.target = this.location.transform;


        }
        public void StopMoving(bool targetSelf = false)
        {
            if (targetSelf && entityObject)
            {
                destinationSetter.target = entityObject.transform;
                MoveTo(entityObject.transform, float.PositiveInfinity);
            }

            if (path == null) return;

            path.endReachedDistance = float.PositiveInfinity;
        }
        public async Task StopMovingAsync(bool targetSelf = false)
        {
            StopMoving(targetSelf);
            while (isMoving)
            {
                StopMoving(targetSelf);
                await Task.Yield();
            }
        }
        #endregion

        #region Functional Properties
        public bool HasReachedTarget(float minimumDistance = 1f)
        {
            return distanceFromTarget <= minimumDistance + endReachDistance;
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
        public bool CanMove(MovementEventData eventData)
        {
            if (!canMove) return false;
            if (!entityInfo.isAlive) return false;

            return InvokeCheck(eventData, false, LogicType.NotExists);
        }
        public bool CanChangeTarget(MovementEventData eventData)
        {
            return InvokeCheck(eventData, false, LogicType.NotExists);
        }

        public bool TargetIsMovementLocation(GameObject target)
        {
            return target == location;
        }

        #endregion

        async public Task<Transform> NextPathTarget()
        {
            var currentTarget = destinationSetter.target;

            while (destinationSetter.target == currentTarget)
            {
                await Task.Yield();
            }

            return destinationSetter.target;
        }
        public void TriggerEvents()
        {
            hasArrivedCheck = !hasArrived;
            isMovingChange = !isMoving;
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
        public GameObject Location()
        {
            if (location)
            {
                return location;
            }
            return null;
        }
    }

    #region AbilityHandler

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

            abilities.OnChannelStart += OnChannelStart;
            abilities.OnAbilityStart += OnAbilityStart;

        }

        async void OnAbilityStart(AbilityInfo ability)
        {
            if (!ability.cantMoveWhenCasting) return;
            WarnDepricated(ability);
            await movement.StopMovingAsync();
            entity.infoEvents.OnCanMoveCheck += HandleMoveCheck;

            await ability.EndActivation();

            entity.infoEvents.OnCanMoveCheck -= HandleMoveCheck;


            void HandleMoveCheck(EntityInfo entity, List<bool> checks)
            {
                checks.Add(false);
            }
        }

        void OnCastStart(AbilityInfo ability)
        {
            if (!ability.cancelCastIfMoved) return;
            WarnDepricated(ability);
            movement.StopMoving();
        }
        void OnChannelStart(AbilityInfo ability, AugmentChannel augment)
        {
            if (augment.cancelChannelOnMove)
            {
                WarnDepricated(ability);
                movement.StopMoving();
            }
        }

        void WarnDepricated(AbilityInfo ability)
        {
            Debugger.System(2412, $"{ability} needs to add augment of type {typeof(AugmentMovement)}) from {entity}");
            Debugger.Error(5045, $"{ability} needs to add augment of type {typeof(AugmentMovement)}");
        }

        #endregion

    }

}
