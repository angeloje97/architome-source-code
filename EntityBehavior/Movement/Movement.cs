using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using System;
using Architome;
using Architome.Enums;
using System.Threading.Tasks;
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

    public float baseMovementSpeed;
    public float entityMovementSpeed;

    [Header("Metrics")]
    public float speed;
    public float maxSpeed;
    public float endReachDistance;
    public Vector3 velocity;
    public bool isMoving;
    public bool canMove = true;
    public bool hasArrived;

    //Events
    public Action<Movement> OnStartMove;
    public Action<Movement> OnEndMove;
    public Action<Movement> OnTryMove;
    public Action<Movement> OnChangePath;
    public Action<Movement, Transform> OnArrival;
    public Action<Movement, Transform> OnAway;
    public Action<Movement, Transform, Transform> OnNewPathTarget;

    //Event Triggers
    private bool isMovingChange;
    private bool hasArrivedCheck;
    private Transform currentPathTarget;
    public void GetDependencies()
    {
        if (GetComponentInParent<EntityInfo>())
        {
            entityInfo = GetComponentInParent<EntityInfo>();
            entityObject = entityInfo.gameObject;
            rigidBody = entityObject.GetComponent<Rigidbody>();

            originalConstraints = rigidBody.constraints;

            if(entityInfo.AIDestinationSetter())
            {
                destinationSetter = entityInfo.AIDestinationSetter();
            }

            if(entityInfo.Path())
            {
                path = entityInfo.Path();

                if(GMHelper.WorldSettings())
                {
                    baseMovementSpeed = GMHelper.WorldSettings().baseMovementSpeed;
                    path.maxSpeed = baseMovementSpeed;
                }
            }

            if(entityInfo.AbilityManager())
            {
                abilityManager = entityInfo.AbilityManager();
            }

            entityInfo.OnLifeChange += OnLifeCheck;
            entityInfo.OnStateChange += OnStateChange;
        }

        if(destinationSetter.target == null)
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


        if(behavior == null && entityInfo && entityInfo.AIBehavior())
        {
            behavior = entityInfo.AIBehavior();
        }

        
    }
    void Start()
    {
        GetDependencies();
    }
    void Update()
    {
        //GetDependencies();
        UpdateMetrics();
        HandleEvents();
    }
    public void OnLifeCheck(bool isAlive)
    {
        if(isAlive)
        {
            SetValues(isAlive, 1f);
        }
        else
        {
            SetValues(isAlive);
        }

        if(isAlive)
        {
            MoveTo(entityObject.transform);
            MoveTo(entityObject.transform.position);
        }
        else
        {
            destinationSetter.target = entityObject.transform;
        }
    }
    public void OnStateChange(EntityState previous, EntityState current)
    {
        if (!entityInfo.isAlive) { return; }
        var immobilizedStates = new List<EntityState>()
        {
            EntityState.Stunned,
            EntityState.Immobalized
        };

        if(immobilizedStates.Contains(current))
        {
            SetValues(false);
            return;
        }

        SetValues(true);
    }
    public async void SetValues(bool val, float timer = 0f)
    {
        await Task.Delay((int)(timer * 1000));
        canMove = val;
        destinationSetter.enabled = val;
        path.enabled = val;
        rigidBody.constraints = val ? originalConstraints : RigidbodyConstraints.FreezeAll;
    }
    void HandleEvents()
    {
        if(destinationSetter == null) { return; }
        if(isMovingChange != isMoving)
        {
            isMovingChange = isMoving;
            if(isMoving)
            {
                OnStartMove?.Invoke(this);
            }
            else
            {
                OnEndMove?.Invoke(this);

            }
        }
        if(hasArrived != hasArrivedCheck)
        {
            hasArrivedCheck = hasArrived;
            if(hasArrived)
            {
                OnArrival?.Invoke(this, destinationSetter.target);
            }
            else
            {
                OnAway?.Invoke(this, destinationSetter.target);
            }
            
        }
        if(currentPathTarget != destinationSetter.target)
        {
            OnNewPathTarget?.Invoke(this, currentPathTarget, destinationSetter.target);
            currentPathTarget = destinationSetter.target;
        }
    }
    public void UpdateMetrics()
    {
        if(path == null) { return; }

        speed = path.desiredVelocity.magnitude;
        maxSpeed = path.maxSpeed;
        endReachDistance = path.endReachedDistance;
        velocity = path.desiredVelocity;
        isMoving = velocity.magnitude > 1;

        var ySpeed = rigidBody.velocity.y;
        rigidBody.velocity = new Vector3(0, ySpeed, 0);

        if(entityInfo.stats.movementSpeed != entityMovementSpeed)
        {
            entityMovementSpeed = entityInfo.stats.movementSpeed;
            path.maxSpeed = entityMovementSpeed * baseMovementSpeed;
        }

        if(destinationSetter.target)
        {
            hasArrived = Vector3.Distance(destinationSetter.target.transform.position, entityObject.transform.position) <= path.endReachedDistance + 2f;
        }
        
    }
    public void MoveTo(Vector3 location)
    {
        if(!entityInfo.isAlive) { return; }
        OnTryMove?.Invoke(this);
        if(!canMove) { return; }
        this.location.transform.position = location;
        destinationSetter.target = this.location.transform;
        path.endReachedDistance = 0;
        OnChangePath?.Invoke(this);
    }
    public void MoveTo(Transform locationTransform, float endReachDistance = 0f)
    {
        if (!entityInfo.isAlive) { return; }
        path.endReachedDistance = endReachDistance;

        if(destinationSetter.target != locationTransform)
        {
            destinationSetter.target = locationTransform;
        }
    }
    public Vector3 TargetPosition()
    {
        if(destinationSetter.target)
        {
            return destinationSetter.target.position;
        }

        return new Vector3();
    }
    public void StopMoving()
    {
        path.endReachedDistance = float.PositiveInfinity;
    }
    public GameObject Location()
    {
        if(location)
        {
            return location;
        }
        return null;
    }
}
