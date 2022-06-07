using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;
using System.Linq;
using Architome.Enums;
using System.Threading.Tasks;
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

        [Header("Metrics")]
        public float speed;
        public float maxSpeed;
        public float endReachDistance;
        public float distanceFromTarget;
        public Vector3 velocity;
        public bool isMoving;
        public bool canMove = true;
        public bool hasArrived;
        public bool walking;

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

        //Event Triggers
        private bool isMovingChange;
        private bool hasArrivedCheck;
        private Transform currentPathTarget;
        Vector3 previousPosition;
        public void GetDependencies()
        {
            if (GetComponentInParent<EntityInfo>())
            {
                entityInfo = GetComponentInParent<EntityInfo>();
                entityObject = entityInfo.gameObject;
                rigidBody = entityObject.GetComponent<Rigidbody>();

                originalConstraints = rigidBody.constraints;

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

        }
        void Update()
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
            }
        }

        void OnChangeStats(EntityInfo entity)
        {
            UpdateMovementSpeed();
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

            speed = path.desiredVelocity.magnitude;
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
        public void MoveTo(Vector3 location)
        {
            if (!entityInfo.isAlive) { return; }
            OnTryMove?.Invoke(this);
            OnTryMoveEvent?.Invoke();
            if (!canMove) { return; }
            hasArrivedCheck = false;
            this.location.transform.position = location;
            destinationSetter.target = this.location.transform;

            TriggerEvents();
            path.endReachedDistance = 0;
            OnChangePath?.Invoke(this);
        }
        public void MoveTo(Transform locationTransform, float endReachDistance = 0f)
        {
            if (!entityInfo.isAlive) { return; }
            path.endReachedDistance = endReachDistance;

            hasArrivedCheck = !hasArrived;
            isMovingChange = true;

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
                MoveTo(entityObject.transform);
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
        public void Initiate(Movement movement)
        {
            if (movement.entityInfo == null) return;

            this.movement = movement;
            entity = movement.entityInfo;

            if (entity == null) return;

            abilities = entity.AbilityManager();

            if (abilities == null) return;

            abilities.OnCastStart += OnCastStart;
            abilities.OnCastEnd += OnCastEnd;
            abilities.OnChannelStart += OnChannelStart;
            abilities.OnChannelEnd += OnChannelEnd;


        }

        

        void OnCastStart(AbilityInfo ability)
        {
            if (ability.cancelCastIfMoved)
            {
                movement.StopMoving();
            }
        }

        void OnCastEnd(AbilityInfo ability)
        {

        }

        void OnChannelStart(AbilityInfo ability)
        {
            if (ability.channel.cancelChannelOnMove)
            {
                movement.StopMoving();
            }
        }

        void OnChannelEnd(AbilityInfo ability)
        {

        }
    }

}
