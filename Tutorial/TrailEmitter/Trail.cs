using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

namespace Architome.Tutorial
{
    [RequireComponent(typeof(AIPath))]
    [RequireComponent(typeof(AIDestinationSetter))]
    [RequireComponent(typeof(Seeker))]
    public class Trail : MonoBehaviour
    {
        public AIDestinationSetter destinationSetter;
        [SerializeField] Transform currentTarget;

        bool destroyed;
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        public void SetTarget(Transform target)
        {
            var catalystManager = CatalystManager.active;

            transform.SetParent(catalystManager.transform);
            destinationSetter.target = target;
            currentTarget = target;
        }

        private void OnTriggerEnter(Collider other)
        {
            HandleReachedDestination(other.transform);
        }

        private void OnCollisionEnter(Collision collision)
        {
            HandleReachedDestination(collision.transform);
        }

        void HandleReachedDestination(Transform target)
        {
            if (target != currentTarget) return;
            if (destroyed) return;
            destroyed = true;
            foreach (var particle in GetComponentsInChildren<ParticleSystem>())
            {
                particle.Stop(true);
            }



            ArchAction.Delay(() => {
                if (destroyed) return;
                Destroy(gameObject);

            }, 2f);
        }



        public void DestroySelf(float delay)
        {
            ArchAction.Delay(() => {
                if (destroyed) return;
                destroyed = true;
                foreach (var particle in GetComponentsInChildren<ParticleSystem>())
                {
                    particle.Stop(true);
                }

                ArchAction.Delay(() => {
                    Destroy(gameObject);
                }, 2f);
            }, delay);
        }

    }
}
