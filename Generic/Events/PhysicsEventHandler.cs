using JetBrains.Annotations;
using LootLabels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;

namespace Architome.Events
{
    public enum eCollisionEvent
    {
        OnTriggerEnter,
        OnTriggerExit,
        OnCollisionEnter,
        OnCollisionExit,
        OnLoSEnter,
        OnLosExit,
    }

    #region ColliderEventHandler
    public class PhysicsEventHandler : MonoBehaviour
    {
        #region Common Data

        ArchEventHandler<eCollisionEvent, CollisionEventData> eventHandler;

        [SerializeField] SphereCollider sphereCollider;

        [SerializeField] float radius;

        [SerializeField] LayerMask targetlayerMask;


        #endregion

        #region Initiation

        private void Awake()
        {
            eventHandler = new(this);
            UpdateSphereCollider();
        }
        void Start()
        {
            GetaDependencies();
            StartLineOfSight();
        }
        void GetaDependencies()
        {
            
        }
        #endregion

        #region General Update
        void Update()
        {
        
        }

        public bool IsValid(Collider collider)
        {
            return targetlayerMask.ContainsLayer(collider.gameObject.layer);
        }

        public bool IsValid(Collision collision)
        {
            return targetlayerMask.ContainsLayer(collision.gameObject.layer);
        }
        #endregion

        #region Line of Sight

        [SerializeField] UniqueList<GameObject> objectsInLoS;
        [SerializeField] UniqueList<GameObject> objectsInRadius;
        LayerMask obstructionLayerMask;

        void StartLineOfSight()
        {
            GetDependenciesLoS();
            HandleEventsLoS();
        }

        void GetDependenciesLoS()
        {
            objectsInLoS = new();
            objectsInRadius = new();
            obstructionLayerMask = LayerMasksData.active.structureLayerMask;
        }
        void HandleEventsLoS()
        {
            eventHandler.AddListener(eCollisionEvent.OnTriggerEnter, async (CollisionEventData data) => {
                HandleEnterRadius(data);

                await World.ActionInterval((float deltaTime) => {

                    if (!V3Helper.IsObstructed(transform.position, data.otherObject.transform.position, obstructionLayerMask))
                    {
                        HandleEnterLoS(data);
                    }
                    else
                    {
                        HandleExitLoS(data);
                    }

                    return objectsInRadius.Contains(data.otherObject);
                }, .25f, true);

            }, this);

            eventHandler.AddListener(eCollisionEvent.OnTriggerExit, (CollisionEventData data) => {
                HandleExitRadius(data);
                HandleExitLoS(data);
            }, this);
        }
        void HandleEnterRadius(CollisionEventData eventData)
        {
            objectsInRadius.Add(eventData.otherObject);
        }

        void HandleExitRadius(CollisionEventData eventData)
        {
            objectsInRadius.Remove(eventData.otherObject);
        }

        void HandleEnterLoS(CollisionEventData eventData)
        {
            var gameObject = eventData.otherObject;
            if (!objectsInLoS.Add(gameObject)) return;
            eventData.eventType = eCollisionEvent.OnLoSEnter;
            Invoke(eventData);
        }

        void HandleExitLoS(CollisionEventData eventData)
        {
            var gameObject = eventData.otherObject;
            if (!objectsInLoS.Remove(gameObject)) return;
            eventData.eventType = eCollisionEvent.OnLosExit;
            Invoke(eventData);
        }

        #endregion

        #region Event Handler Extension

        void Invoke(CollisionEventData data)
        {
            eventHandler.Invoke(data.eventType, data);
        }


        public Action AddListener(eCollisionEvent trigger, Action<CollisionEventData> action, Component listener)
        {
            return eventHandler.AddListener(trigger, action, listener);
        }

        public Action AddListenerLimit(eCollisionEvent eventType, Action action, Component listener, int count = 1)
        {
            return eventHandler.AddListenerLimit(eventType, (e) => { action(); }, listener, count);
        }

        #endregion

        #region Collider

        bool updatedCollider;

        void UpdateSphereCollider()
        {
            if (sphereCollider == null)
            {
                sphereCollider = GetComponent<SphereCollider>();
                if (sphereCollider == null)
                {
                    sphereCollider = gameObject.AddComponent<SphereCollider>();
                }
            }

            sphereCollider.radius = radius;

            updatedCollider = true;
        }

        public async void SetTrigger(bool isTrigger)
        {
            await UntilFinishUpdatingCollider();
            sphereCollider.isTrigger = isTrigger;
        }

        public async Task UntilFinishUpdatingCollider()
        {
            await ArchAction.WaitUntil(() => updatedCollider, true);
        }

        #endregion

        #region Events

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValid(other)) return;
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnTriggerEnter, this, other);
            Invoke(collisionEventData);
            
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValid(other)) return;
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnTriggerExit, this, other);
            Invoke(collisionEventData);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsValid(other)) return;
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnCollisionEnter, this, other);
            Invoke(collisionEventData);

        }

        private void OnCollisionExit(Collision other)
        {
            if (IsValid(other)) return;
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnCollisionExit, this, other);
            Invoke(collisionEventData);
        }

        #endregion

        #region Static Scope

        public static void HandleObject(GameObject gameObject, Action<PhysicsEventHandler> action, bool isTrigger = true)
        {
            var eventHandler = gameObject.GetComponent<PhysicsEventHandler>();

            if(eventHandler == null)
            {
                eventHandler = gameObject.AddComponent<PhysicsEventHandler>();
                eventHandler.SetTrigger(isTrigger);
            }

            action(eventHandler);
        }

        #endregion
    }
    #endregion

    #region CollisionEventData
    public class CollisionEventData
    {
        public eCollisionEvent eventType;
        public PhysicsEventHandler source;        
        public Collider otherCollider;
        public Collision otherCollision;
        public GameObject otherObject;

        public CollisionEventData(eCollisionEvent eventType, PhysicsEventHandler handler, Collider otherCollider)
        {
            this.eventType = eventType;
            source = handler;
            this.otherCollider = otherCollider;
            otherObject = otherCollider.gameObject;
        }

        public CollisionEventData(eCollisionEvent eventType, PhysicsEventHandler handler, Collision otherCollision)
        {
            this.eventType = eventType;
            source = handler;
            this.otherCollision = otherCollision;
            otherObject = otherCollision.gameObject;
        }
    }
    #endregion
}
