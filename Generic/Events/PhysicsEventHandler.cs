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
            StartEntityHandler();
        }

        #endregion

        // Update is called once per frame
        void Update()
        {
        
        }

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

        #region Entity

        [SerializeField] HashSet<EntityInfo> entitiesInRadius;
        [SerializeField] LayerMask entityLayer;

        public void StartEntityHandler()
        {
            entityLayer = LayerMasksData.active.entityLayerMask;
            entitiesInRadius = new();
            FindEntitiesAtStart();
            HandleEvents();
        }

        void FindEntitiesAtStart()
        {
            Entity.ProcessEntitiesInRange(transform.position, radius, entityLayer, (EntityInfo entity) => {
                HandleEntityEnter(entity);
            });
        }

        void HandleEvents()
        {

        }

        void HandleEntityEnter(EntityInfo entity)
        {
            if (entitiesInRadius.Contains(entity)) return;
            entitiesInRadius.Add(entity);
        }

        void HandleEntityExit(EntityInfo entity)
        {
            if (!entitiesInRadius.Contains(entity)) return;
            entitiesInRadius.Remove(entity);
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
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnTriggerEnter, this, other);
            Invoke(collisionEventData);
            
        }

        private void OnTriggerExit(Collider other)
        {
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnTriggerExit, this, other);
            Invoke(collisionEventData);
        }

        private void OnCollisionEnter(Collision other)
        {
            var collisionEventData = new CollisionEventData(eCollisionEvent.OnCollisionEnter, this, other);
            Invoke(collisionEventData);

        }

        private void OnCollisionExit(Collision other)
        {
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


        public CollisionEventData(eCollisionEvent eventType, PhysicsEventHandler handler, Collider otherCollider)
        {
            this.eventType = eventType;
            source = handler;
            this.otherCollider = otherCollider;
        }

        public CollisionEventData(eCollisionEvent eventType, PhysicsEventHandler handler, Collision otherCollision)
        {
            this.eventType = eventType;
            source = handler;
            this.otherCollision = otherCollision;
        }
    }
    #endregion
}
